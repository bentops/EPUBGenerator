using EPUBGenerator.MainLogic;
using EPUBGenerator.MainLogic.SoundEngine;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using Block = EPUBGenerator.MainLogic.Block;
using DResult = System.Windows.Forms.DialogResult;
using MBox = System.Windows.MessageBox;
using ParagraphBlock = System.Windows.Documents.Block;
using Path = System.IO.Path;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>

    public enum State { Stop, Play, Segment, Edit, Caption }

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
        private BlockCollection Paragraphs { get { return BookContentRTB.Document.Blocks; } }
        private InlineCollection ImageInlines { get { return (ImageCaptionRTB.Document.Blocks.FirstBlock as Paragraph).Inlines; } }

        private bool IsPlayingOnlyText { get; set; }
        private bool IsEditingCaption { get; set; }

        private bool IsSaved
        {
            get { return ProjectInfo.IsSaved; }
            set
            {
                CurrentContent.Changed = !value;
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
                if (CurrentState == State.Edit)
                {
                    EditPanel.IsEnabled = true;
                    DictComboBox.Items.Clear();
                    String runText = CurrentRunWord.Text;
                    if (ProjectInfo.Dictionary.ContainsKey(runText))
                        foreach (String pronun in ProjectInfo.Dictionary[runText])
                            DictComboBox.Items.Add(pronun);
                    else
                        DictComboBox.Items.Add(runText);
                    DictComboBox.SelectedIndex = CurrentRunWord.Word.DictIndex;
                    TextBox textBox = DictComboBox.Template.FindName("PART_EditableTextBox", DictComboBox) as TextBox;
                    if (textBox != null)
                    {
                        textBox.Focus();
                        textBox.SelectAll();
                    }
                    LockWord.IsChecked = CurrentRunWord.Word.Locked;
                }
                #endregion
            }
        }
        private ARun CurrentARun { get { return ProjectInfo.CurrentARun; } }
        private RunWord CurrentRunWord { get { return ProjectInfo.CurrentRunWord; } }
        private Content CurrentContent
        {
            get { return ProjectInfo.CurrentContent; }
            set { ProjectInfo.CurrentContent = value; }
        }
        private TreeViewItem SelectedTVI { get; set; }

        private CachedSoundSampleProvider PlayingSound;
        private double CurrentSpeed
        {
            get { return ProjectInfo.Speed; }
            set { if (ProjectInfo != null) ProjectInfo.Speed = value; }
        }
        
        public EditWindow(String epubProjPath)
        {
            InitializeComponent();
            Timer timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;
            Initiate(epubProjPath);
            Closing += OnClosing;
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (IsSaved)
                return;
            cancelEventArgs.Cancel = true;
            this.IsEnabled = false;
            System.Windows.Media.Effects.BlurEffect objBlur = new System.Windows.Media.Effects.BlurEffect();
            objBlur.Radius = 5;
            Effect = objBlur;

            ExitPopup.IsOpen = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            UpdateUI();
        }

        private void UpdateStateLabel()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Stop: StatusLabel.Content = "Playback Stop"; break;
                    case State.Play: StatusLabel.Content = "Playing"; break;
                    case State.Segment: StatusLabel.Content = "Word Segmentation"; break;
                    case State.Edit: StatusLabel.Content = "Word Editing"; break;
                    case State.Caption: StatusLabel.Content = "Image Captioning"; break;
                }
            }));
        } // 1
        private void UpdateSaveStatusLabel()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                SaveLabel.Content = IsSaved ? "SAVED" : "UNSAVED";
                SaveLabel.Foreground = IsSaved ? Brushes.LightGreen : Brushes.Red;
            }));
        } // 2
        private void UpdateBookContentRTB()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Stop:
                    case State.Play:
                    case State.Edit:
                        BookContentRTB.IsEnabled = true;
                        BookContentRTB.CaretBrush = Brushes.Transparent;
                        BookContentRTB.IsReadOnlyCaretVisible = false;
                        break;
                    case State.Segment:
                        BookContentRTB.IsEnabled = true;
                        BookContentRTB.CaretBrush = null;
                        BookContentRTB.IsReadOnlyCaretVisible = true;
                        break;
                    case State.Caption:
                        BookContentRTB.IsEnabled = false;
                        break;
                }
            }));
        } // 3
        private void UpdatePlayPauseButton()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Play:
                        PlayPauseB.Content = FindResource("Pause");
                        break;
                    case State.Stop:
                    case State.Segment:
                    case State.Edit:
                    case State.Caption:
                        PlayPauseB.Content = FindResource("Play");
                        break;
                }
            }));
        } // 4
        private void UpdatePlayOptionPanel()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Play: 
                        PlayOptionPanel.IsEnabled = false;
                        break;
                    case State.Stop: 
                    case State.Segment:
                    case State.Edit: 
                    case State.Caption:
                        PlayOptionPanel.IsEnabled = true;
                        break;
                }
            }));
        } // 5
        private void UpdateSpeedSlider()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Play:
                        SpeedSlider.IsEnabled = false;
                        SpeedTB.IsEnabled = false;
                        break;
                    case State.Stop:
                    case State.Segment:
                    case State.Edit:
                    case State.Caption:
                        SpeedSlider.IsEnabled = true;
                        SpeedTB.IsEnabled = true;
                        break;
                }
            }));
        } // 6
        private void UpdateEditPanel()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Stop: 
                    case State.Play: 
                    case State.Segment:
                    case State.Caption:
                        EditPanel.IsEnabled = false;
                        DictComboBox.Text = "";
                        break;
                    case State.Edit:
                        EditCaptionButton.IsEnabled = true;
                        break;
                }
            }));
        } // 7
        private void UpdateImageCtrlGrid()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                ImageCtrlGrid.Visibility = (CurrentARun != null && CurrentARun.IsImage) ? Visibility.Visible : Visibility.Hidden;
                if (ImageCtrlGrid.Visibility != Visibility.Visible)
                    return;
                RunImage currentImage = CurrentARun as RunImage;
                switch (CurrentState)
                {
                    case State.Play:
                    case State.Stop:
                    case State.Segment:
                    case State.Edit:
                        if (ImageCtrlGrid.Tag != currentImage)
                        {
                            ImageCtrlGrid.Tag = currentImage;
                            Image.Source = new BitmapImage(new Uri(currentImage.ImageSource, UriKind.Absolute));
                            ImageCaptionRTB.Document.Blocks.Clear();
                            ImageCaptionRTB.Document.Blocks.Add(new Paragraph());
                            ImageInlines.Clear();
                            foreach (RunWord run in currentImage.RunWords)
                                ImageInlines.Add(run);
                        }
                        EditCaptionPanel.Visibility = Visibility.Visible;
                        CancelApplyCaptionPanel.Visibility = Visibility.Hidden;
                        break;
                    case State.Caption:
                        EditCaptionPanel.Visibility = Visibility.Hidden;
                        CancelApplyCaptionPanel.Visibility = Visibility.Visible;
                        break;
                }
                #region ImageCaptionRTB
                switch (CurrentState)
                {
                    case State.Stop:
                    case State.Play:
                    case State.Edit:
                        ImageCaptionRTB.SelectionBrush = Brushes.Transparent;
                        ImageCaptionRTB.IsReadOnly = true;
                        ImageCaptionRTB.CaretBrush = Brushes.Transparent;
                        ImageCaptionRTB.IsReadOnlyCaretVisible = false;
                        break;
                    case State.Segment:
                        ImageCaptionRTB.SelectionBrush = Brushes.Transparent;
                        ImageCaptionRTB.IsReadOnly = true;
                        ImageCaptionRTB.CaretBrush = null;
                        ImageCaptionRTB.IsReadOnlyCaretVisible = true;
                        break;
                    case State.Caption:
                        break;
                }
                #endregion
            }));
        } // 8
        private void UpdateSaveButton()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Play:
                    case State.Stop:
                    case State.Segment:
                    case State.Edit:
                        SaveBook.IsEnabled = !IsSaved;
                        break;
                    case State.Caption:
                        SaveBook.IsEnabled = false;
                        break;
                }
            }));
        } // 9
        private void UpdateExportButton()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Play:
                    case State.Stop:
                    case State.Segment:
                    case State.Edit:
                        Export.IsEnabled = IsSaved;
                        break;
                    case State.Caption:
                        Export.IsEnabled = false;
                        break;
                }
            }));
        } // 10

        private void UpdateTemplate()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                switch (CurrentState)
                {
                    case State.Play:
                    case State.Stop:
                    case State.Segment:
                    case State.Edit:
                    case State.Caption:
                        break;
                }
            }));
        }

        private void UpdateUI()
        {
            UpdateStateLabel(); // 1
            UpdateSaveStatusLabel(); // 2
            UpdateBookContentRTB(); // 3
            UpdatePlayPauseButton(); // 4
            UpdatePlayOptionPanel(); // 5
            UpdateSpeedSlider(); //6
            UpdateEditPanel(); // 7
            UpdateImageCtrlGrid(); // 8
            UpdateSaveButton(); // 9
            UpdateExportButton(); // 10
        }

        public void Initiate(String epubProjPath)
        {
            this.PreviewKeyDown += EditWindow_KeyDown;
            BookContentRTB.IsReadOnly = true;
            BookContentRTB.SelectionBrush = ProjectProperties.Transparent;
           
            ProjectInfo = new ProjectInfo(epubProjPath);
            CurrentState = State.Stop;

            bookName.Content = ProjectName;
            projPath.Text = ProjectPath;
            GenerateProjectMenu();
            
            if (CurrentContent != null)
            {
                foreach (TreeViewItem tvi in _AllContentsTVI.Items)
                    if (CurrentContent == tvi.Tag)
                    {
                        CurrentContent = null;
                        tvi.IsSelected = true;
                    }
            }
            else if (ProjectInfo.Contents.Count > 0)
                (_AllContentsTVI.Items.GetItemAt(0) as TreeViewItem).IsSelected = true;
            
            Show();
        }

        private void GenerateProjectMenu()
        {
            _AllContentsTVI = new TreeViewItem() { Header = "Contents", IsExpanded = true };
            _AllImagesTVI = new TreeViewItem() { Header = "Images" };
            foreach (Content content in ProjectInfo.Contents)
            {
                String name = Path.GetFileNameWithoutExtension(content.Source);
                _AllContentsTVI.Items.Add(new TreeViewItem() { Header = name, Tag = content });

                TreeViewItem contentTVI = new TreeViewItem() { Header = name };
                _AllImagesTVI.Items.Add(contentTVI);
                foreach (ImageBlock imageBlock in content.ImageBlocks)
                {
                    String imgName = Path.GetFileNameWithoutExtension(imageBlock.Source);
                    contentTVI.Items.Add(new TreeViewItem() { Header = imgName, Tag = imageBlock});
                }
                Console.WriteLine("Images: " + content.ImageBlocks.Count);
            }

            treeView.Items.Add(_AllContentsTVI);
            treeView.Items.Add(_AllImagesTVI);
            treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem oldTVI = e.OldValue as TreeViewItem;
            TreeViewItem newTVI = e.NewValue as TreeViewItem;
            //Console.WriteLine("OLD: " + oldTVI + ", NEW: " + newTVI);
            if (newTVI == null || newTVI.Tag == null)
                return;
            
            Cursor = Cursors.Wait;
            Content oldContent = CurrentContent;
            Content newContent = (newTVI.Tag is ImageBlock ? (newTVI.Tag as ImageBlock).Content : newTVI.Tag as Content);

            if (oldContent != newContent)
            {
                Paragraphs.Clear();
                if (CurrentContent != null)
                    foreach (Block block in CurrentContent.Blocks)
                        foreach (Sentence sentence in block.Sentences)
                            sentence.ClearCachedSound();
                if (SelectedTVI != null)
                    SelectedTVI.Background = ProjectProperties.Transparent;
                foreach (TreeViewItem tvi in _AllContentsTVI.Items)
                    if (tvi.Tag == oldContent)
                        tvi.Background = ProjectProperties.Transparent;

                CurrentState = State.Stop;
                SelectedTVI = newTVI;
                CurrentContent = newContent;
                //Console.WriteLine("Sentences Count = " + CurrentContent.TotalSentences);

                foreach (Block block in CurrentContent.Blocks)
                {
                    Paragraph paragraph = new Paragraph();
                    Paragraphs.Add(paragraph);

                    if (block is ImageBlock)
                    {
                        RunImage image = new RunImage(block as ImageBlock);
                        InitiateRun(image);
                        paragraph.Inlines.Add(image);
                        continue;
                    }

                    foreach (Sentence sentence in block.Sentences)
                    {
                        //Console.WriteLine("S: " + sentence.ID);
                        sentence.GetCachedSound();
                        foreach (Word word in sentence.Words)
                        {
                            RunWord run = new RunWord(word);
                            paragraph.Inlines.Add(run);
                            InitiateRun(run);
                        }
                    }
                }
                SelectedTVI.Background = ProjectProperties.SelectedContent;
                foreach (TreeViewItem tvi in _AllContentsTVI.Items)
                    if (tvi.Tag == newContent)
                        tvi.Background = ProjectProperties.SelectedContent;

                PageTeller.Text = (1 + ProjectInfo.Contents.IndexOf(CurrentContent)) + " / " + ProjectInfo.Contents.Count;
            }
            
            if (newTVI.Tag is ImageBlock)
            {
                RunImage runImg = (newTVI.Tag as ImageBlock).Run;
                runImg.Select();
                runImg.BringIntoView();
            }
            else if (CurrentARun == null)
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
                case State.Caption:
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
                    RichTextBox rtb = run.IsImage ? ImageCaptionRTB : BookContentRTB;
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
                case State.Caption:
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
                        if (run is RunWord)
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
                        if (run is RunWord)
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
                        RichTextBox rtb = curRun.IsImage ? ImageCaptionRTB : BookContentRTB;
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
                        if (run is RunWord)
                            CurrentState = State.Edit;
                    }
                    break;
                case State.Caption:
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
                case State.Caption:
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
                case State.Caption:
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
                case State.Caption:
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
                case State.Caption:
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
                    int selectIndex = DictComboBox.Items.IndexOf(DictComboBox.Text);
                    if (selectIndex == -1)
                    {
                        selectIndex = DictComboBox.Items.Add(DictComboBox.Text);
                        if (!ProjectInfo.Dictionary.ContainsKey(CurrentRunWord.Text))
                            ProjectInfo.Dictionary.Add(CurrentRunWord.Text, new List<String>() { CurrentRunWord.Text });
                        ProjectInfo.Dictionary[CurrentRunWord.Text].Add(DictComboBox.Text);
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
                            if (content.ID < CurrentContent.ID)
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
                case State.Caption:
                    break;
            }
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
            int idx = ProjectInfo.Contents.IndexOf(CurrentContent);
            if (idx + 1 < ProjectInfo.Contents.Count)
                (_AllContentsTVI.Items.GetItemAt(idx + 1) as TreeViewItem).IsSelected = true;
            (sender as FrameworkElement).Cursor = Cursors.Hand;
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            (sender as FrameworkElement).Cursor = Cursors.Wait;
            int idx = ProjectInfo.Contents.IndexOf(CurrentContent);
            if (idx - 1 >= 0)
                (_AllContentsTVI.Items.GetItemAt(idx - 1) as TreeViewItem).IsSelected = true;
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
                case State.Caption:
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

                    this.IsEnabled = false;
                    BlurEffect objBlur = new BlurEffect();
                    objBlur.Radius = 5;
                    Effect = objBlur;
                    ExportOkButton.Visibility = Visibility.Hidden;
                    ExportCancelButton.Visibility = Visibility.Visible;
                    ExportProgress.Visibility = Visibility.Visible;
                    ExportWait.Content = "Please wait while exporting ...";
                    /////
                    ExportPopup.IsOpen = true;

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

        private ProgressUpdater _ProgressUpdater;
        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
            if (e.Error != null)
            {
                if (e.Error is OperationCanceledException)
                {
                    this.IsEnabled = true;
                    ExportPopup.IsOpen = false;
                    Effect = null;
                }
                else
                {
                    Console.WriteLine("Exporting, RunworkerCompleted with Exception: ");
                    Console.WriteLine("\t" + e.Error.Message);
                    Console.WriteLine(e.Error.StackTrace);
                    //ขึ้นerror ให้กด ok
                    ExportPopupGrid.Background = new BrushConverter().ConvertFrom("#ffffa6") as SolidColorBrush;
                    ExportOkButton.Visibility = Visibility.Visible;
                    ExportCancelButton.Visibility = Visibility.Hidden;
                    ExportProgress.Visibility = Visibility.Hidden;
                    ExportWait.Content = "Exporting ERROR !";
                }
            }
            else
            {
                ExportOkButton.Visibility = Visibility.Visible;
                ExportCancelButton.Visibility = Visibility.Hidden;
                ExportProgress.Visibility = Visibility.Hidden;
                ExportWait.Content = "DONE !";
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ExportProgress.Value = e.ProgressPercentage;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            _ProgressUpdater = new ProgressUpdater(sender as BackgroundWorker, e);
            Project.Export(ProjectInfo.EpubProjectPath, _ExportPath, _ProgressUpdater);
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            _ProgressUpdater.Cancel();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = true;
            ExportPopup.IsOpen = false;
            Effect = null;
        }



        private void PlaySound()
        {
            if (CurrentRunWord == null)
                return;
            if (IsPlayingOnlyText)
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
            if (IsPlayingOnlyText)
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
            if (IsPlayingOnlyText)
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
            if (IsPlayingOnlyText)
                while (nextRun != null && nextRun.IsImage)
                    nextRun = nextRun.LogicalNext();
            if (nextRun == null)
                return false;
            nextRun.Select();
            return true;
        }

        private void PlayAll_Checked(object sender, RoutedEventArgs e)
        {
            IsPlayingOnlyText = false;
        }

        private void PlayText_Checked(object sender, RoutedEventArgs e)
        {
            IsPlayingOnlyText = true;
        }

        private void OpenImageWindow_Click(object sender, RoutedEventArgs e)
        {
            RunImage iRun = CurrentARun as RunImage;
            if (iRun == null)
                return;
            new ImageWindow(iRun.ImageSource).Show();
        }

        private void EditCaptionButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                RunImage currentImage = ImageCtrlGrid.Tag as RunImage;
                ImageInlines.Clear();
                Run caption = new Run();
                foreach (RunWord run in currentImage.RunWords)
                    caption.Text += run.Text;
                ImageInlines.Add(caption);
                ImageCaptionRTB.ClearValue(RichTextBox.SelectionBrushProperty);
                ImageCaptionRTB.IsReadOnly = false;
                ImageCaptionRTB.CaretBrush = null;
                ImageCaptionRTB.SelectAll();
                ImageCaptionRTB.Focus();
            }));
            CurrentState = State.Caption;
        }

        private void ApplyCaptionButton_Click(object sender, RoutedEventArgs e)
        {
            RunImage iRun = ImageCtrlGrid.Tag as RunImage;
            String caption = "";
            foreach (Paragraph par in ImageCaptionRTB.Document.Blocks)
            {
                foreach (Run run in par.Inlines)
                    caption += run.Text;
                caption += "\n";
            }
            iRun.SetCaption(caption);
            foreach (RunWord runWord in iRun.RunWords)
                InitiateRun(runWord);

            ImageCtrlGrid.Tag = null;
            iRun.Select();
            CurrentState = State.Stop;
        }

        private void CancelCaptionButton_Click(object sender, RoutedEventArgs e)
        {
            ImageCtrlGrid.Tag = null;
            CurrentState = State.Stop;
        }

        private void saveExitPopupButton_Click(object sender, RoutedEventArgs e)
        {
            SaveBook_Click(sender, e);
            System.Windows.Application.Current.Shutdown();
        }

        private void dontSaveExitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void cancelExitPopupButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = true;
            ExitPopup.IsOpen = false;
            Effect = null;
        }

        private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CurrentSpeed = e.NewValue;
        }
    }
}
