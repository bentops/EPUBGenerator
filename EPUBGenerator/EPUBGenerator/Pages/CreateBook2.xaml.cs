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


        private object Result = null;

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

            /*
            while (Result == null)
            {
                Thread.Sleep(1000);
                Console.WriteLine("WAITING");
            }
            Console.WriteLine("FINISHED");

            Switcher.Switch(Switcher.createBook3);
            Switcher.createBook3.bookInfo(Result);*/

        }
        
        public void CreateProject(string epubPath, string projPath, string projName)
        {
            this.epubPath = epubPath;
            this.projPath = projPath;
            this.projName = projName;

            infoprojName.Text = projName;
            infoprojLocation.Text = projPath;
            infoinputEPUB.Text = epubPath;

            Thread.Sleep(1000);

            ProjectInfo projInfo = null;
            try
            {
                projInfo = new ProjectInfo(epubPath, projPath);
                Epub epubFile = projInfo.EpubFile;

                int total = epubFile.Content.Count;
                int count = 0;
                Console.WriteLine("Total Content Pages: " + total);

                foreach (ContentData cData in epubFile.Content.Values)
                {
                    int id = projInfo.Contents.Count;
                    Content content = new Content(id, cData, projInfo);
                    projInfo.AddContent(content);
                    GeneratorProgress.Value = (++count * 100) / total;
                }

                total = projInfo.TotalSentences;
                count = 0;
                Console.WriteLine("Total Sentences: " + total);
                foreach (Content content in projInfo.Contents)
                    foreach (Block block in content.Blocks)
                        foreach (Sentence sentence in block.Sentences)
                        {
                            sentence.Synthesize();
                            GeneratorProgress.Value = (++count * 100) / total;
                        }
                projInfo.Save();

                Console.WriteLine("RunCompleted, Current Thread: " + Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(500);
                Switcher.Switch(Switcher.createBook3);
                Switcher.createBook3.bookInfo(new Tuple<String, ProjectInfo, int>(epubPath, projInfo, total));
            }
            catch (OperationCanceledException)
            {
                Switcher.Switch(Switcher.createBook1);
                Project.ClearDirectory(projPath);
            }
            catch (Exception ex)
            {
                Switcher.error.setErrorMsgText("invalidEpubFile", Switcher.createBook1);
                Switcher.Switch(Switcher.error);
                Console.WriteLine("CreatBook2, RunworkerCompleted with Exception: ");
                Console.WriteLine("\t" + ex.Message);
                Console.WriteLine(ex.StackTrace);
                Project.ClearDirectory(projPath);
            }
            finally
            {
                if (projInfo != null)
                    projInfo.Dispose();
            }
        }
        

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            ProjectInfo projInfo = null;
            try
            {
                _ProgressUpdater = new ProgressUpdater(sender as BackgroundWorker, e);
                projInfo = new ProjectInfo(epubPath, projPath);

                Epub epubFile = projInfo.EpubFile;

                Console.WriteLine("Total Content Pages: " + epubFile.Content.Count);
                _ProgressUpdater.Initialize(epubFile.Content.Count);
                
                foreach (ContentData cData in epubFile.Content.Values)
                {
                    int id = projInfo.Contents.Count;
                    Content content = new Content(id, cData, projInfo);
                    projInfo.AddContent(content);
                    _ProgressUpdater.Increment();
                }

                int totalSentences = projInfo.TotalSentences;
                Console.WriteLine("Total Sentences: " + totalSentences);
                _ProgressUpdater.Initialize(totalSentences);
                foreach (Content content in projInfo.Contents)
                    foreach (Block block in content.Blocks)
                        foreach (Sentence sentence in block.Sentences)
                        {
                            sentence.Synthesize();
                            _ProgressUpdater.Increment();
                        }
                projInfo.Save();
                _ProgressUpdater.Result = new Tuple<String, ProjectInfo, int>(epubPath, projInfo, totalSentences);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (projInfo != null)
                    projInfo.Dispose();
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GeneratorProgress.Value = e.ProgressPercentage;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
            if (e.Error != null)
            {
                if (e.Error is OperationCanceledException)
                {
                    Switcher.Switch(Switcher.createBook1);
                    Project.ClearDirectory(projPath);
                }
                else
                {
                    Switcher.error.setErrorMsgText("invalidEpubFile", Switcher.createBook1);
                    Switcher.Switch(Switcher.error);
                    Console.WriteLine("CreatBook2, RunworkerCompleted with Exception: ");
                    Console.WriteLine("\t" + e.Error.Message);
                    Console.WriteLine(e.Error.StackTrace);
                }
            }
            else
            {
                Console.WriteLine("RunCompleted, Current Thread: " + Thread.CurrentThread.ManagedThreadId);
                Thread.Sleep(500);
                //Result = e.Result;
                Switcher.Switch(Switcher.createBook3);
                Switcher.createBook3.bookInfo(e.Result);
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
