using eBdb.EpubReader;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TTS;

namespace EPUBGenerator.MainLogic
{
    static class Project
    {
        public static readonly Synthesizer Synthesizer = new Synthesizer();

        #region Public Methods
        public static void Export(String epubProjPath, String outputPath, ProgressUpdater progressUpdater)
        {
            ProjectInfo projInfo = new ProjectInfo(epubProjPath);
            File.Copy(projInfo.EpubPath, outputPath, true);
            Epub epubFile = projInfo.EpubFile;

            ClearDirectory(projInfo.TempPath);
            using (ZipArchive archive = ZipFile.Open(outputPath, ZipArchiveMode.Update))
            {
                long totalBytes = 0;

                XNamespace smilXns = @"http://www.w3.org/ns/SMIL";
                XNamespace epubXns = @"http://www.idpf.org/2007/ops";

                XDocument xOpf = XDocument.Parse(epubFile.OpfFile.Content);
                XElement rootOpf = xOpf.Root;
                XNamespace xnsOpf = rootOpf.Attribute("xmlns") != null ? rootOpf.Attribute("xmlns").Value : XNamespace.None;
                XElement metadataOpf = rootOpf.Element(xnsOpf + "metadata");
                XElement manifestOpf = rootOpf.Element(xnsOpf + "manifest");

                int total = 4 * projInfo.Contents.Count + 1;
                progressUpdater.Initialize(total);
                foreach (Content content in projInfo.Contents)
                {
                    long contentBytes = 0;
                    String smilID = GetSmilID(content.Order);
                    String audioID = GetAudioID(content.Order);

                    #region Overwrite XHTML files
                    {
                        XElement xContent = new XElement(content.Root);
                        List<XText> xContentBlocks = new List<XText>();
                        List<XElement> xImageBlocks = new List<XElement>();
                        GetBlocks(content.Xns, xContent.Element(content.Xns + "body"), xContentBlocks, xImageBlocks);
                        int count = 0;
                        foreach (XText xText in xContentBlocks)
                        {
                            int id = Int32.Parse(xText.Value.Split('-')[1]);
                            if (id != count)
                                throw new Exception("Wrong ordering: ContentBlocks");

                            XElement parent = xText.Parent;
                            foreach (Sentence sentence in content.ContentBlocks[id].Sentences)
                            {
                                XElement xSentence = new XElement(content.Xns + "span");
                                xSentence.Add(new XAttribute("id", GetXhtmlID(sentence)));
                                xSentence.Add(new XText(sentence.OriginalText));
                                parent.Add(xSentence);
                            }
                            xText.Remove();
                            count++;
                        }
                        count = 0;
                        foreach (XElement xImage in xImageBlocks)
                        {
                            int id = Int32.Parse(xImage.Attribute("id").Value.Split('-')[1]);
                            if (id != count)
                                throw new Exception("Wrong ordering: ImageBlocks");
                            xImage.SetAttributeValue("alt", content.ImageBlocks[id].Text);
                            count++;
                        }
                        String xhtmlZipPath = Path.Combine(projInfo.PackageName, content.Source);
                        ZipArchiveEntry xhtmlEntry = archive.GetEntry(xhtmlZipPath.Replace('\\', '/'));
                        if (xhtmlEntry == null)
                            xhtmlEntry = archive.CreateEntry(xhtmlZipPath);
                        using (StreamWriter streamWriter = new StreamWriter(xhtmlEntry.Open()))
                        {
                            streamWriter.Write(xContent);
                            streamWriter.BaseStream.SetLength(streamWriter.BaseStream.Position);
                            streamWriter.Close();
                        }
                        xhtmlEntry.LastWriteTime = DateTimeOffset.UtcNow.LocalDateTime;
                    }

                    progressUpdater.Increment();
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
                            if (block is ImageBlock)
                            {
                                String begin = GetClockValue(contentBytes);
                                contentBytes += block.Bytes;
                                String end = GetClockValue(contentBytes);

                                XElement xText = new XElement(smilXns + "text");
                                xText.Add(new XAttribute("src", fileName + "#" + GetXhtmlID(block as ImageBlock)));
                                XElement xAudio = new XElement(smilXns + "audio");
                                xAudio.Add(new XAttribute("clipBegin", begin));
                                xAudio.Add(new XAttribute("clipEnd", end));
                                xAudio.Add(new XAttribute("src", fileName + ".mp3"));
                                XElement xPar = new XElement(smilXns + "par", xText, xAudio);
                                xPar.Add(new XAttribute("id", "par" + count));
                                xSeq.Add(xPar);
                                count++;
                                continue;
                            }
                            foreach (Sentence sentence in block.Sentences)
                            {
                                String begin = GetClockValue(contentBytes);
                                contentBytes += sentence.Bytes;
                                String end = GetClockValue(contentBytes);

                                XElement xText = new XElement(smilXns + "text");
                                xText.Add(new XAttribute("src", fileName + "#" + GetXhtmlID(sentence)));
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

                        String smilZipPath = Path.Combine(projInfo.PackageName, content.Source) + ".smil";
                        ZipArchiveEntry smilEntry = archive.CreateEntry(smilZipPath);
                        using (StreamWriter streamWriter = new StreamWriter(smilEntry.Open()))
                        {
                            streamWriter.Write(xContent);
                            streamWriter.BaseStream.SetLength(streamWriter.BaseStream.Position);
                            streamWriter.Close();
                        }
                        smilEntry.LastWriteTime = DateTimeOffset.UtcNow.LocalDateTime;
                    }

                    progressUpdater.Increment();
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

                    progressUpdater.Increment();
                    #endregion

                    #region Merge Audio files
                    {
                        // Check wheter every WavFiles has the same wav-format.
                        WaveFormat waveFormat = null;
                        foreach (String sourceFile in Directory.EnumerateFiles(content.ContentAudio, "*.wav"))
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
                            String outputWave = Path.Combine(projInfo.TempPath, content.CID + ".wav");
                            using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputWave, waveFormat))
                            {
                                foreach (Block block in content.Blocks)
                                {
                                    foreach (Sentence sentence in block.Sentences)
                                    {
                                        String sourceFile = sentence.WavPath;
                                        using (WaveFileReader waveFileReader = new WaveFileReader(sourceFile))
                                        {
                                            while ((read = waveFileReader.Read(buffer, 0, buffer.Length)) > 0)
                                                waveFileWriter.Write(buffer, 0, read);
                                        }
                                        //ProgressUpdater.Increment();
                                    }
                                }
                            }

                            String outputMP3 = Path.Combine(projInfo.TempPath, content.CID + ".mp3");
                            using (WaveFileReader waveReader = new WaveFileReader(outputWave))
                            {
                                using (MediaFoundationResampler resampled = new MediaFoundationResampler(waveReader, new WaveFormat(44100, 1)))
                                {
                                    int desiredBitRate = 0; // ask for lowest available bitrate 
                                    MediaFoundationEncoder.EncodeToMp3(resampled, outputMP3, desiredBitRate);
                                }
                            }

                            String audioPath = GetDirectory(projInfo.PackageName, content.Source + ".mp3");
                            archive.CreateEntryFromFile(outputMP3, audioPath);
                        }
                    }

                    progressUpdater.Increment();
                    #endregion
                }

                XElement totalDuration = new XElement(xnsOpf + "meta");
                totalDuration.Add(new XAttribute("property", "media:duration"));
                totalDuration.Add(GetClockValue(totalBytes));
                metadataOpf.Add(totalDuration);

                String opfPath = epubFile.GetOpfPath();
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

            ClearDirectory(projInfo.ExportPath);
            ZipFile.ExtractToDirectory(outputPath, projInfo.ExportPath);

            progressUpdater.Increment();
        }
        
        public static bool IsRandom(int id)
        {
            return ProjectProperties.MinRandomValue <= id && id < ProjectProperties.MaxRandomValue;
        }

        public static bool IsRandom(String wavName)
        {
            return IsRandom(GetIDFromWavName(wavName));
        }

        public static int GetIDFromWavName(String wavName)
        {
            return int.Parse(Path.GetFileNameWithoutExtension(wavName).Substring(1));
        }

        public static String GetWavNameFromID(int sID)
        {
            return "S" + sID.ToString("D" + ProjectProperties.Digits) + ".wav";
        }

        public static int GetRandomUniqueID(String path)
        {
            int randID = 0;
            string wavPath = "";
            do
            {
                Random random = new Random();
                randID = random.Next(ProjectProperties.MinRandomValue, ProjectProperties.MaxRandomValue);
                wavPath = Path.Combine(path, GetWavNameFromID(randID));
            } while (File.Exists(wavPath));
            return randID;
        }

        public static void ClearDirectory(String path)
        {
            foreach (String file in Directory.GetFiles(path))
                File.Delete(file);
            foreach (String directory in Directory.GetDirectories(path))
                Directory.Delete(directory, true);
        }

        public static String GetDirectory(params String[] paths)
        {
            String path = Path.Combine(paths);
            String directory = String.IsNullOrEmpty(Path.GetExtension(path))? path : Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            return path;
        }
        #endregion
        
        #region Private Methods
        private static void GetBlocks(XNamespace xns, XElement element, List<XText> xContentBlocks, List<XElement> xImageBlocks)
        {
            if (element.Name.Equals(xns + "img"))
                xImageBlocks.Add(element);
            foreach (XNode node in element.Nodes())
            {
                if (node is XText)
                    xContentBlocks.Add(node as XText);
                else if (node is XElement)
                    GetBlocks(xns, node as XElement, xContentBlocks, xImageBlocks);
            }
        }
        
        private static String GetSmilID(int cOrder)
        {
            return "SMIL-" + cOrder;
        }

        private static String GetAudioID(int cOrder)
        {
            return "A-" + cOrder;
        }

        private static String GetXhtmlID(Sentence sentence)
        {
            return sentence.Content.CID + "-" + sentence.SID;
        }

        private static String GetXhtmlID(ImageBlock iBlock)
        {
            return iBlock.Content.CID + "-" + iBlock.B_ID;
        }

        private static String GetClockValue(long bytes)
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

    }
}
