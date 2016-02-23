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
    /// Interaction logic for TestPartTwo02.xaml
    /// </summary>
    public partial class TestPartTwo02 : Window
    {
        public TestPartTwo02()
        {
            InitializeComponent();
        }

        private void R01_MouseEnter(object sender, MouseEventArgs e)
        {
            R01.Background = Brushes.Yellow;
        }

        private void R01_MouseLeave(object sender, MouseEventArgs e)
        {
            R01.ClearValue(Run.BackgroundProperty);
        }
    }
}
