using System.Collections.Generic;
using TTS.Preprocessors;

namespace TTS
{
    public class CPreprocessor
    {
        private Dictionary<int, int> Map;
        private Dictionary<int, IPreprocessor> Preprocessors;

        public CPreprocessor()
        {
            Map = new Dictionary<int, int>();
            Map.Add(1, 1);
            Map.Add(2, 1);
            Map.Add(3, 1);
            Map.Add(4, 1);
            Map.Add(5, 1);

            Preprocessors = new Dictionary<int, IPreprocessor>();
            Preprocessors.Add(1, new Dummy());
        }

        public string Process(string input, int type)
        {
            if (!Map.ContainsKey(type))
                return input;
            return Preprocessors[Map[type]].Process(input);
        }
    }
}
