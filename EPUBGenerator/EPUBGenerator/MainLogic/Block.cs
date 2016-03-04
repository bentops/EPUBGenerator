using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Block
    {
        public int ID { get; set; }
        public String B_ID { get { return "#B-" + ID; } }

        public Content Content { get; private set; }
        public String Text { get; private set; }
        public int Length { get { return Text.Length; } }
        public LinkedList<Sentence> Sentences { get; private set; }

        public Block(int id, String text, Content content)
        {
            ID = id;
            Text = text;
            Content = content;

            Sentences = new LinkedList<Sentence>();
            foreach (int startIdx in Split(Text))
                Sentence.Append(Sentences, new Sentence(startIdx, this));
        }

        public XElement ToXml()
        {
            XElement xBlock = new XElement("Block");
            xBlock.Add(new XAttribute("id", B_ID));
            xBlock.Add(new XElement("Text", Text));
            XElement xSentences = new XElement("Sentences");
            foreach (Sentence sentence in Sentences)
                xSentences.Add(sentence.ToXml());
            xBlock.Add(xSentences);
            return xBlock;
        }

        private static List<int> Split(String text)
        {
            Regex regex = new Regex(@"\s");
            String[] sList = regex.Split(text);

            List<int> indexList = new List<int>();
            int startIndex = 0;
            indexList.Add(0);
            foreach (String cutText in sList)
            {
                if (String.IsNullOrWhiteSpace(cutText))
                    continue;
                Console.WriteLine(cutText);
                int index = text.IndexOf(cutText, startIndex);
                if (index < 0)
                    throw new Exception("Wrong Index Text: " + cutText + " " + startIndex + " " + text);
                startIndex = index + cutText.Length;
                indexList.Add(startIndex);
            }
            return indexList;
        }
    }
}
