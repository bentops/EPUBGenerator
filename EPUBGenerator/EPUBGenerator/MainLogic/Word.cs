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
        private int _SIndex { get; set; }
        private LinkedListNode<Word> Node { get; set; }
        private ProjectInfo ProjectInfo { get { return Content.ProjectInfo; } }

        public Content Content { get { return Sentence.Content; } }
        public Block Block { get { return Sentence.Block; } }
        public Sentence Sentence { get; private set; }
        public String OriginalText { get { return Block.Text.Substring(StartIdx, Length); } }
        public int StartIdx { get { return _SIndex + Sentence.StartIdx; } }
        public int Length { get { return (Next == null ? Sentence.Length : Next._SIndex) - _SIndex; } }
        public Word Previous { get { return Node.Previous == null ? null : Node.Previous.Value; } }
        public Word Next { get { return Node.Next == null ? null : Node.Next.Value; } }

        public long Begin { get; private set; }
        public long End { get { return (Next == null ? Sentence.Bytes : Next.Begin); } }

        public int DictIndex { get; private set; }
        public String Pronunciation
        {
            get
            {
                if (DictIndex == 0)
                    return OriginalText;
                return ProjectInfo.Dictionary[OriginalText][DictIndex];
            }
        }
        public List<String> PronunciationList { get { return Pronunciation.Split('-').ToList(); } }


        #region ----------- NEW PROJECT ------------
        public Word(int start, long begin, Sentence sentence)
        {
            DictIndex = 0;
            _SIndex = start;
            Begin = begin;
            Sentence = sentence;
            AppendTo(Sentence.Words);
        }
        #endregion

        #region ----------- SAVE PROJECT ------------
        public XElement ToXml()
        {
            XElement xWord = new XElement("Word");
            xWord.Add(new XAttribute("start", _SIndex));
            xWord.Add(new XAttribute("begin", Begin));
            xWord.Add(new XAttribute("text", OriginalText));
            if (ProjectInfo.CurrentRunWord != null && this == ProjectInfo.CurrentRunWord.Word)
                xWord.Add(new XAttribute("selected", ""));
            if (DictIndex > 0)
                xWord.Add(new XAttribute("dict", DictIndex));
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
                    case "start": _SIndex = int.Parse(value); break;
                    case "begin": Begin = int.Parse(value); break;
                    case "dict": DictIndex = int.Parse(value); break;
                }
            }
            AppendTo(Sentence.Words);
        }
        #endregion

        #region ----------- EDIT PROJECT ------------
        private Word(int start, Word word)
        {
            DictIndex = 0;
            _SIndex = start;
            Sentence = word.Sentence;
            AddAfter(word.Node, Sentence.Words);
        }

        public Word SplitAt(int index)
        {
            DictIndex = 0;
            Word newWord = new Word(_SIndex + index, this);
            Sentence.Resynthesize();
            return newWord;
        }

        public void MergeWith(Word nextWord)
        {
            if (!Sentence.Equals(nextWord.Sentence))
                Sentence.MergeWith(nextWord.Sentence);
            if (!Next.Equals(nextWord))
                throw new Exception("Two words (to be merged) are not adjacent.");
            DictIndex = 0;
            Sentence.Words.Remove(nextWord.Node);
            Sentence.Resynthesize();
        }

        public void MoveTo(Sentence prevSentence)
        {
            _SIndex = StartIdx - prevSentence.StartIdx;
            Begin += prevSentence.Bytes;
            Sentence = prevSentence;
            AppendTo(Sentence.Words);
        }

        public void SetBegin(long begin)
        {
            Begin = begin;
        }

        public void ChangeDictIndex(int index)
        {
            DictIndex = index;
            Sentence.Resynthesize();
        }
        #endregion

        #region ---------- PRIVATE METHODS -----------
        private void AppendTo(LinkedList<Word> list)
        {
            if (list == null)
                throw new Exception("Words list is null, cannot append.");
            if (list.Last != null && list.Last.Value.StartIdx == StartIdx)
                list.RemoveLast();
            if (_SIndex < Sentence.Length)
                Node = list.AddLast(this);
        }

        private void AddAfter(LinkedListNode<Word> refNode, LinkedList<Word> list)
        {
            if (list == null)
                throw new Exception("Words list is null, cannot append.");
            Node = list.AddAfter(refNode, this);
        }
        #endregion
    }
}
