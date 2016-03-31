using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic.SoundEngine
{
    public class PositionChangedEventArgs : EventArgs
    {
        private double _change;
        private double _position;

        public double Change { get { return _change; } }
        public double ByteChange { get { return _change * 2; } }
        public double Position { get { return _position; } }
        public double BytePosition { get { return _position * 2; } }
        public bool IsEnded { get; private set; }

        public PositionChangedEventArgs(double change, double position, bool end)
        {
            _change = change;
            _position = position;
            IsEnded = end;
        }
    }
}
