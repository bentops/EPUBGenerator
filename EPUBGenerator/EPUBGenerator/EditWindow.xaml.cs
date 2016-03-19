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
using System.Threading;

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>

    public enum State { Stop, Play, Segment, Edit }

    public partial class EditWindow : Window
    {
        private LogicalDirection GoForward = LogicalDirection.Forward;
        private LogicalDirection GoBackward = LogicalDirection.Backward;
        
        private TreeViewItem _AllContentsTVI;
        private TreeViewItem _AllImagesTVI;
        private String _ExportPath;

        private ProjectInfo ProjectInfo { get; set; }
        private String ProjectName { get { return ProjectInfo.ProjectName; } }
        private String ProjectPath { get { return ProjectInfo.ProjectPath; } }
        private BlockCollection Paragraphs { get { return richTextBox.Document.Blocks; } }

        private bool IsSaved
        {
            get { return ProjectInfo.IsSaved; }
            set
            {
                ProjectInfo.IsSaved = value;
                Export.IsEnabled = value;
            }
        }
        private State CurrentState
        {
            get { return ProjectInfo.CurrentState; }
            set
            {
                #region Change CurrentState From
                State oldState = ProjectInfo.CurrentState;
                switch (oldState)
                {
                    case State.Stop:
                        break;
                    case State.Play:
                        break;
                    case State.Segment:
                        break;
                    case State.Edit:
                        break;
                }
                #endregion

                #region Change CurrentState To
                ProjectInfo.CurrentState = value;
                foreach (Paragraph paragraph in Paragraphs)
                    foreach (RunWord run in paragraph.Inlines)
                        run.UpdateBackground();
                switch (value)
                {
                    case State.Stop:
                        Dispatcher.Invoke((Action)(() =>
                        {
                            richTextBox.CaretBrush = Brushes.Transparent;
                            richTextBox.IsReadOnlyCaretVisible = false;
                            playpauseB.Content = FindResource("Play");
                            comboBox.IsEnabled = false;
                        }));
                        break;
                    case State.Play:
                        playpauseB.Content = FindResource("Pause");
                        comboBox.IsEnabled = false;
                        break;
                    case State.Segment:
                        richTextBox.CaretBrush = null;
                        richTextBox.IsReadOnlyCaretVisible = true;
                        comboBox.IsEnabled = false;
                        break;
                    case State.Edit:
                        comboBox.IsEnabled = true;
                        while (comboBox.Items.Count > 0)
                            comboBox.Items.RemoveAt(0);
                        String runText = CurrentRunWord.Text;
                        if (ProjectInfo.Dictionary.ContainsKey(runText))
                            foreach (String pronun in ProjectInfo.Dictionary[runText])
                                comboBox.Items.Add(pronun);
                        else
                            comboBox.Items.Add(runText);
                        comboBox.SelectedIndex = CurrentRunWord.Word.DictIndex;
                        break;
                }
                #endregion
            }
        }
        private RunWord CurrentRunWord { get { return ProjectInfo.CurrentRunWord; } }

        private CachedSoundSampleProvider PlayingSound;

        private Content SelectedContent { get; set; }
        private TreeViewItem SelectedTVI { get; set; }


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
            Console.WriteLine("EditWindow, Current Thread: " + Thread.CurrentThread.ManagedThreadId);
            this.PreviewKeyDown += EditWindow_KeyDown;
            richTextBox.IsReadOnly = true;
            richTextBox.SelectionBrush = ProjectProperties.Transparent;
           
            ProjectInfo = new ProjectInfo(epubProjPath);
            IsSaved = true;
            CurrentState = State.Stop;

            projInfoTextBlock.Text = ProjectName;
            GenerateProjectMenu();

            if (_AllContentsTVI.Items.Count > 0)
            {
                TreeViewItem firstContent = _AllContentsTVI.Items.GetItemAt(0) as TreeViewItem;
                firstContent.IsSelected = true;
            }

            Show();
        }
        
        private void GenerateProjectMenu()
        {
            TreeViewItem projectTVI = new TreeViewItem() { Header = "Project '" + ProjectName + "'", IsExpanded = true };

            _AllContentsTVI = new TreeViewItem() { Header = "Contents", IsExpanded = true };
            foreach (String contentSrc in ProjectInfo.ContentList.Values)
            {
                String contentName = Path.GetFileNameWithoutExtension(contentSrc);
                TreeViewItem contentTVI = new TreeViewItem() { Header = contentName, Tag = contentSrc };
                _AllContentsTVI.Items.Add(contentTVI);
            }
            projectTVI.Items.Add(_AllContentsTVI);

            _AllImagesTVI = new TreeViewItem() { Header = "Images" };
            projectTVI.Items.Add(_AllImagesTVI);

            treeView.Items.Add(projectTVI);
            treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem oldTVI = e.OldValue as TreeViewItem;
            TreeViewItem newTVI = e.NewValue as TreeViewItem;
            Console.WriteLine("OLD: " + oldTVI + ", NEW: " + newTVI);
            if (newTVI == null) return;
            Console.WriteLine("New Tag: " + newTVI.Tag);
            if (newTVI.Tag == null) return;

            if (SelectedTVI != null && !IsSaved)
            {
                Console.WriteLine("In1");
                if (SelectedTVI != newTVI)
                {
                    Console.WriteLine("In2");
                    // SHOW WARNING TO SAVE
                    String message = "Want to save your changes?\nIf you click \"No\", your recent changes will be lost.";
                    MessageBoxResult mResult = MBox.Show(message, "EPUBGenerator", MessageBoxButton.YesNoCancel);
                    if (mResult == MessageBoxResult.Cancel)
                        return;
                    if (mResult == MessageBoxResult.Yes)
                        saveBook_Click(new object(), new RoutedEventArgs());
                }
                else return;
                Console.WriteLine("In3");
            }

            Cursor = Cursors.Wait;
            if (SelectedContent != null && Paragraphs.Count > 0)
            {
                ParagraphBlock[] pList = new ParagraphBlock[Paragraphs.Count];
                Paragraphs.CopyTo(pList, 0);
                foreach (ParagraphBlock par in pList)
                    Paragraphs.Remove(par);
            }

            IsSaved = true;
            CurrentState = State.Stop;
            SelectedTVI = newTVI;
            SelectedContent = new Content(newTVI.Tag as String, ProjectInfo);
            Console.WriteLine("Sentences Count = " + SelectedContent.SentenceCount);

            foreach (Block block in SelectedContent.Blocks)
            {
                Paragraph paragraph = new Paragraph();
                Paragraphs.Add(paragraph);
                foreach (Sentence sentence in block.Sentences)
                {
                    Console.WriteLine("S: " + sentence.ID);
                    sentence.GetCachedSound();
                    foreach (Word word in sentence.Words)
                    {
                        RunWord run = new RunWord(word);
                        paragraph.Inlines.Add(run);
                        InitiateRun(run);
                        run.ApplyAvailableSegmentedBackground(ProjectProperties.SegmentedWords);
                    }
                }
            }
            SelectFirstRunWord();
            Cursor = Cursors.Arrow;
        }
        
        private void InitiateRun(RunWord run)
        {
            run.MouseEnter += Run_MouseEnter;
            run.MouseLeave += Run_MouseLeave;
            run.MouseMove += Run_MouseMove;
            run.MouseLeftButtonDown += Run_MouseLeftButtonDown;
            run.IsEdited = false;
        }
        
        private void EditWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            switch (CurrentState)
            {
                case State.Stop:
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                        CurrentState = State.Segment;
                    break;
                case State.Play:
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                    {
                        // WAIT FOR EDIT HERE
                        CurrentState = State.Segment;
                    }
                    break;
                case State.Segment:
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
                case State.Segment:
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

        private void Run_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Wait;
            RunWord curRun = sender as RunWord;
            switch (CurrentState)
            {
                case State.Stop:
                    if (e.ClickCount == 1)
                    {
                        curRun.Select();
                        curRun.PlayCachedSound();
                    }
                    else if (e.ClickCount == 2)
                    {
                        StopSound();
                        curRun.Select();
                        CurrentState = State.Edit;
                    }
                    break;
                case State.Play:
                    if (e.ClickCount == 1)
                    {
                        CurrentState = State.Stop;
                        StopSound();
                        curRun.Select();
                        curRun.PlayCachedSound();
                    }
                    else if (e.ClickCount == 2)
                    {
                        StopSound();
                        curRun.Select();
                        CurrentState = State.Edit;
                    }
                    break;
                case State.Segment:
                    if (e.ClickCount == 1)
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
                            InitiateRun(newRun);
                            newRun.ApplyAvailableSegmentedBackground(ProjectProperties.SplittedWords);
                            IsSaved = false;
                        }
                    }
                    break;
                case State.Edit:
                    if (e.ClickCount == 1)
                    {
                        CurrentState = State.Stop;
                        curRun.Select();
                        curRun.PlayCachedSound();
                    }
                    else if (e.ClickCount == 2)
                    {
                        curRun.Select();
                        CurrentState = State.Edit;
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
                    run.Hover();
                    break;
                case State.Play:
                    run.Hover();
                    break;
                case State.Segment:
                    break;
                case State.Edit:
                    run.Hover();
                    break;
            }
        }

        private void Run_MouseLeave(object sender, MouseEventArgs e)
        {
            RunWord run = sender as RunWord;
            switch (CurrentState)
            {
                case State.Stop:
                    run.Unhover();
                    break;
                case State.Play:
                    run.Unhover();
                    break;
                case State.Segment:
                    break;
                case State.Edit:
                    run.Unhover();
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
            switch (CurrentState)
            {
                case State.Stop:
                    break;
                case State.Play:
                    break;
                case State.Segment:
                    break;
                case State.Edit:
                    int selectIndex = comboBox.SelectedIndex;
                    if (selectIndex == -1)
                    {
                        selectIndex = comboBox.Items.Add(comboBox.Text);
                        ProjectInfo.Dictionary[CurrentRunWord.Text].Add(comboBox.Text);
                    }
                    CurrentRunWord.SelectDictAt(selectIndex);
                    IsSaved = false;
                    CurrentRunWord.PlayCachedSound();
                    break;
            }
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
                    StopSound();
                    break;
                case State.Segment:
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
            ProjectInfo.Save();
            foreach (Paragraph paragraph in Paragraphs)
                foreach (RunWord run in paragraph.Inlines)
                {
                    run.IsEdited = false;
                    run.ApplyAvailableSegmentedBackground(ProjectProperties.SegmentedWords);
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
            PlayingSound.SoundEnded += PlayingSound_SoundEnded;
            AudioPlaybackEngine.Instance.PlaySound(PlayingSound);
        }

        private void StopSound()
        {
            if (PlayingSound == null)
                return;
            PlayingSound.Stop();
        }
        
        private void PlayingSound_SoundEnded(object sender, EventArgs e)
        {
            switch (CurrentState)
            {
                case State.Stop:
                    break;
                case State.Play:
                    if (SelectNextRunWord())
                        PlaySound();
                    else
                        CurrentState = State.Stop;
                    break;
                case State.Segment:
                    break;
                case State.Edit:
                    break;
            }
        }

        private void Sound_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            switch (CurrentState)
            {
                case State.Stop:
                    break;
                case State.Play:
                    if (!e.IsEnded && !CurrentRunWord.IsIn(e.BytePosition))
                    {
                        while (SelectNextRunWord() && !CurrentRunWord.IsIn(e.BytePosition));
                        if (CurrentRunWord == null)
                            SelectFirstRunWord();
                    }
                    break;
                case State.Segment:
                    break;
                case State.Edit:
                    break;
            }
        }
        
        private bool SelectFirstRunWord()
        {
            Paragraph firstParagraph = Paragraphs.FirstBlock as Paragraph;
            if (firstParagraph == null)
                return false;
            RunWord firstWord = firstParagraph.Inlines.FirstInline as RunWord;
            if (firstWord == null)
                return false;
            firstWord.Select();
            return true;
        }

        private bool SelectNextRunWord()
        {
            if (CurrentRunWord == null)
                return false;
            if (CurrentRunWord.NextRun != null)
            {
                CurrentRunWord.NextRun.Select();
                return true;
            }
            Paragraph nextParagraph = (CurrentRunWord.Parent as Paragraph).NextBlock as Paragraph;
            if (nextParagraph == null)
                return false;
            RunWord nextWord = nextParagraph.Inlines.FirstInline as RunWord;
            if (nextWord == null)
                return false;
            nextWord.Select();
            return true;
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
