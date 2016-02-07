using System.Collections.Generic;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;

namespace TTS.Synthesizers
{
    class SAPI : ISynthesizer
    {
        private int _frequency;
        private SpeechSynthesizer _speechSynthesizer;

        public SAPI()
        {
            _frequency = 16000;
            _speechSynthesizer = new SpeechSynthesizer();
            _speechSynthesizer.Volume = 100;
        }

        public string About()
        {
            return "Microsoft SAPI";
        }

        public void Dispose()
        {
            _speechSynthesizer.Dispose();
        }

        public List<string> GetModel()
        {
            return new List<string>() { "default" };
        }

        public void SetFrequency(int frequency)
        {
            _frequency = frequency;
        }

        public void SetModel(string modelName)
        {
        }

        public void SetPitch(double pitch)
        {
        }

        public void SetSpeed(double speed)
        {
            _speechSynthesizer.Rate = (int)(4 * speed - 4);
        }

        public MemoryStream Synthesize(string input)
        {
            MemoryStream stream = new MemoryStream();
            SpeechAudioFormatInfo audioInfo = new SpeechAudioFormatInfo(_frequency, AudioBitsPerSample.Sixteen, AudioChannel.Mono);
            //_speechSynthesizer.SetOutputToWaveFile(Path.Combine(Path.GetTempPath(), "en" + (object)this.cnt + ".wav"), _frequency);
            _speechSynthesizer.SetOutputToAudioStream(stream, audioInfo);
            _speechSynthesizer.Speak(input);
            Dispose();
            return stream;
        }
    }
}
