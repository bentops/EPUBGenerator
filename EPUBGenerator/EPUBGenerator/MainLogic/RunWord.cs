using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using EPUBGenerator.MainLogic.SoundEngine;

namespace EPUBGenerator.MainLogic
{
    class RunWord : ARun
    {
        private LogicalDirection GoForward = LogicalDirection.Forward;
        private LogicalDirection GoBackward = LogicalDirection.Backward;

        private CachedSoundSampleProvider _OnPlaying;
        private CachedSoundSampleProvider CachedSound { get { return new CachedSoundSampleProvider(Sentence.CachedSound, Word.Begin / 2, Word.End / 2); } }

        private IEnumerable<RunWord> PreviousWordsInSentence
        {
            get
            {
                for (RunWord prev = PreviousRun; prev != null && prev.Sentence == Sentence; prev = prev.PreviousRun)
                    yield return prev;
            }
        }
        private IEnumerable<RunWord> NextWordsInSentence
        {
            get
            {
                for (RunWord next = NextRun; next != null && next.Sentence == Sentence; next = next.NextRun)
                    yield return next;
            }
        }
        
        public ProjectInfo ProjectInfo { get { return Word.Content.ProjectInfo; } }
        public InlineCollection Inlines { get { return ElementStart.Paragraph.Inlines; } }
        public RunWord PreviousRun { get { return PreviousInline as RunWord; } }
        public RunWord NextRun { get { return NextInline as RunWord; } }
        public Block Block { get { return Sentence.Block; } }
        public Sentence Sentence { get { return Word.Sentence; } }
        public Word Word { get; private set; }

        public override bool IsImage { get { return Block is ImageBlock; } }
        public RunImage Image { get { return IsImage ? (Block as ImageBlock).Run : null; } }

        public RunWord(Word word) : base(word.OriginalText)
        {
            Word = word;
            Word.Run = this;
        }
        
        public RunWord(Word word, TextPointer pos) : base(word.OriginalText, pos)
        {
            Word = word;
            Word.Run = this;
        }
        
        public void MergeWithNext()
        {
            RunWord nextRun = NextInline as RunWord;
            if (nextRun == null)
                return;
            MergeWith(nextRun);
        }

        public void MergeWithPrev()
        {
            RunWord prevRun = PreviousInline as RunWord;
            if (prevRun == null)
                return;
            prevRun.MergeWith(this);
        }
        
        private void MergeWith(RunWord nextRun)
        {
            Word.MergeWith(nextRun.Word);
            Inlines.Remove(nextRun);
            Text = Word.OriginalText;
            UpdateBackground();
            //UpdateSegmentedBackground();
        }
        
        public RunWord SplitAt(TextPointer pointer)
        {
            int splitPos = pointer.GetTextRunLength(GoBackward);
            Word newWord = Word.SplitAt(splitPos);
            Text = Word.OriginalText;
            UpdateBackground();
            //UpdateSegmentedBackground();

            TextPointer insertPos = pointer.GetNextContextPosition(GoForward);
            while (insertPos != null && insertPos.GetPointerContext(GoBackward) != TextPointerContext.ElementEnd)
                insertPos = insertPos.GetNextContextPosition(GoForward);
            return new RunWord(newWord, insertPos);
        }
     
        public override void PlayCachedSound()
        {
            if (_OnPlaying != null)
                _OnPlaying.Stop();
            AudioPlaybackEngine.Instance.PlaySound(_OnPlaying = CachedSound);
        }

        public CachedSoundSampleProvider GetSentenceCachedSound()
        {
            return new CachedSoundSampleProvider(Sentence.CachedSound, Word.Begin / 2, Sentence.Bytes / 2);
        }

        public bool IsIn(long position)
        {
            return Word.Begin <= position && position <= Word.End;
        }

        public override bool IsSelected { get { return ProjectInfo.CurrentRunWord == this; } }
        public bool IsSentenceSelected { get { return ProjectInfo.CurrentRunWord != null && ProjectInfo.CurrentRunWord.Sentence == Sentence; } }
        public override void Select()
        {
            ARun recentARun = ProjectInfo.CurrentARun;
            RunWord recentWord = ProjectInfo.CurrentRunWord;
            ProjectInfo.CurrentRunWord = this;
            UpdateBackground();

            if (recentARun != null && recentARun.IsImage && recentARun != ProjectInfo.CurrentARun)
                recentARun.UpdateBackground();

            if (recentWord == null)
                return;
            recentWord.UpdateBackground();

            if (ProjectInfo.CurrentState == State.Play && !recentWord.IsSentenceSelected)
                foreach (RunWord prev in recentWord.PreviousWordsInSentence)
                    prev.UpdateBackground();
            Word.Content.Changed = true;
        }

        public override bool IsEdited
        {
            get { return Word.IsEdited; }
            set { Word.IsEdited = value; }
        }
        public void SelectDictAt(int index)
        {
            Word.ChangeDictIndex(index);
        }
        
        public Brush SegmentedBackground { get; private set; }
        public override void UpdateSegmentedBackground()
        {
            switch (Word.Status)
            {
                case WordStatus.Normal: ApplyAvailableSegmentedBackground(ProjectProperties.SegmentedWords); break;
                case WordStatus.Splitted: ApplyAvailableSegmentedBackground(ProjectProperties.SplittedWords); break;
                case WordStatus.Merged: ApplyAvailableSegmentedBackground(ProjectProperties.MergedWords); break;
            }
        }
        private void ApplyAvailableSegmentedBackground(Brush[] brushes)
        {
            // However, it should have at least 2 brushes.
            if (brushes.Length < 2)
                throw new Exception("BrushList contains only " + brushes.Length + " brush.");

            // If prev is null, just check with the next one.
            if (PreviousRun == null)
            {
                SegmentedBackground = (NextRun == null || NextRun.SegmentedBackground != brushes[0]) ? brushes[0] : brushes[1];
                //UpdateBackground();
                return;
            }

            // If next is null, just check with the prev one. [But now we know that prev is not null.]
            if (NextRun == null)
            {
                SegmentedBackground = (PreviousRun.SegmentedBackground != brushes[0]) ? brushes[0] : brushes[1];
                //UpdateBackground();
                return;
            }

            // Check every brush in brushes, find one that doesn't share color with prev or next.
            foreach (Brush brush in brushes)
            {
                if (PreviousRun.SegmentedBackground == brush || NextRun.SegmentedBackground == brush)
                    continue;
                SegmentedBackground = brush;
                //UpdateBackground();
                return;
            }

            // If there is no consistent brush, choose one that is different from prev, repeat the algo with the next one.
            SegmentedBackground = (PreviousRun.SegmentedBackground != brushes[0]) ? brushes[0] : brushes[1];
            //UpdateBackground();
            NextRun.ApplyAvailableSegmentedBackground(brushes);
        }

        public override void UpdateBackground()
        {
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
                    {
                        brush = ProjectProperties.PlayingWord;
                        foreach (RunWord prev in PreviousWordsInSentence)
                            prev.UpdateBackground();
                        foreach (RunWord next in NextWordsInSentence)
                            next.UpdateBackground();
                    }
                    else if (IsSentenceSelected)
                        brush = ProjectProperties.PlayingSentence;
                    else if (IsHovered)
                        brush = ProjectProperties.HoveredWord;
                    else if (IsEdited)
                        brush = ProjectProperties.EditedWord;
                    else
                        brush = null;
                    break;
                case State.Segment:
                    UpdateSegmentedBackground();
                    brush = SegmentedBackground;
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
            Dispatcher.Invoke((Action)(() => { Background = brush; } ));
        }

        public override ARun LogicalPrevious()
        {
            if (PreviousRun != null)
                return PreviousRun;
            if (PreviousParagraph != null)
                return PreviousParagraph.Inlines.LastInline as ARun;
            return null;
        }
        public override ARun LogicalNext()
        {
            if (NextRun != null)
                return NextRun;
            if (NextParagraph != null)
                return NextParagraph.Inlines.FirstInline as ARun;
            return null;
        }
    }
}
