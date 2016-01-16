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
        private BackgroundWorker bw = new BackgroundWorker();
        private string fileName;
        private string filePath;

        public CreateBook2()
        {
            InitializeComponent();
        }

        public void createEPUB(string file, string savePath)
        {
            fileName = file;
            filePath = savePath;
            TestClass.bw = bw;

            Switcher.Switch(this);

            Console.WriteLine("Suay kaaaa");
            bw.WorkerReportsProgress = true;
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.RunWorkerAsync();
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            TestClass.reCreate(fileName, filePath);
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            GeneratorProgess.Value = e.ProgressPercentage;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(300);
            Switcher.Switch(Switcher.createBook3);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.createBook1);
        }
        
    }
}
