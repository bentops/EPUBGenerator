using System.Collections.Generic;
using System.IO;
using TTS.Synthesizers;

namespace TTS
{
    public class CSynthesizer
    {
        private Dictionary<int, int> Map;
        private Dictionary<int, ISynthesizer> Synthesizers;

        private double speed;
        private double pitch;
        private int frequency;
        private string tempPath;

        public double Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                foreach (ISynthesizer synthesizer in Synthesizers.Values)
                    synthesizer.SetSpeed(speed);
            }
        }
        public double Pitch
        {
            get { return pitch; }
            set
            {
                pitch = value;
                foreach (ISynthesizer synthesizer in Synthesizers.Values)
                    synthesizer.SetPitch(pitch);
            }
        }
        public int Frequency
        {
            get { return frequency; }
            set
            {
                frequency = value;
                foreach (ISynthesizer synthesizer in Synthesizers.Values)
                    synthesizer.SetFrequency(frequency);
            }
        }
        public string TempPath
        {
            get { return tempPath; }
            set
            {
                tempPath = value;
                foreach (ISynthesizer synthesizer in Synthesizers.Values)
                    synthesizer.SetTemp(tempPath);
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
            Map.Add(6, 1);

            // {1 - 2} -> {Synthesizers}
            Synthesizers = new Dictionary<int, ISynthesizer>();
            Synthesizers.Add(1, new Alpha1());
            Synthesizers.Add(2, new SAPI());

            Speed = 1.0;
            Frequency = 16000;
            TempPath = Path.GetTempPath();
        }
        
        public void Synthesize(string input, int type, string id, string outputPath)
        {
            if (!Map.ContainsKey(type)) return;
            Synthesizers[Map[type]].Synthesize(input, id, outputPath);
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

    }
}
