using System.Collections.Generic;

namespace ChulaTTS.G2PConverter.G2P
{
    public class G2PEn
    {
        public string GenTranscript(string inp)
        {
            return inp;
        }

        public List<List<string>> GenTranscripts(string inp)
        {
            return new List<List<string>>() { new List<string>() { inp, inp, inp } };
        }
    }
}
