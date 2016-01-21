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

namespace EPUBGenerator.Pages
{
    /// <summary>
    /// Interaction logic for EditBook1.xaml
    /// </summary>
    public partial class EditBook1 : System.Windows.Controls.UserControl
    {
        private FolderBrowserDialog folderBrowserDialog;
        private OpenFileDialog openFileDialog;

        public EditBook1()
        {
            InitializeComponent();

            folderBrowserDialog = new FolderBrowserDialog();

            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EPUB files (*.epub)|*.epub";
        }

        private void browseEpub_Click(object sender, RoutedEventArgs e)
        {
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                projPath.Text = openFileDialog.FileName;
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            EditWindow editWin = new EditWindow();
            editWin.bookInfo(openFileDialog.FileName);
            editWin.Show();
            Switcher.pageSwitcher.Close();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.home);
        }
    }
}
