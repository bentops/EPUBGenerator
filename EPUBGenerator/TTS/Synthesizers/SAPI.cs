using System;
using System.Collections.Generic;
using System.IO;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;

namespace TTS.Synthesizers
{
    class SAPI : ISynthesizer
    {
        private SpeechSynthesizer speechSynthesizer;
        private SpeechAudioFormatInfo speechAudioFormatInfo;
        private string tempPath;

        public SAPI()
        {
            int frequency = 16000;
            speechSynthesizer = new SpeechSynthesizer();
            speechSynthesizer.Volume = 100;

            speechAudioFormatInfo = new SpeechAudioFormatInfo(frequency, AudioBitsPerSample.Sixteen, AudioChannel.Mono);
        }

        public string About()
        {
            return "Microsoft SAPI";
        }

        public void Dispose()
        {
            speechSynthesizer.Dispose();
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
            speechSynthesizer.Rate = (int)(4 * speed - 4);
        }

        public void SetTemp(string path)
        {
            tempPath = path;
        }

        public void Synthesize(string input, string id, string outputPath)
        {
            string wavPath = Path.Combine(outputPath, id + ".wav");
            speechSynthesizer.SetOutputToWaveFile(wavPath, speechAudioFormatInfo);
            speechSynthesizer.Speak(input);
            Dispose();
        }
    }
}
