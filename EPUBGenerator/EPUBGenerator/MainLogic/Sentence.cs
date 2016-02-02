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
        public String SID { get { return "S" + ID.ToString("D5"); } }
        public int Type { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public LinkedList<Word> Words { get; private set; }

        public String Text
        {
            get { return text; }
            set
            {
                text = value;
                if (Project.Status == (int)Statuses.Create)
                {
                    Words = new LinkedList<Word>();
                    List<KeyValuePair<String, String>> list = Tools.G2P.GenTranscriptList(text, Type);
                    foreach (KeyValuePair<String, String> kvPair in list)
                    {
                        Word word = new Word(Type, kvPair.Key, kvPair.Value);
                        word.Node = Words.AddLast(word);
                    }
                }
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

        public Sentence(int id, int type, String text, LinkedList<Word> words)
        {
            ID = id;
            Type = type;
            Text = text;
            Words = words;
        }
    }
}
