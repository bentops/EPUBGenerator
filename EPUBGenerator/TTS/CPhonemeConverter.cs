using System.Collections.Generic;
using TTS.PhonemeConverters;

namespace TTS
{
    public class CPhonemeConverter
    {
        private Dictionary<int, int> Map;
        private Dictionary<int, IPhonemeConverter> PhonemeConverters;

        public CPhonemeConverter()
        {
            // {1 - 5} -> {1 - 3}
            Map = new Dictionary<int, int>();
            Map.Add(1, 1);
            Map.Add(2, 3);
            Map.Add(3, 2);
            Map.Add(4, 2);
            Map.Add(5, 2);
            Map.Add(6, 4);

            // {1 - 3} -> {PhonemeConverters}
            PhonemeConverters = new Dictionary<int, IPhonemeConverter>();
            PhonemeConverters.Add(1, new ThaiPhonemeConverter());
            PhonemeConverters.Add(2, new NumberPhonemeConverter());
            PhonemeConverters.Add(3, new EngPhonemeConverter());
            PhonemeConverters.Add(4, new NullPhonemeConverter());
        }

        public string Convert(string input, int type)
        {
            if (!Map.ContainsKey(type))
                return input;
            return PhonemeConverters[Map[type]].Convert(input);
        }
    }
}
