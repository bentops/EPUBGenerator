﻿using System.Collections.Generic;

namespace ChulaTTS.G2PConverter.G2P
{
    public class G2PNum
    {
        private Dictionary<string, string> Dict;

        public G2PNum()
        {
            Dict = new Dictionary<string, string>();
            Dict.Add("0", "suun4");
            Dict.Add("1", "nUN1");
            Dict.Add("2", "sOON4");
            Dict.Add("3", "saam4");
            Dict.Add("4", "sii1");
            Dict.Add("5", "haa2");
            Dict.Add("6", "hok1");
            Dict.Add("7", "cet1");
            Dict.Add("8", "pxxt1");
            Dict.Add("9", "kaw2");
            Dict.Add("sib", "sip1");
            Dict.Add("roy", "rOOj3");
            Dict.Add("pun", "phan0");
            Dict.Add("mean", "mUUn1");
            Dict.Add("san", "sxxn4");
            Dict.Add("lan", "laan3");
            Dict.Add("aed", "et1");
            Dict.Add("yee", "jii2");
            Dict.Add("minus", "lop3");
        }

        public string GenTranscript(string inp)
        {
            string Outp = "";
            string binp = inp;

            //reduce zero
            if (inp.Length >= 2)
            {
                while (inp.Substring(0, 1) == "0")
                {
                    if (inp.Length == 1)
                    {
                        return Dict["0"];
                    }
                    else
                    {
                        inp = inp.Substring(1);
                    }
                }
            }

            if (inp == "0" || inp == "-0")
            {
                return Dict["0"];
            }

            if (inp.Substring(0, 1) == "-")
            {
                Outp = "}" + Dict["minus"];
                binp = binp.Substring(1);
            }

            binp = binp.Replace(",", "");
            if (binp.Length == 0)
            {
                return "";
            }

            while (binp.Length > 0)
            {
                if (binp.Substring(0, 1) == "0")
                {
                    if ((binp.Length - 1) % 6 == 0 && binp.Length > 1)
                    {
                        Outp = Outp + "}" + Dict["lan"];
                    }
                    binp = binp.Substring(1);
                }
                else
                {
                    Outp = Outp + "}" + Dict[binp.Substring(0, 1)];
                    if ((binp.Length - 1) % 6 == 0 && binp.Length > 1)
                    {
                        Outp = Outp + "}" + Dict["lan"];
                    }
                    else if ((binp.Length - 1) % 6 == 5 && binp.Length > 1)
                    {
                        Outp = Outp + "}" + Dict["san"];
                    }
                    else if ((binp.Length - 1) % 6 == 4 && binp.Length > 1)
                    {
                        Outp = Outp + "}" + Dict["mean"];
                    }
                    else if ((binp.Length - 1) % 6 == 3 && binp.Length > 1)
                    {
                        Outp = Outp + "}" + Dict["pun"];
                    }
                    else if ((binp.Length - 1) % 6 == 2 && binp.Length > 1)
                    {
                        Outp = Outp + "}" + Dict["roy"];
                    }
                    else if ((binp.Length - 1) % 6 == 1 && binp.Length > 1)
                    {
                        Outp = Outp + "}" + Dict["sib"];
                    }
                    binp = binp.Substring(1);
                }
            }

            Outp = Outp.Replace(Dict["1"] + "}" + Dict["sib"], Dict["sib"]);
            Outp = Outp.Replace(Dict["2"] + "}" + Dict["sib"], Dict["yee"] + " " + Dict["sib"]);
            if (inp.Length > 1)
            {
                if (Outp.Substring(Outp.Length - Dict["1"].Length) == Dict["1"])
                {
                    Outp = Outp.Substring(0, Outp.Length - Dict["1"].Length) + Dict["aed"];
                }
            }

            Outp = Outp.Replace(Dict["1"] + "}" + Dict["lan"], Dict["aed"] + " " + Dict["lan"]);

            if (Outp.Substring(1, Dict["aed"].Length) == Dict["aed"])
            {
                Outp = "}" + Dict["1"] + Outp.Substring(1 + Dict["aed"].Length);
            }

            return Outp.Substring(1);
        }

        public List<KeyValuePair<string, string>> GenTranscriptList(string inp)
        {
            return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(inp, inp) };
        }

        public List<List<string>> GenPronunciationAndTranscriptList(string inp)
        {
            return new List<List<string>>() { new List<string>() { inp, inp, inp } };
        }
    }
}