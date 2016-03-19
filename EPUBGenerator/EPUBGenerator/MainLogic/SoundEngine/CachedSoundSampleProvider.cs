using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic.SoundEngine
{
    public delegate void SoundEndedEventHandler(object sender, EventArgs e);
    public delegate void PositionChangedEventHandler(object sender, PositionChangedEventArgs e);

    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;

        public long Position { get; private set; }
        public long BeginPosition { get; private set; }
        public long EndPosition { get; private set; }

        public event SoundEndedEventHandler SoundEnded;
        public event PositionChangedEventHandler PositionChanged;

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }

        public CachedSoundSampleProvider(CachedSound sound, long begin, long end)
        {
            cachedSound = sound;
            BeginPosition = begin;
            EndPosition = end;
            Position = begin;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            long availableSamples = EndPosition - Position;
            long samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, Position, buffer, offset, samplesToCopy);
            ChangePosition(samplesToCopy);
            return (int)samplesToCopy;
        }

        public void Stop()
        {
            Position = EndPosition;
            EndSound();
        }

        private void EndSound()
        {
            if (SoundEnded != null)
                SoundEnded(this, EventArgs.Empty);
        }

        private void ChangePosition(long change)
        {
            long oldPos = Position;
            long newPos = Position + change;
            Position = newPos;
            if (PositionChanged != null)
                PositionChanged(this, new PositionChangedEventArgs(newPos - oldPos, newPos, newPos >= EndPosition));
            if (change == 0)
                EndSound();
        }
    }
}
