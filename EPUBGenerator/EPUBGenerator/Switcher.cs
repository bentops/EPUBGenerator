using EPUBGenerator.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;


namespace EPUBGenerator
{
    public static class Switcher
    {
        public static MainWindow pageSwitcher;
        public static CreateBook1 createBook1;
        public static CreateBook2 createBook2;
        public static CreateBook3 createBook3;
        public static Home home;

        public static void Switch(UserControl newPage)
        {
            pageSwitcher.Navigate(newPage);
        }
        
    }
}
