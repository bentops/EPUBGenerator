using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    class Word
    {
        private String pronunciation;

        public int ID { get; set; }
        public String WID { get { return "W" + ID.ToString("D3"); } }

        public LinkedListNode<Word> Node { get; set; }
        public String Text { get; private set; }
        public String Transcript { get; private set; }
        public int Type { get; private set; }
        public double StartTime { get; private set; }
        public double EndTime { get; private set; }
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
                Transcript = Tools.G2P.GenTranscript(Pronunciation, Type);
            }
        }
        
        public Word(int type, String text, String transcript)
        {
            Type = type;
            Text = text;
            Transcript = transcript;
        }

        public void SetTime(double start, double end)
        {
            StartTime = start;
            EndTime = end;
        }
        
        public void MergeWithNextWord()
        {
            LinkedList<Word> Words = Node.List;
            LinkedListNode<Word> next = Node.Next;
            if (next == null) return;
            String newText = Text + next.Value.Text;
            String newTranscript = Tools.G2P.GenTranscript(newText, Type);
            Word newWord = new Word(Type, newText, newTranscript);
            Words.AddBefore(Node, newWord);
            Words.Remove(next);
            Words.Remove(Node);
        }
    }
}
