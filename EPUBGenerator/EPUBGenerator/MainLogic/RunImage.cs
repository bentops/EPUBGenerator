using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace EPUBGenerator.MainLogic
{
    class RunImage : Run
    {
        public ImageBlock ImageBlock { get; private set; }
        public List<RunWord> RunWords { get; private set; }

        public RunImage(ImageBlock imageBlock)
        {
            ImageBlock = imageBlock;
            RunWords = new List<RunWord>();
            foreach (Sentence sentence in ImageBlock.Sentences)
                foreach (Word word in sentence.Words)
                {
                    RunWord run = new RunWord(word);
                    RunWords.Add(run);
                }
            Text = "<IMG SOURCE=\"ImageBlock.Source\">";
        }
    }
}
