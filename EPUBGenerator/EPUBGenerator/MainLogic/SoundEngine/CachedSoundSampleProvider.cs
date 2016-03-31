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

        public double Speed { get; set; }
        public double Position { get; private set; }
        public long BeginPosition { get; private set; }
        public long EndPosition { get; private set; }

        public event SoundEndedEventHandler SoundEnded;
        public event PositionChangedEventHandler PositionChanged;

        public WaveFormat WaveFormat { get { return cachedSound.WaveFormat; } }

        public CachedSoundSampleProvider(CachedSound sound, long begin, long end, double speed)
        {
            Speed = speed;
            cachedSound = sound;
            BeginPosition = begin;
            EndPosition = end;
            Position = begin;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            //long availableSamples = EndPosition - Position;
            //long samplesToCopy = Math.Min(availableSamples, count);
            //Array.Copy(cachedSound.AudioData, Position, buffer, offset, samplesToCopy);
            double curPos = Position;
            int i = 0;
            for (; curPos < EndPosition && i < count; i++)
            {
                long lb = (long)Math.Floor(curPos);
                long ub = (long)Math.Ceiling(curPos);
                double mixed = cachedSound.AudioData[lb];
                if (lb < ub && ub < EndPosition)
                    mixed = cachedSound.AudioData[lb] * (ub - curPos) + cachedSound.AudioData[ub] * (curPos - lb);
                buffer[offset + i] = (float)mixed;
                curPos += Speed;
            }
            double change = curPos - Position;
            //Position = curPos;
            ChangePosition(change);
            return i;
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

        private void ChangePosition(double change)
        {
            double oldPos = Position;
            double newPos = Position + change;
            Position = newPos;
            if (PositionChanged != null)
                PositionChanged(this, new PositionChangedEventArgs(newPos - oldPos, newPos, newPos >= EndPosition));
            if (change == 0)
                EndSound();
        }
    }
}
