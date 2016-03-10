using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;

namespace EPUBGenerator.MainLogic
{
    class RunWord : Run
    {
        private LogicalDirection GoForward = LogicalDirection.Forward;
        private LogicalDirection GoBackward = LogicalDirection.Backward;

        public InlineCollection Inlines { get { return ElementStart.Paragraph.Inlines; } }
        public Sentence Sentence { get { return Word.Sentence; } }
        public Word Word { get; set; }

        public bool Selected { get; set; }

        public RunWord(Word word) : base(word.OriginalText)
        {
            Word = word;
        }

        public RunWord(Word word, TextPointer pos) : base(word.OriginalText, pos)
        {
            Word = word;
        }

        public void ClearBackground()
        {
            // May need to check if this word was editted?
            Background = Selected ? ProjectProperties.SelectedWord : null;
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
            ApplyAvailableBrush(ProjectProperties.MergedWords);
        }
        
        public RunWord SplitAt(TextPointer pointer)
        {
            int splitPos = pointer.GetTextRunLength(GoBackward);
            Word newWord = Word.SplitAt(splitPos);
            Text = Word.OriginalText;
            ApplyAvailableBrush(ProjectProperties.SplittedWords);

            TextPointer insertPos = pointer.GetNextContextPosition(GoForward);
            while (insertPos != null && insertPos.GetPointerContext(GoBackward) != TextPointerContext.ElementEnd)
                insertPos = insertPos.GetNextContextPosition(GoForward);
            return new RunWord(newWord, insertPos);
        }

        public void ApplyAvailableBrush(Brush[] brushes)
        {
            // However, it should have at least 2 brushes.
            if (brushes.Length < 2)
                throw new Exception("BrushList contains only " + brushes.Length + " brush.");

            // If prev is null, just check with the next one.
            if (PreviousInline == null)
            {
                Background = (NextInline == null || NextInline.Background != brushes[0]) ? brushes[0] : brushes[1];
                return;
            }

            // If next is null, just check with the prev one. [But now we know that prev is not null.]
            if (NextInline == null)
            {
                Background = (PreviousInline.Background != brushes[0]) ? brushes[0] : brushes[1];
                return;
            }

            // Check every brush in brushes, find one that doesn't share color with prev or next.
            foreach (Brush brush in brushes)
            {
                if (PreviousInline.Background == brush || NextInline.Background == brush)
                    continue;
                Background = brush;
                return;
            }

            // If there is no consistent brush, choose one that is different from prev, repeat the algo with the next one.
            Background = (PreviousInline.Background != brushes[0]) ? brushes[0] : brushes[1];
            (NextInline as RunWord).ApplyAvailableBrush(brushes);
        }
        
    }
}
