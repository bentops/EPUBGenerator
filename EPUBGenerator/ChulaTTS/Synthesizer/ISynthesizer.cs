using System;
using System.Collections.Generic;

namespace ChulaTTS.Synthesizer
{
    interface ISynthesizer
    {
        byte[] Synthesis(List<KeyValuePair<String, Int32>> inp);
        void SetFrequency(int fs);
        void SetModel(String modelName);
        void SetPitch(double pitch);
        void SetSpeed(double speed);
        List<String> GetModel();
        void Dispose();
        String About();
    }
}
