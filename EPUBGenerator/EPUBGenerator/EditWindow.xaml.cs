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
            block.Text = "Project Name : \t" + GetProjectFileName(projPath) + Environment.NewLine +
                "Project Location : \t" + projPath;
        }

        private string GetProjectFileName(string projPath)
        {
            return Path.GetFileNameWithoutExtension(projPath);
        }
    }
}
