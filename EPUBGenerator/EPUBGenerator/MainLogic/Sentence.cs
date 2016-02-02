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
                if (Project.Status == (int)Statuses.Create)
                {
                    Words = new List<Word>();
                    List<KeyValuePair<String, String>> list = Tools.G2P.GenTranscriptList(text, Type);
                    foreach (KeyValuePair<String, String> kvPair in list)
                        Words.Add(new Word(Type, kvPair.Key, kvPair.Value));
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

        public void MergeWithNextWord(int index)
        {
            if (Words == null || index <= 0 || index >= Words.Count)
                return;
            String newText = Words[index - 1].Text + Words[index].Text;
            String newTranscript = Tools.G2P.GenTranscript(newText, Type);
            Word newWord = new Word(Type, newText, newTranscript);
            
        }
    }
}
