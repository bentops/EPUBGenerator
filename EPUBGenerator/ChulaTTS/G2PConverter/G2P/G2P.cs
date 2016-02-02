using System.Collections.Generic;

namespace ChulaTTS.G2PConverter.G2P
{
    public class G2P : IG2P
    {
        private Dictionary<int, int> Map;

        private G2PTh ThG2P;
        private G2PEn EnG2P;
        private G2PNum NumG2P;
        private G2PSym SymG2P;

        public G2P()
        {
            Map = new Dictionary<int, int>();
            Map.Add(1, 1);
            Map.Add(2, 2);
            Map.Add(3, 3);
            Map.Add(4, 4);
            Map.Add(5, 4);

            ThG2P = new G2PTh();
            EnG2P = new G2PEn();
            NumG2P = new G2PNum();
            SymG2P = new G2PSym();
        }
        
        public string GenTranscript(string input, int type)
        {
            switch (Map[type])
            {
                case 1: return ThG2P.GenTranscript(input);
                case 2: return EnG2P.GenTranscript(input);
                case 3: return NumG2P.GenTranscript(input);
                case 4: return SymG2P.GenTranscript(input);
                default: return input;
            }
        }

        public List<KeyValuePair<string, string>> GenTranscriptList(string input, int type)
        {
            switch (Map[type])
            {
                case 1: return ThG2P.GenTranscriptList(input);
                case 2: return EnG2P.GenTranscriptList(input);
                case 3: return NumG2P.GenTranscriptList(input);
                case 4: return SymG2P.GenTranscriptList(input);
                default: return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(input, input) };
            }
        }

        public List<List<string>> GenPronunciationAndTranscriptList(string input, int type)
        {
            switch (Map[type])
            {
                case 1: return ThG2P.GenPronunciationAndTranscriptList(input);
                case 2: return EnG2P.GenPronunciationAndTranscriptList(input);
                case 3: return NumG2P.GenPronunciationAndTranscriptList(input);
                case 4: return SymG2P.GenPronunciationAndTranscriptList(input);
                default: return new List<List<string>>() { new List<string>() { input, input, input } };
            }
        }
    }
}
