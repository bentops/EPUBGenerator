﻿using System;
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
    public partial class EditProj7 : Page
    {
        public EditProj7()
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
        //      Home.states.Add(InstructionSwitcher.editProj7);
        //    Home.states.ForEach(Console.WriteLine);
        //    InstructionSwitcher.Switch(InstructionSwitcher.editProj7);
        //}

        private void PrevPage_click(object sender, RoutedEventArgs e)
        {
            Home.states.RemoveAt(Home.states.Count - 1);
            InstructionSwitcher.Switch(Home.states[Home.states.Count - 1]);
        }

    }
}
