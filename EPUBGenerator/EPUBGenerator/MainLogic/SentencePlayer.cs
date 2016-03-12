using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    class SentencePlayer
    {
        public Sentence Sentence { get; private set; }
        public LinkedList<RunWord> RunWords { get; private set; }

        private MemoryStream MemoryStream;
        private WaveStream WaveStream;
        public IWavePlayer Player { get; set; }

        public SentencePlayer(Sentence sentence)
        {
            Sentence = sentence;
            using (FileStream fileStream = File.OpenRead(Sentence.WavPath))
            {
                fileStream.CopyTo(MemoryStream);
                fileStream.Close();
            }
            WaveStream = new WaveFileReader(MemoryStream);
            Player = new WaveOut(WaveCallbackInfo.FunctionCallback());
            Player.Init(WaveStream);
        }

        public void Play(long begin)
        {
            WaveStream.Position = begin;
        }
    }
}
