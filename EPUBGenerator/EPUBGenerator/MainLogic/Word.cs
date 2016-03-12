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


        #region ----------- NEW PROJECT ------------
        public Word(int start, long begin, Sentence sentence)
        {
            sIdx = start;
            Begin = begin;
            Sentence = sentence;
            AppendTo(Sentence.Words);
        }
        #endregion

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
        public Word(XElement xWord, Sentence sentence)
        {
            Sentence = sentence;
            foreach (XAttribute attribute in xWord.Attributes())
            {
                String value = attribute.Value;
                switch (attribute.Name.ToString())
                {
                    case "start": sIdx = int.Parse(value); break;
                    case "begin": Begin = int.Parse(value); break;
                    case "pronun": Pronunciation = value; break;
                }
            }
            AppendTo(Sentence.Words);
        }
        #endregion

        #region ----------- EDIT PROJECT ------------
        private Word(int start, Word word)
        {
            sIdx = start;
            Sentence = word.Sentence;
            AddAfter(word.node, Sentence.Words);
        }

        public Word SplitAt(int index)
        {
            Word newWord = new Word(sIdx + index, this);
            Sentence.Resynthesize();
            return newWord;
        }

        public void MergeWith(Word nextWord)
        {
            if (!Sentence.Equals(nextWord.Sentence))
                Sentence.MergeWith(nextWord.Sentence);
            if (!Next.Equals(nextWord))
                throw new Exception("Two words (to be merged) are not adjacent.");

            Sentence.Words.Remove(nextWord.node);
            Sentence.Resynthesize();
        }

        public void MoveTo(Sentence prevSentence)
        {
            sIdx = StartIdx - prevSentence.StartIdx;
            Begin += prevSentence.Bytes;
            Sentence = prevSentence;
            AppendTo(Sentence.Words);
        }

        public void SetBegin(long begin)
        {
            Begin = begin;
        }
        #endregion

        #region ---------- PRIVATE METHODS -----------
        private void AppendTo(LinkedList<Word> list)
        {
            if (list == null)
                throw new Exception("Words list is null, cannot append.");
            if (list.Last != null && list.Last.Value.StartIdx == StartIdx)
                list.RemoveLast();
            if (sIdx < Sentence.Length)
                node = list.AddLast(this);
        }

        private void AddAfter(LinkedListNode<Word> refNode, LinkedList<Word> list)
        {
            if (list == null)
                throw new Exception("Words list is null, cannot append.");
            node = list.AddAfter(refNode, this);
        }
        #endregion
    }
}
