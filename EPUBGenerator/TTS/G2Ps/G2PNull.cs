using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTS.G2Ps
{
    class G2PNull : IG2P
    {
        public List<List<string>> GenPronunciationAndTranscriptList(string input)
        {
            return new List<List<string>>() { new List<string>() { input, input, GenTranscript(input) } };
        }

        public string GenTranscript(string input)
        {
            return input;
        }

        public List<KeyValuePair<string, string>> GenTranscriptList(string input)
        {
            return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(input, GenTranscript(input)) };
        }
    }
}
