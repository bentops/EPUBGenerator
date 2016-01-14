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

using TTSScheduler;

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for TempWindow.xaml
    /// </summary>
    public partial class TempWindow : Window
    {
        //private Scheduler TTSSch = new Scheduler("g2pconfig_cutts_dict.conf", "SynBlock.conf");
        private FolderBrowserDialog folderBrowserDialog;
        private OpenFileDialog openFileDialog;


        public TempWindow()
        {
            InitializeComponent();

            folderBrowserDialog = new FolderBrowserDialog();

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EPUB files (*.epub)|*.epub";

            createButton.IsEnabled = false;
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            output.Text = TestClass.reCreate(openFileDialog.FileName, (string)projLocationPath.Content);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TTSSch.Clear();
            //TestClass.Save();
        }
        
        private void projName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(folderBrowserDialog.SelectedPath))
            {
                projLocationPath.Content = System.IO.Path.Combine(folderBrowserDialog.SelectedPath, projName.Text);
            }
            updateCreateButton();
        }

        private void browseLocation_Click(object sender, RoutedEventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                projLocationPath.Content = System.IO.Path.Combine(folderBrowserDialog.SelectedPath, projName.Text);
                updateCreateButton();
            }
        }

        private void browseEpub_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                epubPath.Content = openFileDialog.FileName;
                updateCreateButton();
            }
        }

        private void updateCreateButton()
        {
            createButton.IsEnabled = false;
            if (string.IsNullOrEmpty(openFileDialog.FileName)) return;
            if (string.IsNullOrEmpty(projName.Text)) return;
            if (string.IsNullOrEmpty(folderBrowserDialog.SelectedPath)) return;
            createButton.IsEnabled = true;
        }
    }
}
/*
a-christmas-carol-audio.epub
*/
