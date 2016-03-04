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

        private int sIdx { get; set; }
        private LinkedListNode<Word> node { get; set; }

        public Content Content { get { return Sentence.Content; } }
        public Block Block { get { return Sentence.Block; } }
        public Sentence Sentence { get; private set; }
        public String OriginalText { get { return Block.Text.Substring(StartIdx, Length); } }
        public int StartIdx { get { return sIdx + Sentence.StartIdx; } }
        public int Length { get { return (Next == null ? Sentence.Length : Next.sIdx) - sIdx; } }
        public Word Previous { get { return node.Previous == null ? null : node.Previous.Value; } }
        public Word Next { get { return node.Next == null ? null : node.Next.Value; } }

        public long Begin { get; private set; }
        public long End { get { return (Next == null ? Sentence.Bytes : Next.Begin); } }

        public String Pronunciation { get; private set; }

        public Word(int start, long begin, Sentence sentence)
        {
            sIdx = start;
            Begin = begin;
            Sentence = sentence;
        }

        #region ----------- SAVE PROJECT ------------
        public XElement ToXml()
        {
            XElement xWord = new XElement("Word");
            xWord.Add(new XAttribute("start", sIdx));
            xWord.Add(new XAttribute("begin", Begin));
            xWord.Add(new XAttribute("text", OriginalText));
            if (pronunciation != null)
                xWord.Add(new XAttribute("pronun", Pronunciation));
            return xWord;
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
        /*/
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
        /*/
        #endregion

        #region ----------- EDIT PROJECT ------------

        #endregion

        #region ---------- STATIC METHODS -----------
        public static void Append(LinkedList<Word> list, Word word)
        {
            if (list == null)
                throw new Exception("Words list is null, cannot append.");
            if (list.Last != null && list.Last.Value.StartIdx == word.StartIdx)
                list.RemoveLast();
            if (word.sIdx < word.Sentence.Length)
                word.node = list.AddLast(word);
        }
        #endregion
    }
}
