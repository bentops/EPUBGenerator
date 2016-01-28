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
        private BackgroundWorker bw;
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
            
            bw = new BackgroundWorker();
            TestClass.bw = bw;
            bw.WorkerReportsProgress = true;
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        private void ProjInfo_Loaded(object sender, RoutedEventArgs e)
        {
            // Get TextBlock reference.
            var block = sender as TextBlock;
            // Set text.
            block.Text = "Project Name :\t\t" + projName + Environment.NewLine + Environment.NewLine +
                "Project Location : \t" + projPath + Environment.NewLine  + Environment.NewLine +
                "Input Location : \t\t" + epubPath;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            TestClass.reCreate(epubPath, projPath);
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GeneratorProgress.Value = e.ProgressPercentage;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(300);
            Switcher.Switch(Switcher.createBook3);
            Switcher.createBook3.bookInfo(projName, projPath, epubPath);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.createBook1);
        }
        
    }
}
