using System;
using System.Collections.Generic;
using System.IO;
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
        public int Begin { get; private set; }
        public int End { get; private set; }
        public Sentence Sentence { get; private set; }
        public MemoryStream WaveStream { get; private set; } 
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


        #region ----------- NEW PROJECT ------------

        public Word(String text, String phoneme, Sentence sentence)
        {
            Text = text;
            Phoneme = phoneme;
            Sentence = sentence;
        }

        public XElement ToXml()
        {
            XElement xWord = new XElement("Word");
            //xWord.Add(new XAttribute("id", WID));
            xWord.Add(new XAttribute("txt", Text));
            if (pronunciation != null)
                xWord.Add(new XAttribute("pronun", Pronunciation));
            xWord.Add(new XAttribute("phon", Phoneme));
            xWord.Add(new XAttribute("bytes", End - Begin));
            return xWord;
        }
        
        public void SetPosition(int begin, int end)
        {
            Begin = begin;
            End = end;
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
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
                    case "begin": Begin = int.Parse(value); break;
                    case "end": End = int.Parse(value); break;
                    default: break;
                }

            }
            Sentence = sentence;
        }
        #endregion
        
        #region ----------- EDIT PROJECT ------------
        public void MergeWithNextWord()
        {
            LinkedList<Word> Words = Node.List;
            LinkedListNode<Word> next = Node.Next;
            if (next == null) return;
            String newText = Text + next.Value.Text;
            String newPhoneme = Tools.GetPhoneme(newText, Sentence.Type);
            Word newWord = new Word(newText, newPhoneme, Sentence);
            newWord.Node = Words.AddBefore(Node, newWord);
            Words.Remove(next);
            Words.Remove(Node);
        }

        public void SplitAt(int index)
        {
            LinkedList<Word> Words = Node.List;
            String newText = Text.Substring(index);
            String newPhoneme = Tools.GetPhoneme(newText, Sentence.Type);
            Word newWord = new Word(newText, newPhoneme, Sentence);
            newWord.Node = Words.AddAfter(Node, newWord);

            Text = Text.Substring(0, index);
            Pronunciation = null;
        }
        #endregion
    }
}
