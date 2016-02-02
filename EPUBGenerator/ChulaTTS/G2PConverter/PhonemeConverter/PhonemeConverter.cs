using System.Collections.Generic;
using Wirote2TSync;

namespace ChulaTTS.G2PConverter.PhonemeConverter
{
    public class PhonemeConverter : IPhonemeConverter
    {
        private Dictionary<int, int> Map;

        private Converter Wirote2TSync;

        public PhonemeConverter()
        {
            Map = new Dictionary<int, int>();
            Map.Add(1, 1);
            Map.Add(2, 2);
            Map.Add(3, 2);
            Map.Add(4, 2);
            Map.Add(5, 2);

            Wirote2TSync = new Converter();
        }

        public string[] C2Pronunciation(string Input)
        {
            return Wirote2TSync.C2Pronunciation(Input);
        }

        public string Convert(string Input, int Type)
        {
            switch (Map[Type])
            {
                case 1: return Wirote2TSync.Conversion4(Input);
                case 2: return Wirote2TSync.Conversion(Input);
                default: return Input;
            }
        }
    }
}
