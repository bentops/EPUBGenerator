using EPUBGenerator.InstructionPages;
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

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for InstructionWindow.xaml
    /// </summary>
    public partial class InstructionWindow : Window
    {
        private static double LastWindowTop;
        private static double LastWindowLeft;

        public InstructionWindow()
        {
            InitializeComponent();
            InstructionSwitcher.pageSwitcher = this;
            InstructionSwitcher.home = new Home();
            InstructionSwitcher.createProj1 = new CreateProj1();
            InstructionSwitcher.createProj2 = new CreateProj2();
            InstructionSwitcher.createProj3 = new CreateProj3();
            InstructionSwitcher.createProj4 = new CreateProj4();

            InstructionSwitcher.exportProj1 = new ExportProj1();
            InstructionSwitcher.exportProj2 = new ExportProj2();
            InstructionSwitcher.exportProj3 = new ExportProj3();
            InstructionSwitcher.exportProj4 = new ExportProj4();

            this.Left = LastWindowLeft - 20;
            this.Top = LastWindowTop - 20;

            InstructionSwitcher.Switch(InstructionSwitcher.home);
        }

        public static void setLastWindowLocation(double top, double left)
        {
            LastWindowTop = top;
            LastWindowLeft = left;
        }

        public void Navigate(Page nextPage)
        {
            this.Content = nextPage;
        }
    }
}
