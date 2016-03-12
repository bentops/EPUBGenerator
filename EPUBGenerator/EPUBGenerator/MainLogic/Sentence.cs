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
        private LinkedListNode<Sentence> Node { get; set; }

        public int ID { get; private set; }
        public String SID { get { return "S" + ID.ToString("D6"); } }
        public String WavName { get { return Project.GetWavNameFromID(ID); } }
        public String WavPath { get { return Path.Combine(Content.ContentAudio, WavName); } }
        public bool IsRandomID { get { return Project.IsRandom(ID); } }

        public Content Content { get { return Block.Content; } }
        public Block Block { get; private set; }
        public String OriginalText { get { return Block.Text.Substring(StartIdx, Length); } }
        public int StartIdx { get; private set; }
        public int Length { get { return (Next == null ? Block.Length : Next.StartIdx) - StartIdx; } }
        public Sentence Previous { get { return Node.Previous == null ? null : Node.Previous.Value; } }
        public Sentence Next { get { return Node.Next == null ? null : Node.Next.Value; } }
        public LinkedList<Word> Words { get; private set; }

        public long Bytes { get; private set; }

        public List<List<String>> FinalTextList
        {
            get
            {
                List<List<String>> list = new List<List<String>>();
                foreach (Word word in Words)
                    list.Add(new List<String>() { word.OriginalText });
                return list;
            }
        }

        #region ----------- NEW PROJECT ------------
        public Sentence(int start, Block block)
        {
            StartIdx = start;
            Block = block;
            AppendTo(Block.Sentences);
        }

        public void Synthesize()
        {
            ID = Project.GetRandomUniqueID(Content.ContentAudio);
            Bytes = Project.Synthesizer.Synthesize(OriginalText, WavPath);
            List<int> tList = Project.Synthesizer.TextIndexList;
            List<long> bList = Project.Synthesizer.ByteIndexList;
            if (tList == null)
                throw new Exception("Null Synthesized Text Index List");
            if (bList == null)
                throw new Exception("Null Synthesized Byte Index List");

            Words = new LinkedList<Word>();
            for (int i = 0; i < tList.Count; i++)
                new Word(tList[i], bList[i], this);
        }
        #endregion

        #region ----------- SAVE PROJECT ------------
        public XElement ToXml()
        {
            XElement xSentence = new XElement("Sentence");
            xSentence.Add(new XAttribute("id", SID));
            xSentence.Add(new XAttribute("index", StartIdx));
            xSentence.Add(new XAttribute("bytes", Bytes));
            XElement xWords = new XElement("Words");
            foreach (Word word in Words)
                xWords.Add(word.ToXml());
            xSentence.Add(xWords);
            return xSentence;
        }
        
        public void UseRandomizedID()
        {
            if (!File.Exists(WavPath))
                throw new Exception("NO AUDIO FILE YET.");

            if (IsRandomID)
                return;

            String oldAudio = WavPath;
            ID = Project.GetRandomUniqueID(Content.ContentAudio);
            File.Move(oldAudio, WavPath);
        }

        public void UseNonRandomizedID(int id)
        {
            if (!File.Exists(WavPath))
                throw new Exception("NO AUDIO FILE YET. " + WavPath);

            String oldAudio = WavPath;
            ID = id;
            File.Move(oldAudio, WavPath);
        }

        #endregion

        #region ----------- OPEN PROJECT ------------
        public Sentence(XElement xSentence, Block block)
        {
            Block = block;
            foreach (XAttribute attribute in xSentence.Attributes())
            {
                String value = attribute.Value;
                switch (attribute.Name.ToString())
                {
                    case "id": ID = int.Parse(value.Substring(1)); break;
                    case "index": StartIdx = int.Parse(value); break;
                    case "bytes": Bytes = int.Parse(value); break;
                }
            }
            AppendTo(Block.Sentences);
            Words = new LinkedList<Word>();
            foreach (XElement xWord in xSentence.Element("Words").Elements("Word"))
                new Word(xWord, this);
        }
        #endregion

        #region ----------- EDIT PROJECT ------------
        public void MergeWith(Sentence nextSentence)
        {
            if (!Next.Equals(nextSentence))
                throw new Exception("Two sentences (to be merged) are not adjacent.");
            
            Block.Sentences.Remove(nextSentence.Node);
            foreach (Word word in nextSentence.Words)
                word.MoveTo(this);
            Bytes += nextSentence.Bytes;
        }

        public void Resynthesize()
        {
            ID = Project.GetRandomUniqueID(Content.ContentAudio);
            Bytes = Project.Synthesizer.Synthesize(FinalTextList, WavPath);
            List<long> bList = Project.Synthesizer.ByteIndexList;
            if (bList == null)
                throw new Exception("Null Synthesized Byte Index List");
            if (bList.Count != Words.Count + 1)
                throw new Exception("Wrong ByteIndexList, bList/Words = " + bList.Count + "/" + Words.Count);

            int i = 0;
            foreach (Word word in Words)
                word.SetBegin(bList[i++]);
        }
        #endregion

        #region --------- PRIVATE METHODS ------------
        private void AppendTo(LinkedList<Sentence> list)
        {
            if (list == null)
                throw new Exception("Sentences list is null, cannot append.");
            if (list.Last != null && list.Last.Value.StartIdx == StartIdx)
                list.RemoveLast();
            if (StartIdx < Block.Length)
                Node = list.AddLast(this);
        }
        #endregion

    }
}
