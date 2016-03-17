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
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public static List<string> states = new List<string>();
        public Home()
        {
            InitializeComponent();
            states.Add("home");
        }

        int state = 1;
        private void instructionMenuMouseOver(object sender, RoutedEventArgs e)
        {
            string senderName = ((Border)sender).Name;
            Brush bg = (SolidColorBrush)(new BrushConverter().ConvertFrom("#29d9c2"));
            Brush fore = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF2F2933"));
            if (senderName == "menu1" && state == 1)
            {
                createNewProject.Foreground = Brushes.Gray;
                createNewProjectTh.Foreground = Brushes.Gray;
                Arr1.Foreground = bg;
                state = 2;
            }
            else if (senderName == "menu1" && state == 2)
            {
                createNewProject.Foreground = fore;
                createNewProjectTh.Foreground = fore;
                Arr1.Foreground = Brushes.Gray;
                state = 1;
            }
            else if (senderName == "menu2" && state == 1)
            {
                OpenEditProject.Foreground = Brushes.Gray;
                OpenEditProjectTh.Foreground = Brushes.Gray;
                Arr2.Foreground = bg;
                state = 2;
            }
            else if (senderName == "menu2" && state == 2)
            {
                OpenEditProject.Foreground = fore;
                OpenEditProjectTh.Foreground = fore;
                Arr2.Foreground = Brushes.Gray;
                state = 1;
            }
            else if (senderName == "menu3" && state == 1)
            {
                Export.Foreground = Brushes.Gray;
                ExportTh.Foreground = Brushes.Gray;
                Arr3.Foreground = bg;
                state = 2;
            }
            else if (senderName == "menu3" && state == 2)
            {
                Export.Foreground = fore;
                ExportTh.Foreground = fore;
                Arr3.Foreground = Brushes.Gray;
                state = 1;
            }
        }

        private void menu1Click(object sender, RoutedEventArgs e)
        {
            Home.states.Add("CreateProj4");
            Home.states.ForEach(Console.WriteLine);
            InstructionSwitcher.Switch(InstructionSwitcher.createProj4);
        }

        private void menu2Click(object sender, RoutedEventArgs e)
        {
            //Home.states.Add("CreateProj1");
            //Home.states.ForEach(Console.WriteLine);
            //InstructionSwitcher.Switch(InstructionSwitcher.createProj1);
        }

        private void menu3Click(object sender, RoutedEventArgs e)
        {
            //Home.states.Add("CreateProj1");
            //Home.states.ForEach(Console.WriteLine);
            //InstructionSwitcher.Switch(InstructionSwitcher.createProj1);
        }

        private void exitbutton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows[1].Close();
        }
    }
}
