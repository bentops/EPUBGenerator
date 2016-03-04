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
        Dictionary<string, Brush> colors = new Dictionary<string, Brush>()
        {
            {"splitBlue", (SolidColorBrush)(new BrushConverter().ConvertFrom("#cdeafd")) },
            {"splitBlueD", (SolidColorBrush)(new BrushConverter().ConvertFrom("#84ccfa")) },
            {"splitAdd", (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFDC7F")) },
            {"splitAddD", (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFC833")) },
            {"splitDel", (SolidColorBrush)(new BrushConverter().ConvertFrom("#f1b1ff")) },
            {"splitDelD", (SolidColorBrush)(new BrushConverter().ConvertFrom("#e87fff")) },
            {"editHoverPink", Brushes.Pink },
            {"editPinkD", (SolidColorBrush)(new BrushConverter().ConvertFrom("#ff99a7")) },
            {"playSentence", (SolidColorBrush)(new BrushConverter().ConvertFrom("#ffffcb")) },
            {"playWord", (SolidColorBrush)(new BrushConverter().ConvertFrom("#fdff7f")) }
        };
        int mouseClickCount = 0;

        public TestPartTwo02()
        {
            InitializeComponent();
            Random rnd = new Random();

            //foreach (string val in colors.Values)
            //{
            //    Console.WriteLine(val);
            //}

            Paragraph prg = new Paragraph();
            for(int i=0; i<10; i++)
            {
                    Run[] run = new Run[6];
                    for(int j=0; j<6; j++)
                    {
                        run[j] = new Run();
                        if (j % 2 == 0) run[j].Background = colors["splitBlue"];
                        else run[j].Background = colors["splitBlueD"];
                        //run[j].MouseEnter += R01_MouseEnter;
                        //run[j].MouseLeave += R01_MouseLeave;
                        run[j].MouseLeftButtonDown += R_MouseLeftButtonDown;
                        run[j].Cursor = Cursors.Hand;
                    run[j].FontSize = 20;
                        prg.Inlines.Add(run[j]);
                    }
                    run[0].Text = "นี่";
                    run[1].Text = "คือ";
                    run[2].Text = "ประ";
                    run[3].Text = "โยค";
                    run[4].Text = "ที่ ";
                    run[5].Text = i + " ";
            }

            richTextBox.Document.Blocks.Add(prg);

            Paragraph prg2 = new Paragraph();
            for (int i = 0; i < 10; i++)
            {
                Run[] run = new Run[6];
                for (int j = 0; j < 6; j++)
                {
                    run[j] = new Run();
                    if (j % 2 == 0) run[j].Background = colors["editHoverPink"];
                    else run[j].Background = colors["editPinkD"];
                    //run[j].MouseEnter += R01_MouseEnter;
                    //run[j].MouseLeave += R01_MouseLeave;
                    run[j].MouseLeftButtonDown += R_MouseLeftButtonDown;
                    run[j].Cursor = Cursors.Hand;
                    run[j].FontSize = 20;
                    prg.Inlines.Add(run[j]);
                }
                run[0].Text = "นี่";
                run[1].Text = "คือ";
                run[2].Text = "ประ";
                run[3].Text = "โยค";
                run[4].Text = "ที่ ";
                run[5].Text = i + " ";
            }

            richTextBox.Document.Blocks.Add(prg2);


        }

        private void R01_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Run).Background = Brushes.Yellow;
        }

        private void R01_MouseLeave(object sender, MouseEventArgs e)
        {
            //(sender as Run).ClearValue(Run.BackgroundProperty);
        }
        private void R_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if(mouseClickCount%4 == 0)(sender as Run).Background = colors["splitAdd"];
            else if (mouseClickCount % 4 == 1) (sender as Run).Background = colors["splitAddD"];
            else if (mouseClickCount % 4 == 2) (sender as Run).Background = colors["splitDel"];
            else (sender as Run).Background = colors["splitDelD"];
            mouseClickCount++;
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

        private void richTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void apply_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(comboBox.Text);
        }

        private void browseLocation_Click(object sender, RoutedEventArgs e)
        {
            var textBox = (comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox);
            if (textBox != null)
            {
                textBox.SelectAll();
                comboBox.Focus();
                e.Handled = true;
            }

        }
    }
}
