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
    /// Interaction logic for CreateBook2.xaml
    /// </summary>
    public partial class CreateBook2 : UserControl
    {

        public CreateBook2()
        {
            InitializeComponent();
        }

        public void createEPUB(string file, string savePath)
        {
            Switcher.Switch(this);
            TestClass.reCreate(file, savePath);
        }
        

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.createBook1);
        }
        
    }
}
