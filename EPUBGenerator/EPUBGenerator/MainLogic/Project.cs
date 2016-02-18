using TTS;
using TTS.G2Ps;
using TTS.PhonemeConverters;
using TTS.Preprocessors;
using TTS.Synthesizers;
using eBdb.EpubReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Xml.Linq;
using Path = System.IO.Path;
using System.Linq;
using System.Diagnostics;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace EPUBGenerator.MainLogic
{
    enum ProjectStatus : int { None, Create, Edit, Cancel };
    class Project
    {
        public class ProgressUpdater
        {
            private int Counter;
            private int Total;
            private int Scale;
            public bool Cancelled { get; private set; }

            public ProgressUpdater(int total, int scale)
            {
                Counter = 0;
                Total = total;
                Scale = scale;
                CurrentProject.Worker.ReportProgress(0);
            }

            public void UpCount()
            {
                UpCount(1);
            }

            public void UpCount(int n)
            {
                Counter += n;
                CurrentProject.Worker.ReportProgress(Scale * Counter / Total);
                if (CurrentProject.Worker.CancellationPending)
                {
                    CurrentProject.DoWorkEvent.Cancel = true;
                    CurrentProject.Status = (int)ProjectStatus.Cancel;
                    Cancelled = true;
                }
            }
        }

        public static Project CurrentProject { get; set; }

        public String ProjectDirectory { get; private set; }
        public String EpubName { get; private set; }
        public String EpubPath { get; private set; }
        public BackgroundWorker Worker { get; private set; }
        public DoWorkEventArgs DoWorkEvent { get; private set; }
        public int Status { get; set; }

        private Epub EpubReader { get; set; }
        private List<Content> Contents { get; set; }

        private String PackageDir;
        private Dictionary<String, String> Dirs;

        #region Constructor
        public Project(String epubPath, String projDir, BackgroundWorker bw, DoWorkEventArgs e)
        {
            Status = (int)ProjectStatus.Create;
            Project.CurrentProject = this;

            EpubReader = new Epub(epubPath); // Use original epubPath to read data
            
            #region ------------- Create Project Directories & Main Subdirectories -------------
            Dirs = new Dictionary<String, String>();
            ProjectDirectory = projDir;
            PackageDir = EpubReader.GetOpfDirectory();

            // @"ProjDir\Resources"
            AddDirectory("Resources", Path.Combine(ProjectDirectory, "Resources"));
            // @"ProjDir\Resources\Package"
            AddDirectory("Package", Path.Combine(Dirs["Resources"], PackageDir));

            // @"ProjDir\Saves"
            AddDirectory("Saves", Path.Combine(ProjectDirectory, "Saves"));
            // @"ProjDir\Saves\Audio"
            AddDirectory("Audio", Path.Combine(Dirs["Saves"], "Audio"));
            
            // @"ProjDir\Temp"
            AddDirectory("Temp", Path.Combine(ProjectDirectory, "Temp"));

            // @"ProjDir\Export"
            AddDirectory("Export", Path.Combine(ProjectDirectory, "Export"));
            #endregion

            SetTools(); // Set TTS Tools
            Worker = bw;
            DoWorkEvent = e;

            GenerateResource(epubPath);
            GenerateAudio();

            if (IsCancel())
            {
                Clear(ProjectDirectory);
                return;
            }

            Save();
            ExportEpub("output.epub");
            Status = (int)ProjectStatus.None;
        }
        #endregion

        #region Private Methods
        private bool IsCancel()
        {
            return Status == (int)ProjectStatus.Cancel;
        }

        private void AddDirectory(String dirName, String dirPath)
        {
            Dirs.Add(dirName, Directory.CreateDirectory(dirPath).FullName);
        }

        private void SetTools()
        {
            if (!Tools.IsReady())
            {
                Tools.SentenceSplitter = new SentenceSplitter();
                Tools.Preprocessor = new CPreprocessor();
                Tools.G2P = new CG2P();
                Tools.PhonemeConverter = new CPhonemeConverter();
                Tools.Synthesizer = new CSynthesizer(Dirs["Temp"]);
            }
        }

        private void GetAllContents(List<NavPoint> navList)
        {
            foreach (NavPoint nav in navList)
            {
                if (nav.ContentData != null)
                    Contents.Add(new Content(nav));
                GetAllContents(nav.Children);
            }
        }

        private void GenerateResource(String epubPath)
        {
            if (IsCancel()) return;

            EpubName = "Original_" + Path.GetFileName(epubPath);
            EpubPath = Path.Combine(Dirs["Resources"], EpubName); // Path of the Epub-Copy in this Project
            File.Copy(epubPath, EpubPath);

            Sentence.Total = 0;
            Contents = new List<Content>();
            GetAllContents(EpubReader.TOC);

            Console.WriteLine("Total Content Pages: " + Contents.Count);
            ProgressUpdater updater = new ProgressUpdater(Contents.Count, 100);

            foreach (Content content in Contents)
            {
                // Save Content Structure in @"ProjDir\Resources\Package"
                String contentRescPath = Path.Combine(Dirs["Package"], content.Source);
                Directory.CreateDirectory(Path.GetDirectoryName(contentRescPath));
                using (StreamWriter streamWriter = new StreamWriter(contentRescPath))
                {
                    streamWriter.Write(content.Root);
                    streamWriter.Close();
                }

                updater.UpCount();
                if (IsCancel()) return;
            }
        }

        private void GenerateAudio()
        {
            if (IsCancel()) return;

            Console.WriteLine("Total Sentences: " + Sentence.Total);
            ProgressUpdater updater = new ProgressUpdater(Sentence.Total, 100);
            
            foreach (Content content in Contents)
            {
                String audioPath = Path.Combine(Dirs["Audio"], content.CID);
                Directory.CreateDirectory(audioPath);
                Tools.Synthesize(content, audioPath, updater);

                if (IsCancel()) return;
            }
        }

        private List<XText> GetTextBlocks(XElement element)
        {
            List<XText> xTexts = new List<XText>();
            foreach (XNode node in element.Nodes())
            {
                if (node is XText)
                    xTexts.Add(node as XText);
                else if (node is XElement)
                    xTexts.AddRange(GetTextBlocks(node as XElement));
            }
            return xTexts;
        }

        private String GetClockValue(long bytes)
        {
            long msToHr = 3600000;
            long msToMin = 60000;
            long msToSec = 1000;

            long mSecs = bytes / 32;
            String hr = (mSecs / msToHr).ToString("D2");

            mSecs %= msToHr;
            String min = (mSecs / msToMin).ToString("D2");

            mSecs %= msToMin;
            String sec = (mSecs / msToSec).ToString("D2");

            mSecs %= msToSec;
            String ms = mSecs.ToString("D3");

            return hr + ":" + min + ":" + sec + "." + ms;
        }
        #endregion

        public void ExportEpub(String savePath)
        {
            String exportEpub = Path.Combine(ProjectDirectory, savePath);
            File.Copy(EpubPath, exportEpub, true);
            
            using (ZipArchive archive = ZipFile.Open(exportEpub, ZipArchiveMode.Update))
            {
                long totalBytes = 0;

                XNamespace smilXns = @"http://www.w3.org/ns/SMIL";
                XNamespace epubXns = @"http://www.idpf.org/2007/ops";
                
                XDocument xOpf = XDocument.Parse(EpubReader.OpfFile.Content);
                XElement rootOpf = xOpf.Root;
                XNamespace xnsOpf = rootOpf.Attribute("xmlns") != null ? rootOpf.Attribute("xmlns").Value : XNamespace.None;
                XElement metadataOpf = rootOpf.Element(xnsOpf + "metadata");
                XElement manifestOpf = rootOpf.Element(xnsOpf + "manifest");
                
                foreach (Content content in Contents)
                {
                    long contentBytes = 0;
                    String smilID = "SMIL-" + content.Order;
                    String audioID = "A-" + content.Order;
                    #region Overwrite XHTML files
                    {
                        XElement xContent = new XElement(content.Root);
                        List<XText> xBlocks = GetTextBlocks(xContent.Element(content.Xns + "body"));
                        int count = 0;
                        foreach (XText xText in xBlocks)
                        {
                            int id = Int32.Parse(xText.Value.Substring(1));
                            if (id != count)
                                throw new Exception("Wrong ordering: Blocks");
                            XElement parent = xText.Parent;
                            foreach (Sentence sentence in content.Blocks[id].Sentences)
                            {
                                XElement xSentence = new XElement(content.Xns + "span");
                                xSentence.Add(new XAttribute("id", sentence.BSID));
                                xSentence.Add(new XText(sentence.Text));
                                parent.Add(xSentence);
                            }
                            xText.Remove();
                            count++;
                        }

                        String xhtmlPath = Path.Combine(PackageDir, content.Source);
                        ZipArchiveEntry xhtmlEntry = archive.GetEntry(xhtmlPath.Replace('\\', '/'));
                        if (xhtmlEntry == null)
                            xhtmlEntry = archive.CreateEntry(xhtmlPath);
                        using (StreamWriter streamWriter = new StreamWriter(xhtmlEntry.Open()))
                        {
                            streamWriter.Write(xContent);
                            streamWriter.BaseStream.SetLength(streamWriter.BaseStream.Position);
                            streamWriter.Close();
                        }
                        xhtmlEntry.LastWriteTime = DateTimeOffset.UtcNow.LocalDateTime;
                    }
                    #endregion

                    #region Create SMIL files
                    {
                        String fileName = Path.GetFileName(content.Source);

                        XElement xContent = new XElement(smilXns + "smil");
                        xContent.Add(new XAttribute(XNamespace.Xmlns + "epub", epubXns));
                        xContent.Add(new XAttribute("version", "3.0"));
                        XElement xSeq = new XElement(smilXns + "seq");
                        xSeq.Add(new XAttribute("id", "seq1"));
                        xSeq.Add(new XAttribute(epubXns + "textref", fileName));
                        xSeq.Add(new XAttribute(epubXns + "type", "bodymatter chapter"));
                        xContent.Add(new XElement(smilXns + "body", xSeq));
                        int count = 0;
                        foreach (Block block in content.Blocks)
                        {
                            foreach (Sentence sentence in block.Sentences)
                            {
                                String begin = GetClockValue(contentBytes);
                                contentBytes += sentence.Bytes;
                                String end = GetClockValue(contentBytes);

                                XElement xText = new XElement(smilXns + "text");
                                xText.Add(new XAttribute("src", fileName + "#" + sentence.BSID));
                                XElement xAudio = new XElement(smilXns + "audio");
                                xAudio.Add(new XAttribute("clipBegin", begin));
                                xAudio.Add(new XAttribute("clipEnd", end));
                                xAudio.Add(new XAttribute("src", fileName + ".mp3"));
                                XElement xPar = new XElement(smilXns + "par", xText, xAudio);
                                xPar.Add(new XAttribute("id", "par" + count));
                                xSeq.Add(xPar);
                                count++;
                            }
                        }

                        String smilPath = Path.Combine(PackageDir, content.Source) + ".smil";
                        ZipArchiveEntry smilEntry = archive.CreateEntry(smilPath);
                        using (StreamWriter streamWriter = new StreamWriter(smilEntry.Open()))
                        {
                            streamWriter.Write(xContent);
                            streamWriter.BaseStream.SetLength(streamWriter.BaseStream.Position);
                            streamWriter.Close();
                        }
                        smilEntry.LastWriteTime = DateTimeOffset.UtcNow.LocalDateTime;
                    }
                    #endregion

                    #region Modify Opf file
                    {
                        XElement duration = new XElement(xnsOpf + "meta");
                        duration.Add(new XAttribute("property", "media:duration"));
                        duration.Add(new XAttribute("refines", "#" + smilID));
                        duration.Add(GetClockValue(contentBytes));
                        metadataOpf.Add(duration);
                        totalBytes += contentBytes;


                        XElement smil = new XElement(xnsOpf + "item");
                        smil.Add(new XAttribute("id", smilID));
                        smil.Add(new XAttribute("href", content.Source + ".smil"));
                        smil.Add(new XAttribute("media-type", "application/smil+xml"));

                        XElement audio = new XElement(xnsOpf + "item");
                        audio.Add(new XAttribute("id", audioID));
                        audio.Add(new XAttribute("href", content.Source + ".mp3"));
                        audio.Add(new XAttribute("media-type", "audio/mpeg"));
                        
                        XElement xhtml = manifestOpf.Elements(xnsOpf + "item").FirstOrDefault(e => e.Attribute("href").Value.Equals(content.Source));
                        xhtml.Add(new XAttribute("media-overlay", smilID));
                        xhtml.AddAfterSelf(smil, audio);
                    }
                    #endregion
                    
                    #region Merge Audio files
                    {
                        String contentAudioPath = Path.Combine(Dirs["Audio"], content.CID);

                        // Check wheter every WavFiles has the same wav-format.
                        WaveFormat waveFormat = null;
                        foreach (String sourceFile in Directory.EnumerateFiles(contentAudioPath, "*.wav"))
                        {
                            using (WaveFileReader waveFileReader = new WaveFileReader(sourceFile))
                            {
                                if (waveFormat == null)
                                    waveFormat = waveFileReader.WaveFormat;
                                else if (!waveFormat.Equals(waveFileReader.WaveFormat))
                                    throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                            }
                        }

                        if (waveFormat != null)
                        {
                            byte[] buffer = new byte[1024];
                            int read;
                            String outputWave = Path.Combine(Dirs["Temp"], content.CID + ".wav");
                            using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputWave, waveFormat))
                            {
                                foreach (Block block in content.Blocks)
                                {
                                    foreach (Sentence sentence in block.Sentences)
                                    {
                                        String sourceFile = Path.Combine(contentAudioPath, sentence.BSID + ".wav");
                                        Console.WriteLine(content.CID + ": " + sourceFile);
                                        using (WaveFileReader waveFileReader = new WaveFileReader(sourceFile))
                                        {
                                            while ((read = waveFileReader.Read(buffer, 0, buffer.Length)) > 0)
                                                waveFileWriter.Write(buffer, 0, read);
                                        }
                                    }
                                }
                            }

                            String outputMP3 = Path.Combine(Dirs["Temp"], content.CID + ".mp3");
                            using (WaveFileReader waveReader = new WaveFileReader(outputWave))
                            {
                                using (MediaFoundationResampler resampled = new MediaFoundationResampler(waveReader, new WaveFormat(44100, 1)))
                                {
                                    int desiredBitRate = 0; // ask for lowest available bitrate 
                                    MediaFoundationEncoder.EncodeToMp3(resampled, outputMP3, desiredBitRate);
                                }
                            }

                            String audioPath = Path.Combine(PackageDir, content.Source) + ".mp3";
                            archive.CreateEntryFromFile(outputMP3, audioPath);
                        }
                    }
                    #endregion
                }

                XElement totalDuration = new XElement(xnsOpf + "meta");
                totalDuration.Add(new XAttribute("property", "media:duration"));
                totalDuration.Add(GetClockValue(totalBytes));
                metadataOpf.Add(totalDuration);

                String opfPath = EpubReader.GetOpfPath();
                ZipArchiveEntry opfEntry = archive.GetEntry(opfPath.Replace('\\', '/'));
                if (opfEntry == null)
                    opfEntry = archive.CreateEntry(opfPath);
                using (StreamWriter streamWriter = new StreamWriter(opfEntry.Open()))
                {
                    streamWriter.Write(xOpf);
                    streamWriter.BaseStream.SetLength(streamWriter.BaseStream.Position);
                    streamWriter.Close();
                }
                opfEntry.LastWriteTime = DateTimeOffset.UtcNow.LocalDateTime;
            }
            ZipFile.ExtractToDirectory(exportEpub, Dirs["Export"]);
        }

        private void Clear(String path)
        {
            if (path == null) return;
            try
            {
                Directory.Delete(path, true);
            }
            catch(Exception e)
            {
                Console.WriteLine("Clear Project: ");
                Console.WriteLine(e.Message);
            }
        }

        public void Save()
        {
            Console.WriteLine("Total Content Pages to Save: " + Contents.Count);
            ProgressUpdater updater = new ProgressUpdater(Contents.Count, 100);

            foreach (Content content in Contents)
            {
                // Save Content Detail in @"ProjDir\Saves\Package"
                String contentSavePath = Path.Combine(Dirs["Saves"], content.Source);
                Directory.CreateDirectory(Path.GetDirectoryName(contentSavePath));
                using (StreamWriter streamWriter = new StreamWriter(contentSavePath))
                {
                    streamWriter.Write(content.ToXml());
                    streamWriter.Close();
                }

                updater.UpCount();
                if (IsCancel()) {
                    Clear(ProjectDirectory);
                    return;
                }
            }
        }
    }
}
