using ChulaTTS.G2PConverter;
using ChulaTTS.G2PConverter.G2P;
using ChulaTTS.G2PConverter.PhonemeConverter;
using ChulaTTS.G2PConverter.Preprocessor;
using ChulaTTS.G2PConverter.SentenceSplitter;
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
        private static List<NavPoint> NavPoints { get; set; }

        #region Subdirectories
        private static String ResourcesPath { get; set; }
        private static String PackagePath { get; set; }
        private static String SavesPath { get; set; }
        #endregion

        public static void SetTools()
        {
            if (!Tools.IsReady())
            {
                Tools.SentenceSplitter = new SentenceSplitter();
                Tools.Preprocessor = new Preprocessor();
                Tools.G2P = new G2P();
                Tools.PhonemeConverter = new PhonemeConverter();
            }
        }

        public static void Create(String epubPath, String projPath, BackgroundWorker bw, DoWorkEventArgs e) 
        {
            // ------------------ Initial --------------------------
            EpubReader = new Epub(epubPath); // Use original epubPath to read data
            SetTools(); // Set TTS Tools
            SetSubdirectories(projPath); // Create Subdirectories

            Status = (int)Statuses.Create;
            Worker = bw;
            DoWorkEvent = e;

            EpubName = "Original_" + Path.GetFileName(epubPath);
            EpubPath = Path.Combine(ResourcesPath, EpubName); // Path of the Epub-Copy in this Project
            File.Copy(epubPath, EpubPath);
            NavPoints = GetAllNavPoints(EpubReader.TOC);
            // -----------------------------------------------------

            Console.WriteLine("Total: " + NavPoints.Count);
            int i = 0;
            foreach (NavPoint nav in NavPoints)
            {
                Content content = new Content(nav);
                Save(content);

                i++;
                bw.ReportProgress(i * 100 / NavPoints.Count);
                Thread.Sleep(100);

                if (Worker.CancellationPending)
                {
                    DoWorkEvent.Cancel = true;
                    DoWorkEvent.Result = "Cancel";
                    Clear(ProjectPath);
                    break;
                }
            }

            // ------------------ Final --------------------------
            Status = (int)Statuses.None;
            ExportEpub(Path.Combine(ProjectPath, "OutputEPUB.epub"));
            // ---------------------------------------------------
        }

        public static void ExportEpub(String savePath)
        {
            String exportPath = Directory.CreateDirectory(Path.Combine(ProjectPath, "Export")).FullName;
            ZipFile.ExtractToDirectory(EpubPath, exportPath);

            String exportPackagePath = Path.Combine(exportPath, EpubReader.GetOpfDirectory());

            

            ZipFile.CreateFromDirectory(exportPath, savePath);
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
        }

        private static List<NavPoint> GetAllNavPoints(List<NavPoint> navList)
        {
            List<NavPoint> navs = new List<NavPoint>();
            foreach (NavPoint nav in navList)
            {
                if (nav.ContentData != null)
                    navs.Add(nav);
                navs.AddRange(GetAllNavPoints(nav.Children));
            }
            return navs;
        }
        
    }
}
