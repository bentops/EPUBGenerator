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
using Xceed.Wpf.Toolkit;
using Path = System.IO.Path;
using ParagraphBlock = System.Windows.Documents.Block;
using Block = EPUBGenerator.MainLogic.Block;
using EPUBGenerator.MainLogic;
using System.Xml.Linq;
using System.IO;
using System.ComponentModel;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using DResult = System.Windows.Forms.DialogResult;

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>

    public enum State { Stop, Play, Edit }

    public partial class EditWindow : Window
    {
        private LogicalDirection GoForward = LogicalDirection.Forward;
        private LogicalDirection GoBackward = LogicalDirection.Backward;


        private TreeViewItem contentsTVI;
        private TreeViewItem imagesTVI;

        private State _State;
        private State CurrentState
        {
            get { return _State; }
            set
            {
                _State = value;
                switch(_State)
                {
                    case State.Stop:
                        foreach (Paragraph paragraph in Paragraphs)
                            foreach (RunWord run in paragraph.Inlines)
                                run.ClearBackground();
                        richTextBox.CaretBrush = Brushes.Transparent;
                        richTextBox.IsReadOnlyCaretVisible = true;
                        break;
                    case State.Play:
                        break;
                    case State.Edit:
                        foreach (Paragraph paragraph in Paragraphs)
                            foreach (RunWord run in paragraph.Inlines)
                                run.ApplyAvailableBrush(ProjectProperties.CutWords);
                        richTextBox.CaretBrush = null;
                        richTextBox.IsReadOnlyCaretVisible = false;
                        break;
                }
            }
        }

        private ProjectInfo ProjectInfo { get; set; }
        private String ProjectName { get { return ProjectInfo.ProjectName; } }
        private String ProjectPath { get { return ProjectInfo.ProjectPath; } }
        private Content SelectedContent { get; set; }
        private RunWord CurrentRunWord { get; set; }
        private BlockCollection Paragraphs { get { return richTextBox.Document.Blocks; } }

        private String _ExportPath;

        public EditWindow()
        {
            InitializeComponent();
        }

        public EditWindow(String epubProjPath)
        {
            InitializeComponent();
            Initiate(epubProjPath);
        }

        public void Initiate(String epubProjPath)
        {
            this.PreviewKeyDown += EditWindow_KeyDown;
            richTextBox.IsReadOnly = true;

            CurrentState = State.Stop;
            ProjectInfo = new ProjectInfo(epubProjPath);

            projInfoTextBlock.Text = ProjectName;
            GenerateProjectMenu();

            if (contentsTVI.Items.Count > 0)
            {
                TreeViewItem firstContent = contentsTVI.Items.GetItemAt(0) as TreeViewItem;
                firstContent.IsSelected = true;
            }

        }

        private void EditWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            switch (CurrentState)
            {
                case State.Stop:
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                        CurrentState = State.Edit;
                    break;
                case State.Play:
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                    {

                        CurrentState = State.Edit;
                    }
                    break;
                case State.Edit:
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                        CurrentState = State.Stop;
                    break;
            }
        }
        

        private void GenerateProjectMenu()
        {
            TreeViewItem projectTVI = new TreeViewItem() { Header = "Project '" + ProjectName + "'", IsExpanded = true };

            contentsTVI = new TreeViewItem() { Header = "Contents", IsExpanded = true };
            foreach (String contentSrc in ProjectInfo.ContentList.Values)
            {
                String contentName = Path.GetFileNameWithoutExtension(contentSrc);
                TreeViewItem contentTVI = new TreeViewItem() { Header = contentName, Tag = contentSrc };
                contentTVI.Selected += ContentTVI_Selected;
                contentsTVI.Items.Add(contentTVI);
            }
            projectTVI.Items.Add(contentsTVI);

            imagesTVI = new TreeViewItem() { Header = "Images" };
            projectTVI.Items.Add(imagesTVI);

            treeView.Items.Add(projectTVI);
        }

        private void ContentTVI_Selected(object sender, RoutedEventArgs e)
        {
            if (SelectedContent != null && Paragraphs.Count > 0)
            {
                ParagraphBlock[] pList = new ParagraphBlock[Paragraphs.Count];
                Paragraphs.CopyTo(pList, 0);
                foreach (ParagraphBlock par in pList)
                    Paragraphs.Remove(par);
            }

            CurrentState = State.Stop;

            TreeViewItem contentTVI = sender as TreeViewItem;
            String contentPath = contentTVI.Tag as String;
            SelectedContent = new Content(contentPath, ProjectInfo);
            Console.WriteLine("Sentences Count = " + SelectedContent.SentenceCount);

            foreach (Block block in SelectedContent.Blocks)
            {
                Paragraph paragraph = new Paragraph();
                foreach (Sentence sentence in block.Sentences)
                {
                    foreach (Word word in sentence.Words)
                    {
                        RunWord run = new RunWord(word);
                        run.MouseEnter += Run_MouseEnter;
                        run.MouseLeave += Run_MouseLeave;
                        run.MouseDown += Run_MouseDown;
                        run.MouseMove += Run_MouseMove;

                        paragraph.Inlines.Add(run);
                    }
                }
                Paragraphs.Add(paragraph);
            }
        }

        private void Run_MouseMove(object sender, MouseEventArgs e)
        {
            RunWord run = sender as RunWord;
            switch (CurrentState)
            {
                case State.Stop:
                    run.Cursor = Cursors.Hand;
                    break;
                case State.Play:
                    run.Cursor = Cursors.Hand;
                    break;
                case State.Edit:
                    TextPointer pointer = richTextBox.GetPositionFromPoint(e.GetPosition(run), false);
                    richTextBox.CaretPosition = pointer;
                    TextPointerContext contextPrev = pointer.GetPointerContext(GoBackward);
                    TextPointerContext contextNext = pointer.GetPointerContext(GoForward);
                    if (contextNext == TextPointerContext.ElementEnd || contextPrev == TextPointerContext.ElementStart)
                        run.Cursor = Cursors.No;
                    else if (contextNext == contextPrev && contextNext == TextPointerContext.Text)
                        run.Cursor = Cursors.UpArrow;
                    else
                        run.Cursor = Cursors.Arrow;
                    break;
            }
        }

        private void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RunWord curRun = sender as RunWord;
            switch (CurrentState)
            {
                case State.Stop:
                    break;
                case State.Play:
                    break;
                case State.Edit:
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        TextPointer curPointer = richTextBox.GetPositionFromPoint(e.GetPosition(curRun), false);
                        if (curPointer == null)
                            return;

                        TextPointerContext contextPrev = curPointer.GetPointerContext(GoBackward);
                        TextPointerContext contextNext = curPointer.GetPointerContext(GoForward);

                        // MERGE
                        if (contextNext == TextPointerContext.ElementEnd)
                            curRun.MergeWithNext();
                        else if (contextPrev == TextPointerContext.ElementStart)
                            curRun.MergeWithPrev();
                        // SPLIT
                        else if (contextPrev == contextNext && contextNext == TextPointerContext.Text)
                        {
                            RunWord newRun = curRun.SplitAt(curPointer);
                            newRun.MouseEnter += Run_MouseEnter;
                            newRun.MouseLeave += Run_MouseLeave;
                            newRun.MouseDown += Run_MouseDown;
                            newRun.MouseMove += Run_MouseMove;
                            newRun.ApplyAvailableBrush(ProjectProperties.SplittedWords);
                        }
                    }
                    break;
            }
        }

        private void Run_MouseEnter(object sender, MouseEventArgs e)
        {
            RunWord run = sender as RunWord;
            switch (CurrentState)
            {
                case State.Stop:
                    run.Background = ProjectProperties.HoveredWord;
                    break;
                case State.Play:
                    break;
                case State.Edit:
                    break;
            }
        }

        private void Run_MouseLeave(object sender, MouseEventArgs e)
        {
            RunWord run = sender as RunWord;
            switch (CurrentState)
            {
                case State.Stop:
                    run.ClearBackground();
                    break;
                case State.Play:
                    break;
                case State.Edit:
                    break;
            }
        }

        private void ProjInfo_Loaded(object sender, RoutedEventArgs e)
        {
            // Get TextBlock reference.
            var block = sender as TextBlock;
            // Set text.
            //block.Text = "Project Location : \t" + ProjectPath;
        }

        private string GetProjectFileName(string projPath)
        {
            return Path.GetFileNameWithoutExtension(projPath);
        }

        private void forwardB_Click(object sender, RoutedEventArgs e)
        {

        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void browseLocation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void apply_Click(object sender, RoutedEventArgs e)
        {

        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void playpauseB_Click(object sender, RoutedEventArgs e)
        {
            if (playpauseB.Content == FindResource("Play"))
            {
                playpauseB.Content = FindResource("Pause");

            }
            else
            {
                playpauseB.Content = FindResource("Play");
            }
        }

        private void stopB_Click(object sender, RoutedEventArgs e)
        {
            if (playpauseB.Content == FindResource("Pause"))
            {
                playpauseB.Content = FindResource("Play");
                playpauseB.IsChecked = false;
            }
        }

        private void saveBook_Click(object sender, RoutedEventArgs e)
        {
            SelectedContent.Save();
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = ProjectPath;
                saveFileDialog.Filter = "EPUB files (*.epub)|*.epub";
                if (saveFileDialog.ShowDialog() == DResult.OK)
                {
                    _ExportPath = saveFileDialog.FileName;

                    BackgroundWorker bw = new BackgroundWorker();
                    bw.WorkerReportsProgress = true;
                    bw.WorkerSupportsCancellation = true;
                    bw.DoWork += bw_DoWork;
                    bw.ProgressChanged += bw_ProgressChanged;
                    bw.RunWorkerCompleted += bw_RunWorkerCompleted;
                    bw.RunWorkerAsync();
                }
            }
            
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressUpdater _ProgressUpdater = new ProgressUpdater(sender as BackgroundWorker, e);
            Project.Export(ProjectInfo.EpubProjectPath, _ExportPath, _ProgressUpdater);
        }


        //private void apply_Click(object sender, RoutedEventArgs e)
        //{
        //    Console.WriteLine(comboBox.Text);
        //}

        //private void browseLocation_Click(object sender, RoutedEventArgs e)
        //{
        //    var textBox = (comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox);
        //    if (textBox != null)
        //    {
        //        textBox.SelectAll();
        //        comboBox.Focus();
        //        e.Handled = true;
        //    }

        //}
    }
}
