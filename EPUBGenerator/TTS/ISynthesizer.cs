using System.Collections.Generic;
using System.IO;

namespace TTS
{
    public interface ISynthesizer
    {
        MemoryStream Synthesize(string input, string id);
        void SetFrequency(int frequency);
        void SetModel(string modelName);
        void SetPitch(double pitch);
        void SetSpeed(double speed);
        void SetTemp(string path);
        List<string> GetModel();
        void Dispose();
        string About();
    }
}
