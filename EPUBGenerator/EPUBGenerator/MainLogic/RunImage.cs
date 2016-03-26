using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace EPUBGenerator.MainLogic
{
    class RunImage : ARun
    {
        private RichTextBox RichTextBox { get; set; }

        public ProjectInfo ProjectInfo { get { return ImageBlock.Content.ProjectInfo; } }
        public ImageBlock ImageBlock { get; private set; }
        public List<RunWord> RunWords { get; private set; }

        public override bool IsImage { get { return true; } }
        
        public RunImage(ImageBlock imageBlock, RichTextBox richTextBox)
        {
            ImageBlock = imageBlock;
            ImageBlock.Run = this;
            RichTextBox = richTextBox;
            RunWords = new List<RunWord>();
            foreach (Sentence sentence in ImageBlock.Sentences)
            {
                sentence.GetCachedSound();
                foreach (Word word in sentence.Words)
                {
                    RunWord run = new RunWord(word);
                    RunWords.Add(run);
                }
            }
            Text = "<IMG SOURCE=\"" + ImageBlock.Source + "\">";
        }

        public override void PlayCachedSound()
        {
        }

        public override bool IsSelected { get { return ProjectInfo.CurrentARun == this; } }
        public override void Select()
        {
            Dispatcher.Invoke((Action)(() =>
            {
                RichTextBox.Document.Blocks.Clear();
                Paragraph paragraph = new Paragraph();
                RichTextBox.Document.Blocks.Add(paragraph);
                foreach (RunWord run in RunWords)
                {
                    paragraph.Inlines.Add(run);
                }
            }));
            RunWords[0].Select();
            UpdateBackground();
        }
        public override bool IsEdited { get; set; }
        public override void UpdateSegmentedBackground()
        {
            foreach (RunWord run in RunWords)
                run.UpdateSegmentedBackground();
        }
        public override void UpdateBackground()
        {
            foreach (RunWord run in RunWords)
                run.UpdateBackground();
            Brush brush = null;
            switch (ProjectInfo.CurrentState)
            {
                case State.Stop:
                    if (IsSelected)
                        brush = ProjectProperties.SelectedWord;
                    else if (IsHovered)
                        brush = ProjectProperties.HoveredWord;
                    else if (IsEdited)
                        brush = ProjectProperties.EditedWord;
                    else
                        brush = null;
                    break;
                case State.Play:
                    if (IsSelected)
                        brush = ProjectProperties.PlayingWord;
                    else if (IsHovered)
                        brush = ProjectProperties.HoveredWord;
                    else if (IsEdited)
                        brush = ProjectProperties.EditedWord;
                    else
                        brush = null;
                    break;
                case State.Segment:
                    if (IsSelected)
                        brush = ProjectProperties.PlayingWord;
                    else
                        brush = null;
                    break;
                case State.Edit:
                    if (IsSelected)
                        brush = ProjectProperties.EditingSelectedWord;
                    else if (IsHovered)
                        brush = ProjectProperties.HoveredWord;
                    else if (IsEdited)
                        brush = ProjectProperties.EditedWord;
                    else
                        brush = null;
                    break;
            }
            Dispatcher.Invoke((Action)(() => { Background = brush; }));
        }

        public override ARun LogicalPrevious()
        {
            if (IsSelected)
            {
                ARun prev = ProjectInfo.CurrentRunWord.LogicalPrevious();
                if (prev != null)
                    return prev;
            }
            if (PreviousParagraph != null)
                return PreviousParagraph.Inlines.LastInline as ARun;
            return null;
        }
        public override ARun LogicalNext()
        {
            if (IsSelected)
            {
                ARun next = ProjectInfo.CurrentRunWord.LogicalNext();
                if (next != null)
                    return next;
            }
            if (NextParagraph != null)
                return NextParagraph.Inlines.FirstInline as ARun;
            return null;
        }
    }
}
