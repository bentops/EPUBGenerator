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

        private LinkedListNode<Sentence> node { get; set; }

        public int ID { get; private set; }
        public String SID { get { return "S" + ID.ToString("D6"); } }
        public Content Content { get { return Block.Content; } }
        public Block Block { get; private set; }
        public String OriginalText { get { return Block.Text.Substring(StartIdx, Length); } }
        public int StartIdx { get; private set; }
        public int Length { get { return (Next == null ? Block.Length : Next.StartIdx) - StartIdx; } }
        public Sentence Previous { get { return node.Previous == null ? null : node.Previous.Value; } }
        public Sentence Next { get { return node.Next == null ? null : node.Next.Value; } }
        public LinkedList<Word> Words { get; private set; }

        public long Bytes { get; private set; }

        public List<String> FinalTextList
        {
            get
            {
                List<String> list = new List<String>();
                /*
                foreach (Word word in Words)
                {
                    list.Add(word.Pronunciation);
                }
                */
                return list;
            }
        }

        public Sentence(int start, Block block)
        {
            StartIdx = start;
            Block = block;
            ID = Project.Instance.GetRandomUniqueID(Content);
        }

        #region ----------- NEW PROJECT ------------
        public void Synthesize(String outputPath)
        {
            Bytes = Project.Synthesizer.Synthesize(OriginalText, outputPath);
            List<int> tList = Project.Synthesizer.GetTextIndexList();
            List<long> bList = Project.Synthesizer.GetByteIndexList();
            if (tList == null)
                throw new Exception("Null Synthesized Text Index List");
            if (bList == null)
                throw new Exception("Null Synthesized Byte Index List");

            Words = new LinkedList<Word>();
            for (int i = 0; i < tList.Count; i++)
                Word.Append(Words, new Word(tList[i], bList[i], this));
        }
        #endregion

        #region ----------- SAVE PROJECT ------------
        public XElement ToXml()
        {
            XElement xSentence = new XElement("Sentence");
            xSentence.Add(new XAttribute("index", StartIdx));
            xSentence.Add(new XAttribute("bytes", Bytes));
            XElement xWords = new XElement("Words");
            foreach (Word word in Words)
                xWords.Add(word.ToXml());
            xSentence.Add(xWords);
            return xSentence;
        }

        public static void RunID(LinkedList<Sentence> Sentences, int StartNumber)
        {
            foreach (Sentence sentence in Sentences)
                sentence.ID = StartNumber++;
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
        /*
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
        */
        #endregion

        #region --------- STATIC METHODS ------------
        public static void Append(LinkedList<Sentence> list, Sentence sentence)
        {
            if (list == null)
                throw new Exception("Sentences list is null, cannot append.");
            if (list.Last != null && list.Last.Value.StartIdx == sentence.StartIdx)
                list.RemoveLast();
            if (sentence.StartIdx < sentence.Block.Length)
                sentence.node = list.AddLast(sentence);
        }
        #endregion
    }
}
