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

namespace EPUBGenerator.MainLogic
{
    enum Statuses : int { None, Create, Edit };
    class Project
    {
        public static String ProjectPath { get; private set; }
        public static String EpubName { get; private set; }
        public static String EpubPath { get; private set; }
        public static BackgroundWorker Worker { get; private set; }
        public static DoWorkEventArgs DoWorkEvent { get; private set; }
        public static int Status;

        private static Epub EpubReader { get; set; }
        private static List<Content> Contents { get; set; }
        private static StreamReader StreamReader { get; set; }
        private static StreamWriter StreamWriter { get; set; }

        #region Subdirectories
        private static String ResourcesPath { get; set; }
        private static String PackagePath { get; set; }
        private static String SavesPath { get; set; }
        private static String TempPath { get; set; }
        #endregion

        public static void SetTools()
        {
            if (!Tools.IsReady())
            {
                Tools.SentenceSplitter = new SentenceSplitter();
                Tools.Preprocessor = new CPreprocessor();
                Tools.G2P = new CG2P();
                Tools.PhonemeConverter = new CPhonemeConverter();
                Tools.Synthesizer = new CSynthesizer(TempPath);
            }
        }

        public static void Create(String epubPath, String projPath, BackgroundWorker bw, DoWorkEventArgs e) 
        {
            // ------------------ Initial --------------------------
            EpubReader = new Epub(epubPath); // Use original epubPath to read data
            SetSubdirectories(projPath); // Create Subdirectories
            SetTools(); // Set TTS Tools

            Status = (int)Statuses.Create;
            Worker = bw;
            DoWorkEvent = e;

            EpubName = "Original_" + Path.GetFileName(epubPath);
            EpubPath = Path.Combine(ResourcesPath, EpubName); // Path of the Epub-Copy in this Project
            File.Copy(epubPath, EpubPath);
            Contents = GetAllContents(EpubReader.TOC);
            // -----------------------------------------------------

            Console.WriteLine("Total: " + Contents.Count);
            int i = 0;
            foreach (Content content in Contents)
            {
                Save(content);

                i++;
                bw.ReportProgress(i * 50 / Contents.Count);
                Thread.Sleep(10);

                if (Worker.CancellationPending)
                {
                    DoWorkEvent.Cancel = true;
                    DoWorkEvent.Result = "Cancel";
                    Clear(ProjectPath);
                    break;
                }
            }

            Console.WriteLine("Total Sentences: " + Sentence.total);
            i = 0;
            foreach (Content content in Contents)
            {
                foreach (Block block in content.Blocks)
                {
                    foreach (Sentence sentence in block.Sentences)
                    {
                        Console.WriteLine("IN" + i);
                        sentence.Synthesize();
                        Console.WriteLine("OUT" + i);

                        i++;
                        bw.ReportProgress(i * 50 / Sentence.total + 50);

                        if (Worker.CancellationPending)
                        {
                            DoWorkEvent.Cancel = true;
                            DoWorkEvent.Result = "Cancel";
                            Clear(ProjectPath);
                            break;
                        }
                    }
                }
            }

            // ------------------ Final --------------------------
            Status = (int)Statuses.None;
            ExportEpub(Path.Combine(ProjectPath, "OutputEPUB.zip"));
            // ---------------------------------------------------
        }

        public static void ExportEpub(String savePath)
        {
            String exportPath = Directory.CreateDirectory(Path.Combine(ProjectPath, "Export")).FullName;
            ZipFile.ExtractToDirectory(EpubPath, exportPath);

            String exportPackagePath = Path.Combine(exportPath, EpubReader.GetOpfDirectory());

            foreach (Content content in Contents)
            {
                XElement xContent = new XElement(content.Root);
                List<XText> xBlocks = GetTextBlocks(xContent.Element(content.Xns + "body"));
                int count = 0;
                foreach (XText xText in xBlocks)
                {
                    Console.WriteLine(xText.Value);
                    int id = Int32.Parse(xText.Value.Substring(1));
                    if (id != count)
                        throw new Exception("Wrong ordering: Blocks");
                    XElement parent = xText.Parent;
                    foreach (Sentence sentence in content.Blocks[id].Sentences)
                    {
                        XElement xSentence = new XElement(content.Xns + "span");
                        xSentence.Add(new XAttribute("id", xText.Value + sentence.SID));
                        xSentence.Add(new XText(sentence.Text));
                        parent.Add(xSentence);
                    }
                    xText.Remove();
                    count++;
                }
                StreamWriter = new StreamWriter(Path.Combine(exportPackagePath, content.Source));
                StreamWriter.Write(xContent);
                StreamWriter.Close();
            }
            
            ZipFile.CreateFromDirectory(exportPath, savePath, CompressionLevel.Optimal, false, System.Text.UTF8Encoding.UTF8);
            //Path.ChangeExtension(savePath, "epub");
            //Ionic.Zip.ZipOutputStream.
        }

        private static List<XText> GetTextBlocks(XElement element)
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


        private static void Save(Content content)
        {
            StreamWriter sw;

            // Save Content Structure
            String contentDirectory = Path.GetDirectoryName(content.Source);
            Directory.CreateDirectory(Path.Combine(PackagePath, contentDirectory));
            sw = new StreamWriter(Path.Combine(PackagePath, content.Source));
            sw.Write(content.Root);
            sw.Close();

            // Save Content Detail
            Directory.CreateDirectory(Path.Combine(SavesPath, contentDirectory));
            sw = new StreamWriter(Path.Combine(SavesPath, content.Source));
            sw.Write(content.ToXml());
            sw.Close();

        }

        private static void Clear(String path)
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

        private static void SetSubdirectories(String projPath)
        {
            ProjectPath = projPath;

            Directory.CreateDirectory(ResourcesPath = Path.Combine(ProjectPath, "Resources"));

            String packageDirectory = EpubReader.GetOpfDirectory();
            Directory.CreateDirectory(PackagePath = Path.Combine(ResourcesPath, packageDirectory));

            Directory.CreateDirectory(SavesPath = Path.Combine(ProjectPath, "Saves"));
            
            Directory.CreateDirectory(TempPath = Path.Combine(ProjectPath, "Temp"));
        }

        private static List<Content> GetAllContents(List<NavPoint> navList)
        {
            List<Content> contents = new List<Content>();
            foreach (NavPoint nav in navList)
            {
                if (nav.ContentData != null)
                    contents.Add(new Content(nav));
                contents.AddRange(GetAllContents(nav.Children));
            }
            return contents;
        }
        
    }
}
