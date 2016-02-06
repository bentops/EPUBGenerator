using System.Collections.Generic;

namespace TTS.G2Ps
{
    public class G2PEn : IG2P
    {
        public string GenTranscript(string inp)
        {
            return inp;
        }
        
        public List<KeyValuePair<string, string>> GenTranscriptList(string inp)
        {
            return new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>(inp, inp) };
        }

        public List<List<string>> GenPronunciationAndTranscriptList(string inp)
        {
            return new List<List<string>>() { new List<string>() { inp, inp, inp } };
        }
    }
}
