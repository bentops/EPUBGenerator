using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic.SoundEngine
{
    public class PositionChangedEventArgs : EventArgs
    {
        private long _change;
        private long _position;

        public long Change { get { return _change; } }
        public long ByteChange { get { return _change * 2; } }
        public long Position { get { return _position; } }
        public long BytePosition { get { return _position * 2; } }
        public bool IsEnded { get; private set; }

        public PositionChangedEventArgs(long change, long position, bool end)
        {
            _change = change;
            _position = position;
            IsEnded = end;
        }
    }
}
