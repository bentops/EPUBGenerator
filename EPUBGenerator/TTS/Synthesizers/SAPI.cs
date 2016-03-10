using System;
using System.Collections.Generic;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;

namespace TTS.Synthesizers
{
    class SAPI : ISynthesizer
    {
        private int speechRate;
        private SpeechAudioFormatInfo speechAudioFormatInfo;
        private string tempPath;

        public SAPI()
        {
            int frequency = 16000;
            speechRate = 0;
            speechAudioFormatInfo = new SpeechAudioFormatInfo(frequency, AudioBitsPerSample.Sixteen, AudioChannel.Mono);
        }

        public string About()
        {
            return "Microsoft SAPI";
        }

        public void Dispose()
        {
        }

        public List<string> GetModel()
        {
            return new List<string>() { "default" };
        }

        public void SetFrequency(int frequency)
        {
            speechAudioFormatInfo = new SpeechAudioFormatInfo(frequency, AudioBitsPerSample.Sixteen, AudioChannel.Mono);
        }

        public void SetModel(string modelName)
        {
        }

        public void SetPitch(double pitch)
        {
        }

        public void SetSpeed(double speed)
        {
            speechRate = (int)(4 * speed - 4);
        }

        public void SetTemp(string path)
        {
            tempPath = path;
        }

        public void Synthesize(string input, string id, string outputPath)
        {
            using (SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer())
            {
                string wavPath = Path.Combine(outputPath, id + ".wav");
                speechSynthesizer.Volume = 100;
                speechSynthesizer.Rate = speechRate;
                speechSynthesizer.SetOutputToWaveFile(wavPath, speechAudioFormatInfo);
                speechSynthesizer.Speak(input);
            }

        }
    }
}
