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
        public int ID { get; private set; }
        public String B_ID { get { return "#B-" + ID; } }

        public Content Content { get; private set; }
        public String Text { get; private set; }
        public int Length { get { return Text.Length; } }
        public LinkedList<Sentence> Sentences { get; private set; }

        #region ----------- NEW PROJECT ------------
        public Block(int id, String text, Content content)
        {
            ID = id;
            Text = text;
            Content = content;

            Sentences = new LinkedList<Sentence>();
            foreach (int startIdx in Split(Text))
                new Sentence(startIdx, this);
        }

        private static List<int> Split(String text)
        {
            String eng = @"[a-zA-Z?.,:;'""&$/\\(){}<>[\]\-\s]*";
            Regex eRegex = new Regex(eng + @"[a-zA-Z]+" + eng, RegexOptions.Compiled);
            Regex sRegex = new Regex(@"[\s]+", RegexOptions.Compiled);

            List<int> indexList = new List<int>();
            Match eMatch = eRegex.Match(text);
            int sIndex = 0;
            while (eMatch.Success)
            {
                String eNonMatch = text.Substring(sIndex, eMatch.Index - sIndex);

                // Deal with Non-EngOrSpace
                if (eNonMatch.Length > 0)
                    SplitThaiWord(indexList, eNonMatch, sIndex);

                // Deal with Eng
                IncreasinglyAppend(indexList, eMatch.Index);
                sIndex = eMatch.Index + eMatch.Length;
                eMatch = eMatch.NextMatch();
            }
            SplitThaiWord(indexList, text.Substring(sIndex), sIndex);

            for (int i = 0; i < indexList.Count - 1; i++)
            {
                int start = indexList[i];
                int len = indexList[i + 1] - start;
                Console.WriteLine("/" + text.Substring(start, len) + "/");
            }
            return indexList;
        }

        private static void SplitThaiWord(List<int> list, String text, int offset)
        {
            Regex sRegex = new Regex(@"[\s]+");
            String[] sList = sRegex.Split(text);
            
            int startIndex = 0;
            foreach (String cutText in sList)
            {
                if (String.IsNullOrWhiteSpace(cutText))
                    continue;
                IncreasinglyAppend(list, offset + startIndex);
                int index = text.IndexOf(cutText, startIndex);
                if (index < 0)
                    throw new Exception("Wrong Index Text: " + cutText + " " + startIndex + " " + text);
                //IncreasinglyAppend(list, offset + index);
                startIndex = index + cutText.Length;
            }
            //IncreasinglyAppend(list, offset + startIndex);
        }

        private static void IncreasinglyAppend(List<int> list, int num)
        {
            int count = list.Count;
            if (count == 0 || list[count - 1] < num)
                list.Add(num);
        }
        #endregion

        #region ----------- SAVE PROJECT ------------
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
        #endregion

        #region ----------- OPEN PROJECT ------------
        public Block(XElement xBlock, Content content)
        {
            Content = content;
            ID = int.Parse(xBlock.Attribute("id").Value.Split('-')[1]);
            Text = xBlock.Element("Text").Value;
            Sentences = new LinkedList<Sentence>();
            foreach (XElement xSentence in xBlock.Element("Sentences").Elements("Sentence"))
                new Sentence(xSentence, this);
        }
        #endregion
    }
}
