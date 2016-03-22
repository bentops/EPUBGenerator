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
        public static CreateProj1 createProj1;
        public static CreateProj2 createProj2;
        public static CreateProj2e createProj2e;
        public static CreateProj3 createProj3;
        public static CreateProj4 createProj4;

        public static EditProj1 editProj1;
        public static EditProj2 editProj2;
        public static EditProj3M editProj3M;
        public static EditProj4 editProj4;
        public static EditProj5 editProj5;
        public static EditProj6 editProj6;

        public static ExportProj1 exportProj1;
        public static ExportProj2 exportProj2;
        public static ExportProj3 exportProj3;
        public static ExportProj4 exportProj4;

        public static void Switch(Page newPage)
        {
            pageSwitcher.Navigate(newPage);
        }
    }
}
