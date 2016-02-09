using System.Collections.Generic;

namespace TTS.G2Ps
{
    public class G2PSym : IG2P
    {
        private Dictionary<string, string> Dict = new Dictionary<string, string>();
        public G2PSym()
        {
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
            Dict.Add(".", "cut1");
            Dict.Add("(", "p@@t1}woN0}lep3");
            Dict.Add(")", "pit1}woN0}lep3");
        }

        public string GenTranscript(string inp)
        {
            string outp = "";
            while (inp.Length > 0)
            {
                if (Dict.ContainsKey(inp.Substring(0, 1)))
                    outp = outp + "}" + Dict[inp.Substring(0, 1)];
                inp = inp.Substring(1);
            }
            if (outp.Length > 0)
            {
                outp = outp.Substring(1);
            }
            return outp;
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
