using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    class Word
    {
        public String Text { get; set; }
        public String[] Pronun { get; set; }
        public double StartTime { get; private set; }
        public double EndTime { get; private set; }

        public Word(String txt, String[] pronun)
        {
            Text = txt;
            Pronun = pronun;
        }

        public void SetTime(double start, double end)
        {
            StartTime = start;
            EndTime = end;
        }

    }
}
