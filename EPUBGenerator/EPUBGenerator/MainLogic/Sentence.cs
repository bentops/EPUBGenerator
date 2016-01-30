using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    class Sentence
    {
        public String Text { get; set; }
        public int Type { get; set; }
        public List<Word> Words { get; set; }

        public Sentence(String text, int type)
        {
            Text = text;
            Type = type;
        }
    }
}
