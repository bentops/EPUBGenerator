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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EPUBGenerator.Pages
{
    /// <summary>
    /// Interaction logic for CreateBook3.xaml
    /// </summary>
    public partial class CreateBook3 : UserControl
    {
        private string projName;
        private string savePath;
        private string epubPath;

        public CreateBook3()
        {
            InitializeComponent();
            
        }
        public void bookInfo(string projName, string savePath, string epubPath)
        {
            Switcher.Switch(this);

            this.projName = projName;
            this.savePath = savePath;
            this.epubPath = epubPath;
        }

        private void ProjInfo_Loaded(object sender, RoutedEventArgs e)
        {
            // Get TextBlock reference.
            var block = sender as TextBlock;
            // Set text.
            block.Text = "Project Name :\t\t" + projName + Environment.NewLine + Environment.NewLine +
                "Project Location : \t" + savePath + Environment.NewLine + Environment.NewLine +
                "Input Location : \t\t" + epubPath;
        }

        private void homebutton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.NewHome();
        }

        private void editThisBookbutton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void exitbutton_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
