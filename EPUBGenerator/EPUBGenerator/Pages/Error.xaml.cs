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
    /// Interaction logic for Error.xaml
    /// </summary>
    public partial class Error : UserControl
    {

        private string errorMsg = "none";
        private string errorText = "none";
        private UserControl previousPage;

        public Error()
        {
            InitializeComponent();
        }

        public void setErrorMsgText(string errorType, UserControl previousPage)   //errorType = invalidEpubFile, invalidProjFile, somethingWrong, error
        {
            this.previousPage = previousPage;

            if (errorType == "invalidEpubFile")
            {
                this.errorMsg = "Invalid File Format";
                this.errorText = "Please Select .epub File";
            }
            else if (errorType == "invalidProjFile")
            {
                this.errorMsg = "Invalid Project Flie";
                this.errorText = "Please Select EPUB Project File";
            }
            else if (errorType == "somethingWrong")
            {
                this.errorMsg = "There is something wrong while processing";
                this.errorText = "Please Try Again";
            }
            else if (errorType == "error")
            {
                this.errorMsg = "Error";
                this.errorText = "";
            }
        }

        private void errorMsg_Loaded(object sender, RoutedEventArgs e)
        {
            // Get TextBlock reference.
            var block = sender as TextBlock;
            // Set text.
            block.Text = errorMsg;
        }

        private void errorText_Loaded(object sender, RoutedEventArgs e)
        {
            // Get TextBlock reference.
            var block = sender as TextBlock;
            // Set text.
            block.Text = errorText;
        }


        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            if (previousPage == null) Switcher.Switch(Switcher.home);
            else Switcher.Switch(previousPage);
        }
    }
}
