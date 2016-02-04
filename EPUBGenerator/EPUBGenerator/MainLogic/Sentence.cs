using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Sentence
    {
        public int ID { get; set; }
        public String SID { get { return "S" + ID; } }
        public int Type { get; set; }
        public String Text { get; private set; }
        public double Begin { get; private set; }
        public double End { get; private set; }
        public Block Block { get; private set; }
        public LinkedList<Word> Words { get; private set; }


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

        public Sentence(int id, int type, String text, Block block)
        {
            ID = id;
            Type = type;
            Block = block;
            SetTextAndWords(text);
        }

        public Sentence(XElement xSentence, Block block)
        {
            foreach (XAttribute attribute in xSentence.Attributes())
            {
                String value = attribute.Value;
                switch(attribute.Name.ToString())
                {
                    case "id": ID = Int32.Parse(value.Substring(1)); break;
                    case "type": Type = Int32.Parse(value); break;
                    case "begin": Begin = Double.Parse(value); break;
                    case "end": End = Double.Parse(value); break;
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

        public XElement ToXml()
        {
            XElement xSentence = new XElement("Sentence");
            xSentence.Add(new XAttribute("id", SID));
            xSentence.Add(new XAttribute("type", Type));
            xSentence.Add(new XAttribute("begin", Begin));
            xSentence.Add(new XAttribute("end", End));
            xSentence.Add(new XElement("Text", Text));
            foreach (Word word in Words)
                xSentence.Add(word.ToXml());
            return xSentence;
        }
        
        public void SetTextAndWords(String text)
        {
            Text = text;
            Words = new LinkedList<Word>();
            List<KeyValuePair<String, String>> list = Tools.G2P.GenTranscriptList(text, Type);
            foreach (KeyValuePair<String, String> kvPair in list)
            {
                Word word = new Word(kvPair.Key, kvPair.Value, this);
                word.Node = Words.AddLast(word);
            }
        }
    }
}
