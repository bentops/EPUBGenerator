 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;

namespace EPUBGenerator.Pages
{
    /// <summary>
    /// Interaction logic for CreateBook1.xaml
    /// </summary>
    public partial class CreateBook1 : System.Windows.Controls.UserControl
    {
        //private Scheduler TTSSch = new Scheduler("g2pconfig_cutts_dict.conf", "SynBlock.conf");
        private static String plsSelLoc = ".. please select project location ..";
        private static String plsSelEpub = ".. please select input file ..";
        private FolderBrowserDialog folderBrowserDialog;
        private OpenFileDialog openFileDialog;
        private String ProjectName
        {
            get { return projName.Text; }
        }
        private String LocationPath
        {
            get { return folderBrowserDialog.SelectedPath; }
        }
        private String ProjectPath
        {
            get
            {
                if (String.IsNullOrEmpty(LocationPath) || String.IsNullOrEmpty(ProjectName))
                    return "";
                return System.IO.Path.Combine(LocationPath, ProjectName);
            }
        }
        private String EpubPath
        {
            get { return openFileDialog.FileName; }
        }

        public CreateBook1()
        {
            InitializeComponent();

            folderBrowserDialog = new FolderBrowserDialog();
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EPUB files (*.epub)|*.epub";

            updateProjectLocationPath();
            updateEpubPath();
            updateNextButton();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.home);
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.createBook2);
            Switcher.createBook2.createEPUB(EpubPath, ProjectPath, ProjectName);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TTSSch.Clear();
            //TestClass.Save();
        }

        private void projName_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateProjectLocationPath();
            updateNextButton();
        }

        private void browseLocation_Click(object sender, RoutedEventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                updateProjectLocationPath();
                updateNextButton();
            }
        }

        private void browseEpub_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                updateEpubPath();
                updateNextButton();
            }
        }

        private void updateProjectLocationPath()
        {
            projLocationPath.Text = String.IsNullOrEmpty(LocationPath) ? plsSelLoc :
                String.IsNullOrEmpty(ProjectName) ? LocationPath : ProjectPath;
        }

        private void updateEpubPath()
        {
            epubPath.Text = String.IsNullOrEmpty(EpubPath) ? plsSelEpub : EpubPath;
        }

        private void updateNextButton()
        {
            nextButton.IsEnabled = !String.IsNullOrEmpty(ProjectPath) && !String.IsNullOrEmpty(EpubPath); ;
        }
    }
}
