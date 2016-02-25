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
        int count = 0;
        public TestPartTwo02()
        {
            InitializeComponent();
            Random rnd = new Random();

            for (int j =0; j<4; j++)
            {
                Paragraph paragraph = new Paragraph();
                for (int i = 0; i < 20; i++)
                {
                    Run run = new Run();
                    run.Text = i + "create_from_c# ";
                    run.Background = PickRandomBrush(rnd);
                    run.MouseEnter += R01_MouseEnter;
                    run.MouseLeave += R01_MouseLeave;
                    run.MouseLeftButtonDown += R_MouseLeftButtonDown;
                    run.Cursor = Cursors.Hand;
                    paragraph.Inlines.Add(run);
                }
                richTextBox.Document.Blocks.Add(paragraph);
            }
            
            
        }

        private void R01_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Run).Background = Brushes.Yellow;
        }

        private void R01_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Run).ClearValue(Run.BackgroundProperty);
        }
        private void R_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            (sender as Run).Text = "AAAAA" + count++;
        }

        private Brush PickRandomBrush(Random rnd)
        {
            Brush result = Brushes.Transparent;
            Type brushesType = typeof(Brushes);
            System.Reflection.PropertyInfo[] properties = brushesType.GetProperties();
            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);
            return result;
        }

    }
}
