using System.Collections.Generic;
using TTS.G2Ps;

namespace TTS
{
    public class CG2P
    {
        private Dictionary<int, int> Map;
        private Dictionary<int, IG2P> G2Ps;

        public CG2P()
        {
            // {1 - 5} -> {1 - 4}
            Map = new Dictionary<int, int>();
            Map.Add(1, 1);
            Map.Add(2, 2);
            Map.Add(3, 3);
            Map.Add(4, 4);
            Map.Add(5, 4);

            // {1 - 4} -> {G2Ps}
            G2Ps = new Dictionary<int, IG2P>();
            G2Ps.Add(1, new G2PTh());
            G2Ps.Add(2, new G2PEn());
            G2Ps.Add(3, new G2PNum());
            G2Ps.Add(4, new G2PSym());
        }

        public string GenTranscript(string input, int type)
        {
            if (!Map.ContainsKey(type))
                return input;
            return G2Ps[Map[type]].GenTranscript(input);
        }

        public List<KeyValuePair<string, string>> GenTranscriptList(string input, int type)
        {
            if (!Map.ContainsKey(type))
                return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(input, input) };
            return G2Ps[Map[type]].GenTranscriptList(input);
        }

        public List<List<string>> GenPronunciationAndTranscriptList(string input, int type)
        {
            if (!Map.ContainsKey(type))
                return new List<List<string>>() { new List<string>() { input, input, input } };
            return G2Ps[Map[type]].GenPronunciationAndTranscriptList(input);
        }
    }
}
