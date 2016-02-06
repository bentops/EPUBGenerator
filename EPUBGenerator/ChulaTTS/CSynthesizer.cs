using System.Collections.Generic;
using System.IO;
using TTS.Synthesizers;

namespace TTS
{
    public class CSynthesizer
    {
        private Dictionary<int, int> Map;
        private Dictionary<int, ISynthesizer> Synthesizers;

        private double _speed;
        private double _pitch;
        private int _frequency;

        public double Speed
        {
            get { return _speed; }
            set
            {
                _speed = value;
                foreach (ISynthesizer synthesizer in Synthesizers.Values)
                    synthesizer.SetSpeed(_speed);
            }
        }

        public double Pitch
        {
            get { return _pitch; }
            set
            {
                _pitch = value;
                foreach (ISynthesizer synthesizer in Synthesizers.Values)
                    synthesizer.SetPitch(_pitch);
            }
        }

        public int Frequency
        {
            get { return _frequency; }
            set
            {
                _frequency = value;
                foreach (ISynthesizer synthesizer in Synthesizers.Values)
                    synthesizer.SetFrequency(_frequency);
            }
        }

        public CSynthesizer()
        {
            // {1 - 5} -> {1 - 2}
            Map = new Dictionary<int, int>();
            Map.Add(1, 1);
            Map.Add(2, 2);
            Map.Add(3, 1);
            Map.Add(4, 1);
            Map.Add(5, 1);

            // {1 - 2} -> {Synthesizers}
            Synthesizers = new Dictionary<int, ISynthesizer>();
            Synthesizers.Add(1, new Alpha1());
            Synthesizers.Add(2, new SAPI());
        }
        
        public void Reset()
        {
            foreach (ISynthesizer synthesizer in Synthesizers.Values)
                synthesizer.Dispose();
        }

        public List<KeyValuePair<int, string>> GetSynthesizerNames()
        {
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            foreach (KeyValuePair<int, ISynthesizer> syn in Synthesizers)
                list.Add(new KeyValuePair<int, string>(syn.Key, syn.Value.About()));
            return list;
        }

        public void SetModel(int slot, string modelName)
        {
            Synthesizers[slot].SetModel(modelName);
        }

        public List<string> GetModel(int slot)
        {
            return Synthesizers[slot].GetModel();
        }

        public MemoryStream Synthesize(string input, int type)
        {
            if (!Map.ContainsKey(type)) return new MemoryStream();
            return Synthesizers[Map[type]].Synthesize(input);
        }
    }
}
