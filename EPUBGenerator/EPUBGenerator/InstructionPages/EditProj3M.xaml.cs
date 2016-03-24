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

namespace EPUBGenerator.InstructionPages
{
    /// <summary>
    /// Interaction logic for ExportProj1.xaml
    /// </summary>
    public partial class EditProj3M : Page
    {
        public EditProj3M()
        {
            InitializeComponent();
        }

        private void homebutton_Click(object sender, RoutedEventArgs e)
        {
            InstructionSwitcher.Switch(InstructionSwitcher.home);
        }

        private void exitbutton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows[1].Close();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            Home.states.Add(InstructionSwitcher.editProj3M2);
            Home.states.ForEach(Console.WriteLine);
            InstructionSwitcher.Switch(InstructionSwitcher.editProj3M2);
        }

        private void NextPhonemePage_Click(object sender, RoutedEventArgs e)
        {
            Home.states.Add(InstructionSwitcher.editProj4);
            Home.states.ForEach(Console.WriteLine);
            InstructionSwitcher.Switch(InstructionSwitcher.editProj4);
        }

        private void NextSegmentPage_Click(object sender, RoutedEventArgs e)
        {
            Home.states.Add(InstructionSwitcher.editProj5);
            Home.states.ForEach(Console.WriteLine);
            InstructionSwitcher.Switch(InstructionSwitcher.editProj5);
        }

        private void NextPlayConsolePage_Click(object sender, RoutedEventArgs e)
        {
            Home.states.Add(InstructionSwitcher.editProj6);
            Home.states.ForEach(Console.WriteLine);
            InstructionSwitcher.Switch(InstructionSwitcher.editProj6);
        }

        private void NextImgPage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PrevPage_click(object sender, RoutedEventArgs e)
        {
            Home.states.RemoveAt(Home.states.Count - 1);
            InstructionSwitcher.Switch(Home.states[Home.states.Count - 1]);
        }
    }
}
