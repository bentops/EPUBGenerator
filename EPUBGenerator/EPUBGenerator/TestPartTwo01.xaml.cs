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
    /// Interaction logic for TestPartTwo01.xaml
    /// </summary>
    public partial class TestPartTwo01 : Window
    {
        Brush[] brushes;
        List<String[]> parList;
        ExtRun test1 = new ExtRun();
        ExtRun test2 = new ExtRun();
        ExtRun test3 = new ExtRun();
        
        public TestPartTwo01()
        {
            InitializeComponent();

            brushes = new Brush[] { Brushes.Yellow, Brushes.SkyBlue, Brushes.Lime };
            richTextBox.Focus();

            String text = @"You can construct a Paragraph object manually by adding a object for each line of your text and when ever a line break is needed. You can split your text to lines using String.Split method. สวัสดีครับ เราเคยรู้จัก กันรึเปล่า? หน้าตาคุ้นๆ แค่ผมมองคุณ ยังไม่ค่อยชัด";
            parList = new List<string[]>();
            parList.Add(text.Split(' '));
            parList.Add(text.Split(' '));

            foreach (String[] par in parList)
            {
                Paragraph paragraph = new Paragraph();
                foreach (String word in par)
                {
                    Run run = new Run();
                    run.Text = word + " ";
                    run.MouseMove += run_MouseMove;
                    //run.MouseEnter += run_MouseEnter;
                    //run.MouseLeave += run_MouseLeave;
                    run.MouseDown += run_MouseDown;
                    paragraph.Inlines.Add(run);
                    GetAvailableBrush(run);
                    
                    /*Run splitter = new Run();
                    splitter.Text = "";
                    //splitter.MouseMove += run_MouseMove;
                    splitter.MouseEnter += splitter_MouseEnter;
                    splitter.MouseLeave += splitter_MouseLeave;
                    paragraph.Inlines.Add(splitter);*/
                }
                richTextBox.Document.Blocks.Add(paragraph);
            }
            richTextBox.Document.Blocks.Add(new Paragraph(test1));
            richTextBox.Document.Blocks.Add(new Paragraph(test2));
            richTextBox.Document.Blocks.Add(new Paragraph(test3));

            TextPointer tp = richTextBox.Document.ContentStart;
            TextPointer start = tp.DocumentStart;
            LogicalDirection fw = LogicalDirection.Forward;
            for (int i = 0; i < 20; i++)
            {
                TextPointer np = tp;
                Console.Write("Itself: ");
                Console.WriteLine(np.GetOffsetToPosition(start) + ", " + np.GetTextInRun(fw) + ", " + np.GetPointerContext(fw));
                np = tp.GetNextContextPosition(fw);
                Console.Write("NextContext: ");
                Console.WriteLine(np.GetOffsetToPosition(start) + ", " + np.GetTextInRun(fw) + ", " + np.GetPointerContext(fw));
                np = tp.GetNextInsertionPosition(fw);
                Console.Write("NextInsertion: ");
                Console.WriteLine(np.GetOffsetToPosition(start) + ", " + np.GetTextInRun(fw) + ", " + np.GetPointerContext(fw));
                np = tp.GetInsertionPosition(fw);
                Console.Write("Insertion: ");
                Console.WriteLine(np.GetOffsetToPosition(start) + ", " + np.GetTextInRun(fw) + ", " + np.GetPointerContext(fw));
                np = tp.GetLineStartPosition(0);
                Console.Write("LineStart: ");
                Console.WriteLine(np.GetOffsetToPosition(start) + ", " + np.GetTextInRun(fw) + ", " + np.GetPointerContext(fw));

                Console.WriteLine(" --------------------------------------- ");
                tp = tp.GetPositionAtOffset(1);
            }
        }

        private void run_MouseMove(object sender, MouseEventArgs e)
        {
            Run run = sender as Run;
            TextPointer pointer = richTextBox.GetPositionFromPoint(e.GetPosition(run), true);

            test1.Text = pointer.GetTextInRun(LogicalDirection.Forward) + " " + pointer.GetOffsetToPosition(pointer.DocumentStart);
            TextPointer p2 = pointer.GetNextContextPosition(LogicalDirection.Forward);
            test1.Text += "\n" + p2.GetTextInRun(LogicalDirection.Forward) + " " + p2.GetOffsetToPosition(pointer.DocumentStart);
            TextPointer p3 = pointer.GetNextInsertionPosition(LogicalDirection.Forward);
            test1.Text += "\n" + p3.GetTextInRun(LogicalDirection.Forward) + " " + p3.GetOffsetToPosition(pointer.DocumentStart);

            richTextBox.CaretPosition = pointer;
            int c = richTextBox.CaretPosition.GetOffsetToPosition(pointer.DocumentStart);
            test2.Text = c.ToString();

            test3.Text = run.ContentStart.GetOffsetToPosition(pointer.DocumentStart) + " " + run.ContentEnd.GetOffsetToPosition(pointer.DocumentStart);


            TextPointerContext contextPrev = pointer.GetPointerContext(LogicalDirection.Backward);
            TextPointerContext contextNext = pointer.GetPointerContext(LogicalDirection.Forward);
            if (contextNext == TextPointerContext.ElementEnd || contextPrev == TextPointerContext.ElementStart)
                run.Cursor = Cursors.Hand;
            else if (contextNext == contextPrev && contextNext == TextPointerContext.Text)
                run.Cursor = Cursors.UpArrow;
            else
                run.Cursor = Cursors.Arrow;
        }

        private void run_MouseEnter(object sender, MouseEventArgs e)
        {
            Run run = sender as Run;
            run.Background = Brushes.Yellow;
        }

        private void run_MouseLeave(object sender, MouseEventArgs e)
        {
            Run run = sender as Run;
            run.Background = Brushes.White;
        }

        private void run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            Run curRun = sender as Run;
            TextPointer curPointer = richTextBox.GetPositionFromPoint(e.GetPosition(curRun), false);
            if (curPointer == null)
                return;

            TextPointerContext contextPrev = curPointer.GetPointerContext(LogicalDirection.Backward);
            TextPointerContext contextNext = curPointer.GetPointerContext(LogicalDirection.Forward);

            if (contextNext == TextPointerContext.ElementEnd)
            {
                Run nextRun = curRun.NextInline as Run;
                if (nextRun == null)
                    return;

                curRun.Text += nextRun.Text;
                curRun.ElementStart.Paragraph.Inlines.Remove(nextRun);
                GetAvailableBrush(curRun);
            }
            else if (contextPrev == TextPointerContext.ElementStart)
            {
                Run prevRun = curRun.PreviousInline as Run;
                if (prevRun == null)
                    return;

                curRun.Text = prevRun.Text + curRun.Text;
                curRun.ElementStart.Paragraph.Inlines.Remove(prevRun);
                GetAvailableBrush(curRun);
            }
            else if (contextPrev == contextNext && contextNext == TextPointerContext.Text)
            {
                TextPointer insertPos = curPointer.GetNextContextPosition(LogicalDirection.Forward);
                while (insertPos.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.ElementEnd)
                    insertPos = insertPos.GetNextContextPosition(LogicalDirection.Forward);
                if (insertPos == null)
                    return;

                String cutText = curPointer.GetTextInRun(LogicalDirection.Forward);
                Run newRun = new Run(cutText, insertPos);
                newRun.MouseMove += run_MouseMove;
                newRun.MouseDown += run_MouseDown;
                GetAvailableBrush(newRun);

                curRun.Text = curRun.Text.Substring(0, curRun.Text.Length - cutText.Length);
            }
        }

        private void GetAvailableBrush(Run run)
        {
            Inline next = run.NextInline;
            Inline prev = run.PreviousInline;
            Brush cur = run.Background;
            if (cur != null)
                if (next == null || !next.Background.Equals(cur))
                    if (prev == null || !prev.Background.Equals(cur))
                        return;

            foreach (Brush brush in brushes)
                if (next == null || !next.Background.Equals(brush))
                    if (prev == null || !prev.Background.Equals(brush))
                    {
                        run.Background = brush;
                        return;
                    }
        }
    }

    class ExtRun : Run
    {
        
    }
}
