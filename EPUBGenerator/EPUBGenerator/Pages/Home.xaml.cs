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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();
        }

        private void createbutton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.createBook1);
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            Switcher.Switch(Switcher.editBook1);
        }

        private void insButton_Click(object sender, RoutedEventArgs e)
        {
            InstructionWindow.setLastWindowLocation(Application.Current.MainWindow.Top, Application.Current.MainWindow.Left);
            InstructionWindow instructionWindow = new InstructionWindow();
            instructionWindow.Show();
            InstructionSwitcher.Switch(InstructionSwitcher.home);
        }


    }
}
