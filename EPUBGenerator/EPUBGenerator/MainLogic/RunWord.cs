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
    class RunWord : Run
    {
        private LogicalDirection GoForward = LogicalDirection.Forward;
        private LogicalDirection GoBackward = LogicalDirection.Backward;

        private CachedSoundSampleProvider _OnPlaying;

        private LinkedListNode<RunWord> Node { get; set; }
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
        public Sentence Sentence { get { return Word.Sentence; } }
        public Word Word { get; set; }
        
        public RunWord(Word word) : base(word.OriginalText)
        {
            Word = word;
        }
        
        public RunWord(Word word, TextPointer pos) : base(word.OriginalText, pos)
        {
            Word = word;
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
            ApplyAvailableEditModeBackground(ProjectProperties.MergedWords);
        }
        
        public RunWord SplitAt(TextPointer pointer)
        {
            int splitPos = pointer.GetTextRunLength(GoBackward);
            Word newWord = Word.SplitAt(splitPos);
            Text = Word.OriginalText;
            ApplyAvailableEditModeBackground(ProjectProperties.SplittedWords);

            TextPointer insertPos = pointer.GetNextContextPosition(GoForward);
            while (insertPos != null && insertPos.GetPointerContext(GoBackward) != TextPointerContext.ElementEnd)
                insertPos = insertPos.GetNextContextPosition(GoForward);
            return new RunWord(newWord, insertPos);
        }
        /*
        public void ApplyAvailableBrush(Brush[] brushes)
        {
            // However, it should have at least 2 brushes.
            if (brushes.Length < 2)
                throw new Exception("BrushList contains only " + brushes.Length + " brush.");

            // If prev is null, just check with the next one.
            if (PreviousRun == null)
            {
                Background = (NextRun == null || NextRun.Background != brushes[0]) ? brushes[0] : brushes[1];
                return;
            }

            // If next is null, just check with the prev one. [But now we know that prev is not null.]
            if (NextRun == null)
            {
                Background = (PreviousRun.Background != brushes[0]) ? brushes[0] : brushes[1];
                return;
            }

            // Check every brush in brushes, find one that doesn't share color with prev or next.
            foreach (Brush brush in brushes)
            {
                if (PreviousRun.Background == brush || NextRun.Background == brush)
                    continue;
                Background = brush;
                return;
            }

            // If there is no consistent brush, choose one that is different from prev, repeat the algo with the next one.
            Background = (PreviousRun.Background != brushes[0]) ? brushes[0] : brushes[1];
            NextRun.ApplyAvailableBrush(brushes);
        }*/
        
        public void PlayCachedSound()
        {
            if (_OnPlaying != null)
                _OnPlaying.Position = _OnPlaying.EndPosition;
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
        
        public bool IsSelected { get { return ProjectInfo.CurrentRunWord == this; } }
        public bool IsSentenceSelected { get { return ProjectInfo.CurrentRunWord != null && ProjectInfo.CurrentRunWord.Sentence == Sentence; } }
        public void Select()
        {
            if (IsSelected)
                return;
            RunWord recentWord = ProjectInfo.CurrentRunWord;
            ProjectInfo.CurrentRunWord = this;
            UpdateBackground();

            if (recentWord == null)
                return;
            recentWord.UpdateBackground();
            if (ProjectInfo.CurrentState == State.Play && !recentWord.IsSentenceSelected)
                foreach (RunWord prev in recentWord.PreviousWordsInSentence)
                    prev.UpdateBackground();
        }

        public bool IsHovered { get; private set; }
        public void Hover()
        {
            IsHovered = true;
            UpdateBackground();
        }
        public void Unhover()
        {
            IsHovered = false;
            UpdateBackground();
        }

        public bool IsEdited { get; set; }
        
        public Brush EditModeBackground { get; private set; }

        public void ApplyAvailableEditModeBackground(Brush[] brushes)
        {
            // However, it should have at least 2 brushes.
            if (brushes.Length < 2)
                throw new Exception("BrushList contains only " + brushes.Length + " brush.");

            // If prev is null, just check with the next one.
            if (PreviousRun == null)
            {
                EditModeBackground = (NextRun == null || NextRun.EditModeBackground != brushes[0]) ? brushes[0] : brushes[1];
                UpdateBackground();
                return;
            }

            // If next is null, just check with the prev one. [But now we know that prev is not null.]
            if (NextRun == null)
            {
                EditModeBackground = (PreviousRun.EditModeBackground != brushes[0]) ? brushes[0] : brushes[1];
                UpdateBackground();
                return;
            }

            // Check every brush in brushes, find one that doesn't share color with prev or next.
            foreach (Brush brush in brushes)
            {
                if (PreviousRun.EditModeBackground == brush || NextRun.EditModeBackground == brush)
                    continue;
                EditModeBackground = brush;
                UpdateBackground();
                return;
            }

            // If there is no consistent brush, choose one that is different from prev, repeat the algo with the next one.
            EditModeBackground = (PreviousRun.EditModeBackground != brushes[0]) ? brushes[0] : brushes[1];
            UpdateBackground();
            NextRun.ApplyAvailableEditModeBackground(brushes);
        }

        public void UpdateBackground()
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
                    else
                        brush = null;
                    break;
                case State.Edit:
                    brush = EditModeBackground;
                    break;
            }

            Dispatcher.Invoke((Action)(() => { Background = brush; } ));
        }
    }
}
