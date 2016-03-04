using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using Path = System.IO.Path;

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private string projPath;

        public EditWindow()
        {
            InitializeComponent();
            //bookName.Content = GetProjectFileName(projPath);
        }

        public void bookInfo(string projPath)
        {
            this.projPath = projPath;
        }

        private void ProjInfo_Loaded(object sender, RoutedEventArgs e)
        {
            // Get TextBlock reference.
            var block = sender as TextBlock;
            // Set text.
            block.Text = "Project Location : \t" + projPath;
        }

        private string GetProjectFileName(string projPath)
        {
            return Path.GetFileNameWithoutExtension(projPath);
        }

        private void forwardB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void browseLocation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void apply_Click(object sender, RoutedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void playpauseB_Click(object sender, RoutedEventArgs e)
        {
            if (playpauseB.Content == FindResource("Play"))
            {
                playpauseB.Content = FindResource("Pause");
                
            }
            else
            {
                playpauseB.Content = FindResource("Play");
            }
        }

        private void stopB_Click(object sender, RoutedEventArgs e)
        {
            if (playpauseB.Content == FindResource("Pause"))
            {
                playpauseB.Content = FindResource("Play");
                playpauseB.IsChecked = false;
            }
        }
    }
}
