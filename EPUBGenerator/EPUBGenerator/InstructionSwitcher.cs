using EPUBGenerator.InstructionPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;


namespace EPUBGenerator
{
    public static class InstructionSwitcher
    {
        public static InstructionWindow pageSwitcher;
        public static Home home;

        public static void Switch(Page newPage)
        {
            pageSwitcher.Navigate(newPage);
        }
    }
}
