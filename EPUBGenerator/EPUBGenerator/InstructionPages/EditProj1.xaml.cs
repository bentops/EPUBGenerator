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
    /// Interaction logic for EditProj1.xaml
    /// </summary>
    public partial class EditProj1 : Page
    {
        public EditProj1()
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

        //private void NextPage_Click(object sender, RoutedEventArgs e)
        //{
        //    InstructionSwitcher.Switch(InstructionSwitcher.createProj2);
        //}

        private void EditProjPage_Click(object sender, RoutedEventArgs e)
        {
            Home.states.Add(InstructionSwitcher.editProj3M);
            Home.states.ForEach(Console.WriteLine);
            InstructionSwitcher.Switch(InstructionSwitcher.editProj3M);
        }

        private void OpenEditProjPage_Click(object sender, RoutedEventArgs e)
        {
            Home.states.Add(InstructionSwitcher.editProj2);
            Home.states.ForEach(Console.WriteLine);
            InstructionSwitcher.Switch(InstructionSwitcher.editProj2);
        }

        private void s1MouseEnter(object sender, RoutedEventArgs e)
        {
            s1.Background = Brushes.Beige;
        }

        private void s1MouseLeave(object sender, RoutedEventArgs e)
        {
            s1.Background = Brushes.LemonChiffon;
        }

        private void s2MouseEnter(object sender, RoutedEventArgs e)
        {
            s2.Background = Brushes.Beige;
        }

        private void s2MouseLeave(object sender, RoutedEventArgs e)
        {
            s2.Background = Brushes.LemonChiffon;
        }

        private void PrevPage_click(object sender, RoutedEventArgs e)
        {
            Home.states.RemoveAt(Home.states.Count - 1);
            InstructionSwitcher.Switch(Home.states[Home.states.Count - 1]);
        }

    }
}
