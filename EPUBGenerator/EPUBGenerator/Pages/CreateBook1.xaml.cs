using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace EPUBGenerator.Pages
{
    /// <summary>
    /// Interaction logic for CreateBook1.xaml
    /// </summary>
    public partial class CreateBook1 : UserControl
    {
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
                return Path.Combine(LocationPath, ProjectName);
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
            try
            {
                if (!Directory.CreateDirectory(ProjectPath).EnumerateFileSystemInfos().Any())
                {
                    Switcher.Switch(Switcher.createBook2);
                    Switcher.createBook2.createEPUB(EpubPath, ProjectPath, ProjectName);
                    //System.Threading.Thread.Sleep(1000);
                    //Switcher.createBook2.CreateProject(EpubPath, ProjectPath, ProjectName);
                    //System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
                else
                {
                    projectNameError.Content = "'" + ProjectName + "'" + " already exists in this directory and is not empty." + Environment.NewLine + "Please use other name.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("CreatBook1, NextButton: ");
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
            }
        }
        
        private void projName_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateProjectLocationPath();
            updateNextButton();
        }

        private void browseLocation_Click(object sender, RoutedEventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                updateProjectLocationPath();
                updateNextButton();
            }
        }

        private void browseEpub_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
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
            nextButton.IsEnabled = !String.IsNullOrEmpty(ProjectPath) && !String.IsNullOrEmpty(EpubPath);
        }
    }
}
