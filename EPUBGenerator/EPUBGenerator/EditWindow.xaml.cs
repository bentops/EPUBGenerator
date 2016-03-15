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
using MBox = System.Windows.MessageBox;
using NAudio.Wave;
using EPUBGenerator.MainLogic.SoundEngine;

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
        
        private TreeViewItem _AllContentsTVI;
        private TreeViewItem _AllImagesTVI;

        private ProjectInfo ProjectInfo { get; set; }
        private String ProjectName { get { return ProjectInfo.ProjectName; } }
        private String ProjectPath { get { return ProjectInfo.ProjectPath; } }
        private BlockCollection Paragraphs { get { return richTextBox.Document.Blocks; } }

        private State _State;
        public State CurrentState
        {
            get { return _State; }
            private set
            {
                #region Change CurrentState From
                switch (_State)
                {
                    case State.Stop:
                        break;
                    case State.Play:
                        break;
                    case State.Edit:
                        foreach (Paragraph paragraph in Paragraphs)
                            foreach (RunWord run in paragraph.Inlines)
                                run.EditModeBackground = run.Background;
                        break;
                }
                #endregion

                #region Change CurrentState To
                _State = value;
                switch(_State)
                {
                    case State.Stop:
                        foreach (Paragraph paragraph in Paragraphs)
                            foreach (RunWord run in paragraph.Inlines)
                                run.UsingNormalModeBackground();

                        Dispatcher.Invoke((Action)(() =>
                        {
                            richTextBox.CaretBrush = Brushes.Transparent;
                            richTextBox.IsReadOnlyCaretVisible = false;
                            playpauseB.Content = FindResource("Play");
                        }));
                        break;
                    case State.Play:
                        playpauseB.Content = FindResource("Pause");
                        break;
                    case State.Edit:
                        foreach (Paragraph paragraph in Paragraphs)
                            foreach (RunWord run in paragraph.Inlines)
                                run.UsingEditModeBackground();
                        richTextBox.CaretBrush = null;
                        richTextBox.IsReadOnlyCaretVisible = true;
                        break;
                }
                #endregion
            }
        }

        private bool _Saved;
        public bool IsSaved
        {
            get { return _Saved; }
            set
            {
                _Saved = value;
                Export.IsEnabled = value;
            }
        }

        private RunWord _CurrentWord;
        private RunWord CurrentRunWord
        {
            get { return _CurrentWord; }
            set
            {
                if (_CurrentWord == value)
                    return;

                if (_CurrentWord != null)
                {
                    _CurrentWord.IsSelected = false;
                    _CurrentWord.UsingNormalModeBackground();
                }

                _CurrentWord = value;
                if (_CurrentWord != null)
                {
                    _CurrentWord.IsSelected = true;
                    _CurrentWord.UsingNormalModeBackground();
                }
            }
        }

        private CachedSoundSampleProvider PlayingSound;

        private Content SelectedContent { get; set; }
        private TreeViewItem SelectedTVI { get; set; }

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
            IsSaved = true;
            ProjectInfo = new ProjectInfo(epubProjPath);

            projInfoTextBlock.Text = ProjectName;
            GenerateProjectMenu();

            if (_AllContentsTVI.Items.Count > 0)
            {
                TreeViewItem firstContent = _AllContentsTVI.Items.GetItemAt(0) as TreeViewItem;
                firstContent.IsSelected = true;
            }
        }
        
        private void GenerateProjectMenu()
        {
            TreeViewItem projectTVI = new TreeViewItem() { Header = "Project '" + ProjectName + "'", IsExpanded = true };

            _AllContentsTVI = new TreeViewItem() { Header = "Contents", IsExpanded = true };
            foreach (String contentSrc in ProjectInfo.ContentList.Values)
            {
                String contentName = Path.GetFileNameWithoutExtension(contentSrc);
                TreeViewItem contentTVI = new TreeViewItem() { Header = contentName, Tag = contentSrc };
                contentTVI.Selected += ContentTVI_Selected;
                _AllContentsTVI.Items.Add(contentTVI);
            }
            projectTVI.Items.Add(_AllContentsTVI);

            _AllImagesTVI = new TreeViewItem() { Header = "Images" };
            projectTVI.Items.Add(_AllImagesTVI);

            treeView.Items.Add(projectTVI);
        }
        
        private void ContentTVI_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem contentTVI = sender as TreeViewItem;
            String contentPath = contentTVI.Tag as String;

            if (SelectedTVI != null && !IsSaved)
            {
                if (SelectedTVI != contentTVI)
                {
                    // SHOW WARNING TO SAVE
                    MBox.Show("Please Save Your Project First");
                    contentTVI.IsSelected = false;
                    SelectedTVI.IsSelected = true;
                }
                return;
            }
            Cursor = Cursors.Wait;

            if (SelectedContent != null && Paragraphs.Count > 0)
            {
                ParagraphBlock[] pList = new ParagraphBlock[Paragraphs.Count];
                Paragraphs.CopyTo(pList, 0);
                foreach (ParagraphBlock par in pList)
                    Paragraphs.Remove(par);
            }

            if (SelectedTVI != null)
                SelectedTVI.IsSelected = false;

            CurrentState = State.Stop;
            SelectedTVI = contentTVI;
            SelectedContent = new Content(contentPath, ProjectInfo);
            Console.WriteLine("Sentences Count = " + SelectedContent.SentenceCount);

            int toggle = 0;
            foreach (Block block in SelectedContent.Blocks)
            {
                Paragraph paragraph = new Paragraph();
                foreach (Sentence sentence in block.Sentences)
                {
                    Console.WriteLine("S: " + sentence.ID);
                    sentence.GetCachedSound();
                    foreach (Word word in sentence.Words)
                    {
                        RunWord run = new RunWord(word);
                        run.MouseEnter += Run_MouseEnter;
                        run.MouseLeave += Run_MouseLeave;
                        run.MouseDown += Run_MouseDown;
                        run.MouseMove += Run_MouseMove;

                        run.IsEdited = false;
                        run.EditModeBackground = ProjectProperties.CutWords[toggle];
                        toggle = 1 - toggle;
                            
                        paragraph.Inlines.Add(run);
                    }
                }
                Paragraphs.Add(paragraph);
            }
            CurrentRunWord = GetFirstRunWord();
            Cursor = Cursors.Arrow;
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
                        // WAIT FOR EDIT HERE
                        CurrentState = State.Edit;
                    }
                    break;
                case State.Edit:
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                        CurrentState = State.Stop;
                    break;
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
            Cursor = Cursors.Wait;
            RunWord curRun = sender as RunWord;
            switch (CurrentState)
            {
                case State.Stop:
                    CurrentRunWord = curRun;
                    curRun.PlayCachedSound();
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
                        {
                            curRun.MergeWithNext();
                            IsSaved = false;
                        }
                        else if (contextPrev == TextPointerContext.ElementStart)
                        {
                            curRun.MergeWithPrev();
                            IsSaved = false;
                        }
                        // SPLIT
                        else if (contextPrev == contextNext && contextNext == TextPointerContext.Text)
                        {
                            RunWord newRun = curRun.SplitAt(curPointer);
                            newRun.MouseEnter += Run_MouseEnter;
                            newRun.MouseLeave += Run_MouseLeave;
                            newRun.MouseDown += Run_MouseDown;
                            newRun.MouseMove += Run_MouseMove;
                            newRun.ApplyAvailableBrush(ProjectProperties.SplittedWords);
                            IsSaved = false;
                        }
                    }
                    break;
            }
            Cursor = Cursors.Arrow;
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
                    run.Background = run.NormalModeBackground;
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
            switch (CurrentState)
            {
                case State.Stop:
                    if (CurrentRunWord == null)
                        return;
                    CurrentState = State.Play;
                    PlaySound();
                    break;
                case State.Play:
                    CurrentState = State.Stop;
                    PlayingSound.Position = PlayingSound.EndPosition;
                    break;
                case State.Edit:
                    break;
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
            if (IsSaved)
                return;
            Cursor = Cursors.Wait;
            SelectedContent.Save();
            int toggle = 0;
            foreach (Paragraph paragraph in Paragraphs)
                foreach (RunWord run in paragraph.Inlines)
                {
                    run.IsEdited = false;
                    run.EditModeBackground = ProjectProperties.CutWords[toggle];
                    toggle = 1 - toggle;

                    run.Background = (CurrentState == State.Edit) ? run.EditModeBackground : run.NormalModeBackground;
                }
            IsSaved = true;
            Cursor = Cursors.Arrow;
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
        
        private void PlaySound()
        {
            PlayingSound = CurrentRunWord.GetSentenceCachedSound();
            PlayingSound.PositionChanged += Sound_PositionChanged;
            AudioPlaybackEngine.Instance.PlaySound(PlayingSound);
            Console.WriteLine("Start Play");
        }

        private void Sound_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            switch (CurrentState)
            {
                case State.Stop:
                    break;
                case State.Play:
                    if (e.IsEnded)
                    {
                        Console.WriteLine("Ended In");
                        RunWord nextRun = GetNextRunWord();
                        if (nextRun == null)
                        {
                            Console.WriteLine("T 1");
                            CurrentState = State.Stop;
                        }
                        else
                        {
                            Console.WriteLine("T 2");
                            CurrentRunWord = nextRun;
                            PlaySound();
                        }
                        Console.WriteLine("Ended Out");
                    }
                    else if (!CurrentRunWord.IsIn(e.BytePosition))
                    {
                        Console.WriteLine("ChangeWord");
                        do { CurrentRunWord = GetNextRunWord(); }
                        while (CurrentRunWord != null && !CurrentRunWord.IsIn(e.BytePosition));

                        if (CurrentRunWord == null)
                            CurrentRunWord = GetFirstRunWord();
                    }
                    break;
                case State.Edit:
                    break;
            }
        }
        
        private RunWord GetFirstRunWord()
        {
            Paragraph firstParagraph = Paragraphs.FirstBlock as Paragraph;
            if (firstParagraph == null)
                return null;
            return firstParagraph.Inlines.FirstInline as RunWord;
        }

        private RunWord GetNextRunWord()
        {
            if (CurrentRunWord == null)
                return GetFirstRunWord();
            if (CurrentRunWord.NextRun != null)
                return CurrentRunWord.NextRun;
            Paragraph paragraph = CurrentRunWord.Parent as Paragraph;
            if (paragraph.NextBlock != null)
                return (paragraph.NextBlock as Paragraph).Inlines.FirstInline as RunWord;
            return null;
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
