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
using System.Timers;

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

        private bool PlayOnlyText { get; set; }

        private bool IsSaved
        {
            get { return ProjectInfo.IsSaved; }
            set
            {
                CurrentContent.Changed = !value;
                Export.IsEnabled = ProjectInfo.IsSaved;
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
                    foreach (ARun run in paragraph.Inlines)
                        run.UpdateBackground();
                UpdateUI();
                #endregion
            }
        }
        private ARun CurrentARun { get { return ProjectInfo.CurrentARun; } }
        private RunWord CurrentRunWord { get { return ProjectInfo.CurrentRunWord; } }
        private Content CurrentContent { get { return ProjectInfo.CurrentContent; } }
        private TreeViewItem SelectedTVI { get; set; }

        private CachedSoundSampleProvider PlayingSound;

        
        public EditWindow(String epubProjPath)
        {
            InitializeComponent();
            Timer timer = new Timer();
            timer.Interval = 200;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            Initiate(epubProjPath);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Stop:
                        // StatusLabel
                        StatusLabel.Content = "Stop";
                        // richTextBox
                        richTextBox.CaretBrush = Brushes.Transparent;
                        richTextBox.IsReadOnlyCaretVisible = false;
                        // PlayPauseButton
                        PlayPauseB.Content = FindResource("Play");
                        // PlayOptionPanel
                        PlayOptionPanel.IsEnabled = true;
                        // EditPanel
                        EditPanel.IsEnabled = false;
                        comboBox.Items.Clear();
                        // ImageTextBox
                        ImageCaptionRTB.CaretBrush = Brushes.Transparent;
                        ImageCaptionRTB.IsReadOnlyCaretVisible = false;


                        break;
                    case State.Play:
                        // StatusLabel
                        StatusLabel.Content = "Playing";
                        // richTextBox
                        richTextBox.CaretBrush = Brushes.Transparent;
                        richTextBox.IsReadOnlyCaretVisible = false;
                        // PlayPauseButton
                        PlayPauseB.Content = FindResource("Pause");
                        // PlayOptionPanel
                        PlayOptionPanel.IsEnabled = false;
                        // EditPanel
                        EditPanel.IsEnabled = false;
                        comboBox.Items.Clear();
                        // ImageTextBox
                        ImageCaptionRTB.CaretBrush = Brushes.Transparent;
                        ImageCaptionRTB.IsReadOnlyCaretVisible = false;

                        break;
                    case State.Segment:
                        // StatusLabel
                        StatusLabel.Content = "Word Segmentation";
                        // richTextBox
                        richTextBox.CaretBrush = null;
                        richTextBox.IsReadOnlyCaretVisible = true;
                        // PlayPauseButton
                        PlayPauseB.Content = FindResource("Play");
                        // PlayOptionPanel
                        PlayOptionPanel.IsEnabled = true;
                        // EditPanel
                        EditPanel.IsEnabled = false;
                        comboBox.Items.Clear();
                        // ImageTextBox
                        ImageCaptionRTB.CaretBrush = null;
                        ImageCaptionRTB.IsReadOnlyCaretVisible = true;

                        break;
                    case State.Edit:
                        // StatusLabel
                        StatusLabel.Content = "Word Editing";
                        // richTextBox
                        richTextBox.CaretBrush = Brushes.Transparent;
                        richTextBox.IsReadOnlyCaretVisible = false;
                        // PlayPauseButton
                        PlayPauseB.Content = FindResource("Play");
                        // PlayOptionPanel
                        PlayOptionPanel.IsEnabled = true;
                        // EditPanel
                        EditPanel.IsEnabled = true;
                        comboBox.Items.Clear();
                        String runText = CurrentRunWord.Text;
                        if (ProjectInfo.Dictionary.ContainsKey(runText))
                            foreach (String pronun in ProjectInfo.Dictionary[runText])
                                comboBox.Items.Add(pronun);
                        else
                            comboBox.Items.Add(runText);
                        comboBox.SelectedIndex = CurrentRunWord.Word.DictIndex;
                        TextBox textBox = comboBox.Template.FindName(comboBox.Text, comboBox) as TextBox;
                        if (textBox != null)
                        {
                            textBox.SelectAll();
                            comboBox.Focus();
                        }
                        LockWord.IsChecked = CurrentRunWord.Word.Locked;
                        // ImageTextBox
                        ImageCaptionRTB.CaretBrush = Brushes.Transparent;
                        ImageCaptionRTB.IsReadOnlyCaretVisible = false;

                        break;
                }
                
                ImageCtrlGrid.Visibility = (CurrentARun != null && CurrentARun.IsImage) ? Visibility.Visible : Visibility.Hidden;
                if (ImageCtrlGrid.Visibility == Visibility.Visible)
                {
                    RunImage currentImage = CurrentARun as RunImage;
                    if (ImageCtrlGrid.Tag != currentImage)
                    {
                        Image.Source = new BitmapImage(new Uri("", UriKind.Relative));
                        //Image.Source = currentImage.
                        //CurrentARun
                        //ImageCtrlGrid.Tag = currentImage;
                    }

                }
                SaveLabel.Content = IsSaved ? "saved" : "unsaved";
                SaveLabel.Foreground = IsSaved ? Brushes.Green : Brushes.Red;
            }));
        }

        public void Initiate(String epubProjPath)
        {
            this.PreviewKeyDown += EditWindow_KeyDown;
            richTextBox.IsReadOnly = true;
            richTextBox.SelectionBrush = ProjectProperties.Transparent;
           
            ProjectInfo = new ProjectInfo(epubProjPath);
            CurrentState = State.Stop;

            bookName.Content = ProjectName;
            projPath.Text = ProjectPath;
            GenerateProjectMenu();
            
            if (CurrentContent != null)
            {
                foreach (TreeViewItem tvi in _AllContentsTVI.Items)
                    if (CurrentContent.Source.Equals(tvi.Tag as String))
                        tvi.IsSelected = true;
            }
            else if (ProjectInfo.Contents.Count > 0)
                (_AllContentsTVI.Items.GetItemAt(0) as TreeViewItem).IsSelected = true;


            Show();
        }

        private void GenerateProjectMenu()
        {
            TreeViewItem projectTVI = new TreeViewItem() { Header = "Project '" + ProjectName + "'", IsExpanded = true };

            _AllContentsTVI = new TreeViewItem() { Header = "Contents", IsExpanded = true };
            foreach (Content content in ProjectInfo.Contents)
            {
                String src = content.Source;
                String name = Path.GetFileNameWithoutExtension(src);
                _AllContentsTVI.Items.Add(new TreeViewItem() { Header = name, Tag = src });
            }
            projectTVI.Items.Add(_AllContentsTVI);

            _AllImagesTVI = new TreeViewItem() { Header = "Images" };
            /*//
            IMAGEEEEEEEEEEEEEEES
            //*/
            projectTVI.Items.Add(_AllImagesTVI);

            treeView.Items.Add(projectTVI);
            treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem oldTVI = e.OldValue as TreeViewItem;
            TreeViewItem newTVI = e.NewValue as TreeViewItem;
            Console.WriteLine("OLD: " + oldTVI + ", NEW: " + newTVI);
            Console.WriteLine("New Tag: " + newTVI == null ? null : newTVI.Tag);
            if (newTVI == null || newTVI.Tag == null)
                return;
            
            Cursor = Cursors.Wait;
            Paragraphs.Clear();
            if (CurrentContent != null)
                foreach (Block block in CurrentContent.Blocks)
                    foreach (Sentence sentence in block.Sentences)
                        sentence.ClearCachedSound();
            if (SelectedTVI != null)
                SelectedTVI.Background = ProjectProperties.Transparent;

            CurrentState = State.Stop;
            SelectedTVI = newTVI;
            ProjectInfo.SelectContent(SelectedTVI.Tag as String);
            Console.WriteLine("Sentences Count = " + CurrentContent.SentenceCount);

            foreach (Block block in CurrentContent.Blocks)
            {
                Paragraph paragraph = new Paragraph();
                Paragraphs.Add(paragraph);

                if (block is ImageBlock)
                {
                    RunImage image = new RunImage(block as ImageBlock, ImageCaptionRTB);
                    InitiateRun(image);
                    paragraph.Inlines.Add(image);
                    continue;
                }

                foreach (Sentence sentence in block.Sentences)
                {
                    Console.WriteLine("S: " + sentence.ID);
                    sentence.GetCachedSound();
                    foreach (Word word in sentence.Words)
                    {
                        RunWord run = new RunWord(word);
                        paragraph.Inlines.Add(run);
                        InitiateRun(run);
                        //run.UpdateSegmentedBackground();
                    }
                }
            }
            SelectedTVI.Background = ProjectProperties.SelectedContent;
            if (CurrentARun == null)
                SelectFirstRunWord();
            else
                CurrentARun.Select();
            Cursor = Cursors.Arrow;
        }
        
        private void InitiateRun(ARun run)
        {
            if (run is RunImage)
                foreach (RunWord iRun in (run as RunImage).RunWords)
                    InitiateRun(iRun);
            run.MouseEnter += Run_MouseEnter;
            run.MouseLeave += Run_MouseLeave;
            run.MouseMove += Run_MouseMove;
            run.MouseLeftButtonDown += Run_MouseLeftButtonDown;
            run.IsEdited = false;
        }
        
        private void EditWindow_KeyDown(object sender, KeyEventArgs e)
        {
            Cursor = Cursors.Wait;
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
                        CurrentState = State.Stop;
                        StopSound();
                        CurrentState = State.Segment;
                    }
                    break;
                case State.Segment:
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                        CurrentState = State.Stop;
                    break;
                case State.Edit:
                    if (key == Key.LeftCtrl || key == Key.RightCtrl)
                        CurrentState = State.Segment;
                    break;
            }
            Cursor = Cursors.Arrow;
        }

        private void Run_MouseMove(object sender, MouseEventArgs e)
        {
            ARun run = sender as ARun;
            switch (CurrentState)
            {
                case State.Stop:
                    run.Cursor = Cursors.Hand;
                    break;
                case State.Play:
                    run.Cursor = Cursors.Hand;
                    break;
                case State.Segment:
                    if (run is RunImage)
                    {
                        run.Cursor = Cursors.Hand;
                        break;
                    }
                    RichTextBox rtb = run.IsImage ? ImageCaptionRTB : richTextBox;
                    rtb.Focus();
                    TextPointer pointer = rtb.GetPositionFromPoint(e.GetPosition(run), false);
                    rtb.CaretPosition = pointer;
                    TextPointerContext contextPrev = pointer.GetPointerContext(GoBackward);
                    TextPointerContext contextNext = pointer.GetPointerContext(GoForward);
                    if (contextNext == TextPointerContext.ElementEnd || contextPrev == TextPointerContext.ElementStart)
                        run.Cursor = Cursors.No;
                    else if (contextNext == contextPrev && contextNext == TextPointerContext.Text)
                        run.Cursor = Cursors.UpArrow;
                    else
                        run.Cursor = Cursors.Arrow;
                    break;
                case State.Edit:
                    run.Cursor = Cursors.Hand;
                    break;
            }
        }

        private void Run_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ARun run = sender as ARun;
            run.Cursor = Cursors.Wait;
            switch (CurrentState)
            {
                case State.Stop:
                    if (e.ClickCount == 1)
                    {
                        run.Select();
                        run.PlayCachedSound();
                    }
                    else if (e.ClickCount == 2)
                    {
                        StopSound();
                        run.Select();
                        CurrentState = State.Edit;
                    }
                    break;
                case State.Play:
                    if (e.ClickCount == 1)
                    {
                        CurrentState = State.Stop;
                        StopSound();
                        run.Select();
                        run.PlayCachedSound();
                    }
                    else if (e.ClickCount == 2)
                    {
                        StopSound();
                        run.Select();
                        CurrentState = State.Edit;
                    }
                    break;
                case State.Segment:
                    if (e.ClickCount == 1)
                    {
                        if (run is RunImage)
                        {
                            run.Select();
                            break;
                        }

                        RunWord curRun = run as RunWord;
                        RichTextBox rtb = curRun.IsImage ? ImageCaptionRTB : richTextBox;
                        TextPointer curPointer = rtb.GetPositionFromPoint(e.GetPosition(run), false);
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
                            InitiateRun(newRun);
                            newRun.UpdateBackground();
                            //newRun.UpdateSegmentedBackground();
                        }
                    }
                    break;
                case State.Edit:
                    if (e.ClickCount == 1)
                    {
                        CurrentState = State.Stop;
                        run.Select();
                        run.PlayCachedSound();
                    }
                    else if (e.ClickCount == 2)
                    {
                        run.Select();
                        CurrentState = State.Edit;
                    }
                    break;
            }
            run.Cursor = Cursors.Hand;
        }

        private void Run_MouseEnter(object sender, MouseEventArgs e)
        {
            ARun run = sender as ARun;
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
            ARun run = sender as ARun;
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

        private void ForwardB_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
            switch (CurrentState)
            {
                case State.Stop:
                    if (SelectNextRunWord())
                        CurrentRunWord.PlayCachedSound();
                    break;
                case State.Play:
                    CurrentState = State.Stop;
                    StopSound();
                    if (SelectNextRunWord())
                        CurrentRunWord.PlayCachedSound();
                    break;
                case State.Segment:
                    break;
                case State.Edit:
                    CurrentState = State.Stop;
                    if (SelectNextRunWord())
                        CurrentRunWord.PlayCachedSound();
                    break;
            }
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }

        private void BackwardB_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
            switch (CurrentState)
            {
                case State.Stop:
                    if (SelectPreviousRunWord())
                        CurrentRunWord.PlayCachedSound();
                    break;
                case State.Play:
                    CurrentState = State.Stop;
                    StopSound();
                    if (SelectPreviousRunWord())
                        CurrentRunWord.PlayCachedSound();
                    break;
                case State.Segment:
                    break;
                case State.Edit:
                    CurrentState = State.Stop;
                    if (SelectPreviousRunWord())
                        CurrentRunWord.PlayCachedSound();
                    break;
            }
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }
        
        private void LockWord_Checked(object sender, RoutedEventArgs e)
        {
            CurrentRunWord.Word.Locked = true;
        }

        private void LockWord_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentRunWord.Word.Locked = false;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
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
                        if (!ProjectInfo.Dictionary.ContainsKey(CurrentRunWord.Text))
                            ProjectInfo.Dictionary.Add(CurrentRunWord.Text, new List<String>() { CurrentRunWord.Text });
                        ProjectInfo.Dictionary[CurrentRunWord.Text].Add(comboBox.Text);
                    }
                    if (ApplyOnlyThisWord.IsChecked == true)
                        CurrentRunWord.SelectDictAt(selectIndex);
                    else if (ApplyAll.IsChecked == true)
                    {
                        foreach (Content content in ProjectInfo.Contents)
                            foreach (Block block in content.Blocks)
                                foreach (Sentence sentence in block.Sentences)
                                    foreach (Word word in sentence.Words)
                                        if (word.OriginalText.Equals(CurrentRunWord.Text))
                                            if (ExceptLockWord.IsChecked == false || !word.Locked)
                                                word.ChangeDictIndex(selectIndex);
                    }
                    else if (ApplyAllFromHere.IsChecked == true)
                    {
                        bool foundFirstWord = false;
                        foreach (Content content in ProjectInfo.Contents)
                        {
                            if (content.Order < CurrentContent.Order)
                                continue;
                            foreach (Block block in content.Blocks)
                                foreach (Sentence sentence in block.Sentences)
                                    foreach (Word word in sentence.Words)
                                        if (word == ProjectInfo.CurrentWord)
                                        {
                                            CurrentRunWord.SelectDictAt(selectIndex);
                                            foundFirstWord = true;
                                        }
                                        else if (foundFirstWord && word.OriginalText.Equals(CurrentRunWord.Text))
                                            if (ExceptLockWord.IsChecked == false || !word.Locked)
                                                word.ChangeDictIndex(selectIndex);
                        }
                           
                    }
                    
                    CurrentRunWord.PlayCachedSound();
                    break;
            }
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
            switch (CurrentState)
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
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
            switch (CurrentState)
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
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }
        
        private void PlayPauseB_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
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
                case State.Edit:
                    CurrentState = State.Play;
                    PlaySound();
                    break;
            }
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }

        private void stopB_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
            switch (CurrentState)
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
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }
        
        private void SaveBook_Click(object sender, RoutedEventArgs e)
        {
            if (IsSaved) return;

            (sender as FrameworkElement).Cursor = Cursors.Wait;
            ProjectInfo.Save();
            foreach (Paragraph paragraph in Paragraphs)
                foreach (ARun run in paragraph.Inlines)
                    run.UpdateBackground();
                    //run.UpdateSegmentedBackground();
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }

        private void Export_Click(object sender, RoutedEventArgs e)
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
            if (CurrentRunWord == null)
                return;
            if (PlayOnlyText)
                if (CurrentARun.IsImage)
                    if (!SelectNextRunWord())
                        return;
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

            ARun firstWord = firstParagraph.Inlines.FirstInline as ARun;
            if (PlayOnlyText)
                while (firstWord != null && firstWord.IsImage)
                    firstWord = firstWord.LogicalNext();
            if (firstWord == null)
                return false;
            firstWord.Select();
            return true;
        }

        private bool SelectPreviousRunWord()
        {
            if (CurrentRunWord == null)
                return false;
            ARun prevRun = CurrentARun.LogicalPrevious();
            if (PlayOnlyText)
                while (prevRun != null && prevRun.IsImage)
                    prevRun = prevRun.LogicalPrevious();
            if (prevRun == null)
                return false;
            prevRun.Select();
            return true;
        }

        private bool SelectNextRunWord()
        {
            if (CurrentRunWord == null)
                return false;

            ARun nextRun = CurrentARun.LogicalNext();
            if (PlayOnlyText)
                while (nextRun != null && nextRun.IsImage)
                    nextRun = nextRun.LogicalNext();
            if (nextRun == null)
                return false;
            nextRun.Select();
            return true;
        }

        private void PlayAll_Checked(object sender, RoutedEventArgs e)
        {
            PlayOnlyText = false;
        }

        private void PlayText_Checked(object sender, RoutedEventArgs e)
        {
            PlayOnlyText = true;
        }

        private void openImgWindow_Click(object sender, RoutedEventArgs e)
        {
            //ImageWindow imageWindow = new ImageWindow(imgPath);
            //ImageWindow.Show();
        }

        private void EditCaptionButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ApplyCaptionButton_Click(object sender, RoutedEventArgs e)
        {

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
