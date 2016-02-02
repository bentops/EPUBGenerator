using System;
using System.Collections.Generic;
using System.Text;

namespace Chula.SLS.TTS.C2SSegmentator
{
    [global::System.Serializable]
    public class C2SSegmentatorException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public C2SSegmentatorException() { }
        public C2SSegmentatorException(string message) : base(message) { }
        public C2SSegmentatorException(string message, Exception inner) : base(message, inner) { }
        protected C2SSegmentatorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}