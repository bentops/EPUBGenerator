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
using EPUBGenerator.Pages;

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Switcher.pageSwitcher = this;
            Switcher.home = new Home();
            Switcher.createBook1 = new CreateBook1();
            Switcher.createBook2 = new CreateBook2();
            Switcher.createBook3 = new CreateBook3();
            Switcher.Switch(Switcher.home);
        }

        public void Navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }

    }
}
