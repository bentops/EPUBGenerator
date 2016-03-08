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

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>

    public enum States { Stop, Play, Edit }

    public partial class EditWindow : Window
    {
        private TreeViewItem contentsTVI;
        private TreeViewItem imagesTVI;

        private States CurrentState { get; set; }

        private ProjectInfo ProjectInfo { get; set; }
        private String ProjectName { get { return ProjectInfo.ProjectName; } }
        private String ProjectPath { get { return ProjectInfo.ProjectPath; } }
        private Content SelectedContent { get; set; }
        private RunWord CurrentRunWord { get; set; }
        private BlockCollection Paragraphs { get { return richTextBox.Document.Blocks; } }


        public EditWindow(String epubProjPath)
        {
            InitializeComponent();
            this.KeyDown += EditWindow_KeyDown;
            CurrentState = States.Stop;
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
            Key key = e.SystemKey;
            switch (CurrentState)
            {
                case States.Stop:
                    Console.WriteLine("STATE STOP");
                    if (key == Key.LeftAlt || key == Key.RightAlt)
                        ToEditMode();
                    break;
                case States.Play:
                    if (key == Key.LeftAlt || key == Key.RightAlt)
                    {

                    }
                    break;
                case States.Edit:
                    if (key == Key.LeftAlt || key == Key.RightAlt)
                    {
                        foreach (Paragraph paragraph in Paragraphs)
                            foreach (RunWord run in paragraph.Inlines)
                                run.ClearBackground();
                        CurrentState = States.Stop;
                    }
                    break;
            }
        }

        private void ToEditMode()
        {
            int count = 0;
            foreach (Paragraph paragraph in Paragraphs)
                foreach (RunWord run in paragraph.Inlines)
                {
                    run.Background = ProjectProperties.CutWord[count];
                    count = 1 - count;
                }
            CurrentState = States.Edit;
            Console.WriteLine("TOEDITMODE");
        }

        private void GenerateProjectMenu()
        {
            TreeViewItem projectTVI = new TreeViewItem() { Header = "Project '" + ProjectName + "'", IsExpanded = true };

            contentsTVI = new TreeViewItem() { Header = "Contents", IsExpanded = true };
            foreach (String contentSrc in ProjectInfo.ContentSources)
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

                        paragraph.Inlines.Add(run);
                    }
                }
                Paragraphs.Add(paragraph);
            }
        }

        private void Run_MouseDown(object sender, MouseButtonEventArgs e)
        {
            RunWord run = sender as RunWord;
            switch (CurrentState)
            {
                case States.Stop:
                    break;
                case States.Play:
                    break;
                case States.Edit:
                    break;
            }
        }

        private void Run_MouseEnter(object sender, MouseEventArgs e)
        {
            RunWord run = sender as RunWord;
            switch (CurrentState)
            {
                case States.Stop:
                    run.Background = ProjectProperties.HoveredWord;
                    break;
                case States.Play:
                    break;
                case States.Edit:
                    break;
            }
        }

        private void Run_MouseLeave(object sender, MouseEventArgs e)
        {
            RunWord run = sender as RunWord;
            switch (CurrentState)
            {
                case States.Stop:
                    run.ClearBackground();
                    break;
                case States.Play:
                    break;
                case States.Edit:
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

        }

        private void export_Click(object sender, RoutedEventArgs e)
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
