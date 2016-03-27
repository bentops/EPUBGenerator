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
        public ProjectInfo ProjectInfo { get { return ImageBlock.Content.ProjectInfo; } }
        public ImageBlock ImageBlock { get; private set; }
        //public LinkedList<RunWord> RunWords { get; private set; }
        public String ImageSource { get { return ImageBlock.ImageResource; } }
        public IEnumerable<RunWord> RunWords
        {
            get
            {
                foreach (Sentence sentence in ImageBlock.Sentences)
                    foreach (Word word in sentence.Words)
                        yield return word.Run;
            }
        }
        public override bool IsImage { get { return true; } }
        
        public RunImage(ImageBlock imageBlock)
        {
            ImageBlock = imageBlock;
            ImageBlock.Run = this;
            foreach (Sentence sentence in ImageBlock.Sentences)
            {
                sentence.GetCachedSound();
                foreach (Word word in sentence.Words)
                    new RunWord(word);
            }
            Text = "<IMG SOURCE=\"" + ImageBlock.Source + "\">";
        }

        public override void PlayCachedSound()
        {
            // WHAT TO DO ?? =_=
        }

        public override bool IsSelected { get { return ProjectInfo.CurrentARun == this; } }
        public override void Select()
        {
            RunWords.First().Select();
            UpdateBackground();
        }

        public override bool IsEdited { get { return ImageBlock.IsEdited; } }
        public void SetCaption(String text)
        {
            ImageBlock.SetAltText(text);
            foreach (Sentence sentence in ImageBlock.Sentences)
            {
                sentence.GetCachedSound();
                foreach (Word word in sentence.Words)
                    new RunWord(word);
            }
        }

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
                case State.Caption:
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
