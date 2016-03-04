using EPUBGenerator.MainLogic;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace EPUBGenerator.Pages
{
    /// <summary>
    /// Interaction logic for CreateBook3.xaml
    /// </summary>
    public partial class CreateBook3 : UserControl
    {
        private String savePath;
        private string projName;
        private string projPath;
        private string epubPath;

        private int textFiles;
        private int audioFiles;
        private int avgSentences;

        public CreateBook3()
        {
            InitializeComponent();
        }

        public void bookInfo(string projName, string projPath, string epubPath)
        {
            this.projName = projName;
            this.projPath = projPath;
            this.epubPath = epubPath;

            infoprojName.Text = projName;
            infoprojLocation.Text = projPath;
            infoinputEPUB.Text = epubPath;
        }

        private void ProjInfo_Loaded(object sender, RoutedEventArgs e)
        {
            // Get TextBlock reference.
            var block = sender as TextBlock;
            // Set text.
            block.Text = "Project Name :\t\t" + projName + Environment.NewLine + Environment.NewLine +
                "Project Location : \t" + projPath + Environment.NewLine + Environment.NewLine +
                "Input Location : \t\t" + epubPath;
        }

        private void homebutton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }


        private void exportbutton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Project.Instance.ProjectDirectory;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                //INSERT Select .EPUB file location here//
                savePath = saveFileDialog.FileName;

                this.IsEnabled = false;
                System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
                objBlur.Radius = 5;
                Switcher.createBook3.Effect = objBlur;
                /////
                exportPopup.IsOpen = true;


                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += bw_DoWork;
                bw.ProgressChanged += bw_ProgressChanged;
                bw.RunWorkerCompleted += bw_RunWorkerCompleted;
                bw.RunWorkerAsync();
            }
        }

        private void editThisBookbutton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWin = new EditWindow();
            editWin.bookInfo(projPath);             //!!!!
            editWin.Show();
            Switcher.pageSwitcher.Close();
        }

        private void exitbutton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Project.ProgressUpdater.Cancel();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = true;
            exportPopup.IsOpen = false;
            Switcher.createBook3.Effect = null;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Project.ProgressUpdater = new ProgressUpdater(sender as BackgroundWorker, e);
            Project.Export(savePath);
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ExportProgress.Value = e.ProgressPercentage;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            bw.Dispose();
            if (e.Cancelled)
            {
                this.IsEnabled = true;
                exportPopup.IsOpen = false;
                Switcher.createBook3.Effect = null;
            }
            else if (e.Error != null)
            {
                Console.WriteLine("CreatBook3, RunworkerCompleted with Exception: ");
                Console.WriteLine("\t" + e.Error.Message);
                Console.WriteLine(e.Error.StackTrace);
                //ขึ้นerror ให้กด ok
                okButton.Visibility = Visibility.Visible;
                cancelButton.Visibility = Visibility.Hidden;
                ExportProgress.Visibility = Visibility.Hidden;
                ExportWait.Content = "Exporting error !";
            }
            else
            {
                okButton.Visibility = Visibility.Visible;
                cancelButton.Visibility = Visibility.Hidden;
                ExportProgress.Visibility = Visibility.Hidden;
                ExportWait.Content = "DONE !";
            }
        }
    }
}
