using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        int test = 1;
        public MainWindow()
        {
            InitializeComponent();
            Console.WriteLine("MainWindow, Current Thread: " + Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine(Application.Current.StartupUri);
            Switcher.pageSwitcher = this;
            Switcher.home = new Home();
            Switcher.createBook1 = new CreateBook1();
            Switcher.createBook2 = new CreateBook2();
            Switcher.createBook3 = new CreateBook3();
            Switcher.editBook1 = new EditBook1();
            Switcher.error = new Error();
            if (test == 0)
                Switcher.Switch(Switcher.home);
            else if (test == 1)
            {
                Switcher.Switch(Switcher.createBook2);
                String epubPath = @"C:\Users\xinghbtong.Baitongs\Documents\Top\Chula\Year 4\Senior Project\Project\EPUB\EpubFiles\Tester_01.epub";
                String projPath = @"C:\Users\xinghbtong.Baitongs\Desktop\TestEPUB\T01";
                if (System.IO.Directory.Exists(projPath))
                    System.IO.Directory.Delete(projPath, true);
                Switcher.createBook2.createEPUB(epubPath, projPath, "T01");
            }
            else if (test == 2)
            {
                String epubProjPath = @"C:\Users\xinghbtong.Baitongs\Desktop\TestEPUB\T01\T01.epubproj";
                new EditWindow(epubProjPath).Show();
                Close();
            }
        }

        public void Navigate(UserControl nextPage)
        {
            this.Content = nextPage;
        }

    }
}
