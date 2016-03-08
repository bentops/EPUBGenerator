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
        public Sentence Sentence { get { return Word.Sentence; } }
        public Word Word { get; set; }

        public bool Selected { get; set; }

        public RunWord(Word word) : base(word.OriginalText)
        {
            Word = word;
        }

        public void ClearBackground()
        {
            // May need to check if this word was editted?
            Background = Selected ? ProjectProperties.SelectedWord : null;
        }
    }
}
