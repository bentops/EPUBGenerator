﻿using EPUBGenerator.MainLogic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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


namespace EPUBGenerator.Pages
{
    /// <summary>
    /// Interaction logic for CreateBook2.xaml
    /// </summary>
    public partial class CreateBook2 : UserControl
    {
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

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Project.ProgressUpdater = new ProgressUpdater(sender as BackgroundWorker, e);
            Project.Create(epubPath, projPath);
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GeneratorProgress.Value = e.ProgressPercentage;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            bw.Dispose();
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
                Thread.Sleep(500);
                Switcher.Switch(Switcher.createBook3);
                Switcher.createBook3.bookInfo(projName, projPath, epubPath);
            }
        }
        
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Project.ProgressUpdater.Cancel();
        }
        
        public void editWaitlabel(String txt)
        {
            wait.Content = txt;
        }
    }
}
