using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    class Block
    {
        public int ID { get; set; }
        public String BID { get { return "B" + ID.ToString("D3"); } }
        public List<Sentence> Sentences { get; set; }

        public String Text
        {
            get
            {
                if (Sentences == null)
                    return null;
                String text = "";
                foreach (Sentence s in Sentences)
                    text += s.Text + " ";
                return Text;
            }
        }

        public Block(int id, String text)
        {
            ID = id;

            // DO SOME SPLIT HERE //
            int count = 0;
            Sentences = new List<Sentence>();
            foreach (KeyValuePair<String, Int32> sentence in Tools.SentenceSplitter.Split(text))
            {
                if (String.IsNullOrWhiteSpace(sentence.Key)) continue;
                Sentences.Add(new Sentence(count++, sentence.Value, sentence.Key));
            }
        }

        public Block(int id, List<Sentence> sentences)
        {
            ID = id;
            Sentences = sentences;
        }
    }
}
