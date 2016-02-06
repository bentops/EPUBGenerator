using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Word
    {
        private String pronunciation;

        public int ID { get; set; }
        public String WID { get { return "W" + ID; } }

        public LinkedListNode<Word> Node { get; set; }
        public String Text { get; private set; }
        public String Phoneme { get; private set; }
        public double Begin { get; private set; }
        public double End { get; private set; }
        public Sentence Sentence { get; private set; }
        public String Pronunciation
        {
            get
            {
                if (pronunciation == null)
                    return Text;
                return pronunciation;
            }
            set
            {
                pronunciation = value;
                Phoneme = Tools.GetPhoneme(Pronunciation, Sentence.Type);
            }
        }
        
        public Word(String text, String phoneme, Sentence sentence)
        {
            Text = text;
            Phoneme = phoneme;
            Sentence = sentence;
        }

        public Word(XElement xWord, Sentence sentence)
        {
            foreach (XAttribute attribute in xWord.Attributes()) 
            {
                String value = attribute.Value;
                switch (attribute.Name.ToString())
                {
                    case "id": break;
                    case "txt": Text = value; break;
                    case "pronun": Pronunciation = value; break;
                    case "begin": Begin = Double.Parse(value); break;
                    case "end": End = Double.Parse(value); break;
                    default: break;
                }

            }
            Sentence = sentence;
        }
        
        public XElement ToXml()
        {
            XElement xWord = new XElement("Word");
            xWord.Add(new XAttribute("id", WID));
            xWord.Add(new XAttribute("txt", Text));
            if (pronunciation != null)
                xWord.Add(new XAttribute("pronun", Pronunciation));
            //xWord.Add(new XAttribute("phon", Phoneme));
            xWord.Add(new XAttribute("begin", Begin));
            xWord.Add(new XAttribute("end", End));
            return xWord;
        }

        public void SetTime(double begin, double end)
        {
            Begin = begin;
            End = end;
        }
        
        public void MergeWithNextWord()
        {
            LinkedList<Word> Words = Node.List;
            LinkedListNode<Word> next = Node.Next;
            if (next == null) return;
            String newText = Text + next.Value.Text;
            String newPhoneme = Tools.GetPhoneme(newText, Sentence.Type);
            Word newWord = new Word(newText, newPhoneme, Sentence);
            Words.AddBefore(Node, newWord);
            Words.Remove(next);
            Words.Remove(Node);
        }

        public void Split()
        {
            // T_T ทำไรดี
        }
    }
}
