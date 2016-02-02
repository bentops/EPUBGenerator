using System.Collections.Generic;

namespace ChulaTTS.G2PConverter.Preprocessor
{
    public class Preprocessor : IPreprocessor
    {
        private Dictionary<int, int> Map;

        private Dummy Dummy;

        public Preprocessor()
        {
            Map = new Dictionary<int, int>();
            Map.Add(1, 1);
            Map.Add(2, 1);
            Map.Add(3, 1);
            Map.Add(4, 1);
            Map.Add(5, 1);

            Dummy = new Dummy();
        }
        public string Process(string Input, int Type)
        {
            switch (Map[Type])
            {
                case 1: return Dummy.Process(Input);
                default: return Input;
            }
        }
    }
}
