using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ChulaTTS.G2PConverter
{
    public class SentenceSplitter
    {

        //Define type 1 = Thai
        //Define type 2 = English
        //Define type 3 = number read group
        //Define type 4 = number read individual
        //Define type 5 = Special Character;

        private static Regex cutter1 = new Regex("[ก-ูเ-์]+", RegexOptions.Compiled);
        private static Regex cutter2 = new Regex("[,0-9๐-๙]+", RegexOptions.Compiled);
        private static Regex cutter3 = new Regex("[a-zA-Z]+", RegexOptions.Compiled);

        public static List<KeyValuePair<String, Int32>> Split(String inp)
        {
            Match m;
            Int32 offset;
            String buffp = "";

            m = cutter1.Match(inp);
            offset = 0;
            while (m.Success)
            {
                buffp = buffp + inp.Substring(offset, m.Index - offset);
                buffp = buffp + " " + m.Value + " ";
                offset = m.Index + m.Value.Length;
                m = m.NextMatch();
            }
            buffp = buffp + inp.Substring(offset);

            inp = buffp;
            buffp = "";
            offset = 0;
            m = cutter2.Match(inp);
            while (m.Success)
            {
                buffp = buffp + inp.Substring(offset, m.Index - offset);
                if (m.Value.Substring(m.Value.Length - 1) == ",")
                {
                    buffp = buffp + " " + m.Value.Substring(0, m.Value.Length - 1) + " , ";
                }
                else
                {
                    buffp = buffp + " " + m.Value.Replace(",", "") + " ";
                }
                offset = m.Index + m.Value.Length;
                m = m.NextMatch();
            }
            buffp = buffp + inp.Substring(offset);

            inp = buffp;
            buffp = "";
            offset = 0;
            m = cutter3.Match(inp);
            while (m.Success)
            {
                buffp = buffp + inp.Substring(offset, m.Index - offset);
                buffp = buffp + " " + m.Value + " ";
                offset = m.Index + m.Value.Length;
                m = m.NextMatch();
            }
            buffp = buffp + inp.Substring(offset);


            String[] buffa;
            buffa = buffp.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<KeyValuePair<String, Int32>> outp = new List<KeyValuePair<String, Int32>>();
            KeyValuePair<String, Int32> buffk = new KeyValuePair<string, int>();

            Match m1, m2, m3;
            Int32 buffVal;
            String buffKey;
            Int32 mergeItem = 0;
            for (int a = 0; a < buffa.Length; a++)
            {
                //if (a == 16)
                //    a = 16;
                m1 = cutter1.Match(buffa[a]);
                m2 = cutter3.Match(buffa[a]);
                m3 = cutter2.Match(buffa[a]);
                buffKey = buffa[a];
                if (m1.Success)
                {
                    buffVal = 1;
                }
                else if (m2.Success)
                {
                    buffVal = 2;
                }
                else if (m3.Success)
                {
                    if (outp.Count > 1)
                    {
                        if (outp[a - 2 - mergeItem].Value == 3 && outp[a - 1 - mergeItem].Key == ".")
                        {
                            buffVal = 4;
                        }
                        else if (buffa[a] == "." || buffa[a] == ",")
                        {
                            buffVal = 5;
                        }
                        else
                        {
                            buffVal = 3;
                        }
                    }
                    else
                    {
                        if (buffa[a] == "." || buffa[a] == ",")
                            buffVal = 5;
                        else
                            buffVal = 3;
                    }
                }
                else
                {
                    buffVal = 5;
                }
                if (outp.Count > 0)
                {
                    if (outp[outp.Count - 1].Key == "-" && buffVal == 3)
                    {
                        outp.RemoveAt(outp.Count - 1);
                        buffKey = "-" + buffKey;
                        mergeItem++;
                    }
                }
                buffk = new KeyValuePair<String, Int32>(buffKey, buffVal);
                outp.Add(buffk);
            }

            return outp;
        }
    }
}
