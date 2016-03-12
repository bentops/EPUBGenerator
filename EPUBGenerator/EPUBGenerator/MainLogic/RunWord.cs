using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
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

        private LinkedListNode<RunWord> Node { get; set; }

        public InlineCollection Inlines { get { return ElementStart.Paragraph.Inlines; } }
        public RunWord PreviousRun { get { return PreviousInline as RunWord; } }
        public RunWord NextRun { get { return NextInline as RunWord; } }
        public Sentence Sentence { get { return Word.Sentence; } }
        public Word Word { get; set; }
        
        public bool IsEdited { get; set; }
        public bool IsSelected { get; set; }
        public Brush NormalModeBackground { get { return IsSelected? ProjectProperties.SelectedWord : null; } }
        public Brush EditModeBackground { get; set; }


        private MemoryStream MemoryStream;
        private WaveStream WaveStream;
        public IWavePlayer Player { get; set; }

        public RunWord(Word word) : base(word.OriginalText)
        {
            Word = word;
        }

        public RunWord(Word word, TextPointer pos) : base(word.OriginalText, pos)
        {
            Word = word;
            InitiateSentenceWavePlayer();
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
            InitiateSentenceWavePlayer();
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
        }
        
        public void InitiateWavePlayer(FileStream fileStream)
        {
            byte[] buffer = new byte[1024];

            fileStream.Position = 16;
            fileStream.Read(buffer, 0, 4);
            int headSize = BitConverter.ToInt32(buffer, 0) + 28; // 44 or 46

            MemoryStream = new MemoryStream();
            fileStream.Position = 0;
            fileStream.Read(buffer, 0, headSize);
            MemoryStream.Write(buffer, 0, headSize);

            long count = Word.End - Word.Begin;
            fileStream.Position = headSize + Word.Begin;
            while (count > 1024)
            {
                fileStream.Read(buffer, 0, 1024);
                MemoryStream.Write(buffer, 0, 1024);
                count -= 1024;
            }
            fileStream.Read(buffer, 0, (int)count);
            MemoryStream.Write(buffer, 0, (int)count);

            MemoryStream.Position = 0;
            WaveStream = new WaveFileReader(MemoryStream);
            Player = new WaveOut(WaveCallbackInfo.FunctionCallback());
            Player.Init(WaveStream);
        }

        public void InitiateSentenceWavePlayer()
        {
            using (FileStream fileStream = File.OpenRead(Sentence.WavPath))
            {
                InitiateWavePlayer(fileStream);
                for (RunWord prev = PreviousRun; prev != null && prev.Sentence.Equals(Sentence); prev = prev.PreviousRun)
                    prev.InitiateWavePlayer(fileStream);
                for (RunWord next = NextRun; next != null && next.Sentence.Equals(Sentence); next = next.NextRun)
                    next.InitiateWavePlayer(fileStream);
            }
        }

        public void Play()
        {
            WaveStream.Position = 0;
            Player.Play();
        }
    }
}
