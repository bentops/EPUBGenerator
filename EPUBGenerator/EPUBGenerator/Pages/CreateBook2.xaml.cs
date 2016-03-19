using eBdb.EpubReader;
using EPUBGenerator.MainLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Block = EPUBGenerator.MainLogic.Block;


namespace EPUBGenerator.Pages
{
    /// <summary>
    /// Interaction logic for CreateBook2.xaml
    /// </summary>
    public partial class CreateBook2 : UserControl
    {
        private ProgressUpdater _ProgressUpdater;
        private string epubPath;
        private string projPath;
        private string projName;

        public CreateBook2()
        {
            InitializeComponent();
        }

        public void createEPUB(string epubPath, string projPath, string projName)
        {
            this.epubPath = epubPath;
            this.projPath = projPath;
            this.projName = projName;

            infoprojName.Text = projName;
            infoprojLocation.Text = projPath;
            infoinputEPUB.Text = epubPath;

            /*

            Console.WriteLine("RunCompleted, Current Thread: " + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(500);
            Switcher.Switch(Switcher.createBook3);
            Switcher.createBook3.bookInfo(projName, projPath, epubPath);
            */
            
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
            
        }
        /*
        private void CreateProject(IProgress<string> progress)
        {
            //_ProgressUpdater = new ProgressUpdater(sender as BackgroundWorker, e);
            ProjectInfo projInfo = new ProjectInfo(epubPath, projPath);

            Epub epubFile = projInfo.EpubFile;
            List<NavPoint> navsWithContent = new List<NavPoint>();
            GetAllNavsWithContents(epubFile.TOC, navsWithContent);

            List<Content> contents = new List<Content>();
            int totalSentence = 0;

            Console.WriteLine("Total Content Pages: " + navsWithContent.Count);
            //_ProgressUpdater.Initialize(navsWithContent.Count);
            foreach (NavPoint nav in navsWithContent)
            {
                Content content = new Content(nav, projInfo);
                contents.Add(content);
                totalSentence += content.SentenceCount;
                // Save Content Structure in @"ProjDir\Resources\Package"
                using (StreamWriter streamWriter = new StreamWriter(content.ContentResource))
                {
                    streamWriter.Write(content.Root);
                    streamWriter.Close();
                }
                //_ProgressUpdater.Increment();
            }

            Console.WriteLine("Total Sentences: " + totalSentence);
            //_ProgressUpdater.Initialize(totalSentence);
            foreach (Content content in contents)
            {
                foreach (Block block in content.Blocks)
                    foreach (Sentence sentence in block.Sentences)
                    {
                        sentence.Synthesize();
                        //_ProgressUpdater.Increment();
                    }
                content.Save();
            }
            projInfo.Save();
        }
        */

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            _ProgressUpdater = new ProgressUpdater(sender as BackgroundWorker, e);
            ProjectInfo projInfo = new ProjectInfo(epubPath, projPath);
            
            Epub epubFile = projInfo.EpubFile;
            List<NavPoint> navsWithContent = new List<NavPoint>();
            GetAllNavsWithContents(epubFile.TOC, navsWithContent);

            List<Content> contents = new List<Content>();
            int totalSentence = 0;

            Console.WriteLine("Total Content Pages: " + navsWithContent.Count);
            _ProgressUpdater.Initialize(navsWithContent.Count);
            foreach (NavPoint nav in navsWithContent)
            {
                Content content = new Content(nav, projInfo);
                contents.Add(content);
                totalSentence += content.SentenceCount;
                // Save Content Structure in @"ProjDir\Resources\Package"
                using (StreamWriter streamWriter = new StreamWriter(content.ContentResource))
                {
                    streamWriter.Write(content.Root);
                    streamWriter.Close();
                }
                _ProgressUpdater.Increment();
            }

            Console.WriteLine("Total Sentences: " + totalSentence);
            _ProgressUpdater.Initialize(totalSentence);
            foreach (Content content in contents)
            {
                foreach (Block block in content.Blocks)
                    foreach (Sentence sentence in block.Sentences)
                    {
                        sentence.Synthesize();
                        _ProgressUpdater.Increment();
                    }
                content.Save();
            }
            projInfo.Save();
        }

        private void GetAllNavsWithContents(List<NavPoint> navList, List<NavPoint> navsWithContent)
        {
            foreach (NavPoint nav in navList)
            {
                if (nav.ContentData != null)
                    navsWithContent.Add(nav);
                GetAllNavsWithContents(nav.Children, navsWithContent);
            }
        }


        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GeneratorProgress.Value = e.ProgressPercentage;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
            if (e.Cancelled)
            {
                Switcher.Switch(Switcher.createBook1);
            }
            else if (e.Error != null)
            {
                Switcher.error.setErrorMsgText("invalidEpubFile", Switcher.createBook1);
                Switcher.Switch(Switcher.error);
                Console.WriteLine("CreatBook2, RunworkerCompleted with Exception: ");
                Console.WriteLine("\t" + e.Error.Message);
                Console.WriteLine(e.Error.StackTrace);
            }
            else
            {
                Console.WriteLine("RunCompleted, Current Thread: " + Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(500);
                Switcher.Switch(Switcher.createBook3);
                Switcher.createBook3.bookInfo(projName, projPath, epubPath);
            }
        }
        
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            _ProgressUpdater.Cancel();
        }
        
        public void editWaitlabel(String txt)
        {
            wait.Content = txt;
        }
    }
}
