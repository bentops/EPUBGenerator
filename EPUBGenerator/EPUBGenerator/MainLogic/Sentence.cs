using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    class Sentence
    {
        private String text;

        public int ID { get; set; }
        public String SID { get { return "S" + ID.ToString("D4"); } }
        public int Type { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public List<Word> Words { get; private set; }

        public String Text
        {
            get { return text; }
            set
            {
                text = value;
                List<List<String>> list = Tools.G2P.GenTranscripts(text, Type);
                Words = new List<Word>();
                for (int i = 0; i < list.Count; i++)
                    Words.Add(new Word(i, Type, list[i][0], list[i][1], list[i][2]));
            }
        }

        public String Pronunciation
        {
            get
            {
                String pronun = "";
                foreach (Word word in Words)
                    pronun += word.Pronunciation;
                return pronun;
            }
        }

        public String Transcript
        {
            get
            {
                String transcript = "";
                foreach (Word word in Words)
                    transcript += word.Transcript + ";";
                return transcript;
            }
        }

        public Sentence(int id, int type, String text)
        {
            ID = id;
            Type = type;
            Text = text;
        }
    }
}
