using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    class Word
    {
        public int ID { get; set; }
        public String WID { get { return "W" + ID.ToString("D4"); } }

        public String Text { get; private set; }
        public String Pronunciation { get; private set; }
        public String Transcript { get; private set; }
        public int Type { get; private set; }
        public double StartTime { get; private set; }
        public double EndTime { get; private set; }
        
        public Word(int id, int type, String text, String pronunciation, String transcript)
        {
            ID = id;
            Type = type;
            Text = text;
            Pronunciation = pronunciation;
            Transcript = transcript;
        }

        public void SetNewPronunciation(String newPronun)
        {
            Pronunciation = newPronun;
            Transcript = Tools.G2P.GenTranscript(newPronun, Type);
        }

        public void SetTime(double start, double end)
        {
            StartTime = start;
            EndTime = end;
        }

    }
}
