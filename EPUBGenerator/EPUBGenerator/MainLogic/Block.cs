using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    abstract class Block
    {
        protected ProjectInfo ProjectInfo { get { return Content.ProjectInfo; } }

        public int ID { get; protected set; }
        public abstract String B_ID { get; }

        public Content Content { get; protected set; }
        public String Text { get; protected set; }
        public int Length { get { return Text.Length; } }
        public LinkedList<Sentence> Sentences { get; protected set; }
        public long Bytes
        {
            get
            {
                long bytes = 0;
                foreach (Sentence sentence in Sentences)
                    bytes += sentence.Bytes;
                return bytes;
            }
        }

        public abstract XElement ToXml();


        #region ----------- PROTECTED METHODS ------------
        protected static List<int> Split(String text)
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

        protected static void SplitThaiWord(List<int> list, String text, int offset)
        {
            Regex sRegex = new Regex(@"[\s]+");
            String[] sList = sRegex.Split(text);

            int startIndex = 0;
            IncreasinglyAppend(list, offset + startIndex);
            foreach (String cutText in sList)
            {
                if (String.IsNullOrWhiteSpace(cutText))
                    continue;
                IncreasinglyAppend(list, offset + startIndex);
                int index = text.IndexOf(cutText, startIndex);
                if (index < 0)
                    throw new Exception("Wrong Index Text: " + cutText + " " + startIndex + " " + text);
                startIndex = index + cutText.Length;
                IncreasinglyAppend(list, offset + startIndex);
            }
        }

        protected static void IncreasinglyAppend(List<int> list, int num)
        {
            int count = list.Count;
            if (count == 0 || list[count - 1] < num)
                list.Add(num);
        }
        #endregion
    }
}
