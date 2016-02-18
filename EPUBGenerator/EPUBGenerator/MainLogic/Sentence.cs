using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Sentence
    {
        public static int Total;
        public int ID { get; set; }
        public String SID { get { return "S" + ID; } }
        public String BSID { get { return Block.Content.CID + Block.BID + SID; } }
        public int Type { get; set; }
        public int Bytes { get; set; }
        public String Text { get; private set; }
        public Block Block { get; private set; }
        public LinkedList<Word> Words { get; private set; }
        public MemoryStream WaveStream { get; private set; }

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

        public String Phoneme
        {
            get
            {
                String phoneme = "";
                foreach (Word word in Words)
                    phoneme += word.Phoneme;
                if (Type == 2)
                    return phoneme;
                return @"sil;7;0|" + phoneme + @"sil;7;0|";
            }
        }

        #region ----------- NEW PROJECT ------------
        public Sentence(int id, int type, String text, Block block)
        {
            Total++;
            ID = id;
            Type = type;
            Block = block;
            SetTextAndWords(text);
        }

        public void SetTextAndWords(String text)
        {
            Text = text;
            Words = Tools.GenWordList(this);
        }

        public XElement ToXml()
        {
            XElement xSentence = new XElement("Sentence");
            xSentence.Add(new XAttribute("id", SID));
            xSentence.Add(new XAttribute("type", Type));
            xSentence.Add(new XAttribute("bytes", Bytes));
            xSentence.Add(new XAttribute("begin", Words.First.Value.Begin));
            xSentence.Add(new XElement("Text", Text));
            foreach (Word word in Words)
                xSentence.Add(word.ToXml());
            return xSentence;
        }


        public void Synthesize()
        {
            Tools.Synthesize(Phoneme, Type, Block.Content.Order + "-" + Block.BID + "-" + SID);
        }

        // -----------------------------------------------------------
        #endregion


        #region ----------- OPEN PROJECT ------------
        // Need to recheck (@ id)
        public Sentence(XElement xSentence, Block block)
        {
            Total++;
            foreach (XAttribute attribute in xSentence.Attributes())
            {
                String value = attribute.Value;
                switch(attribute.Name.ToString())
                {
                    case "id": ID = int.Parse(value.Substring(1)); break;
                    case "type": Type = int.Parse(value); break;
                    //case "begin": Begin = int.Parse(value); break;
                    //case "end": End = int.Parse(value); break;
                    default: break;
                }
            }
            Words = new LinkedList<Word>();
            foreach (XElement child in xSentence.Descendants())
            {
                switch(child.Name.ToString())
                {
                    case "Text": Text = child.Value; break;
                    case "Word":
                        Word word = new Word(child, this);
                        word.Node = Words.AddLast(word);
                        break;
                    default: break;
                }
            }
            Block = block;
        }
        #endregion
    }
}
