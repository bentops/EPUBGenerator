using System.Collections.Generic;

namespace ChulaTTS.G2PConverter
{
    public interface IG2P
    {
        string GenTranscript(string input, int type);
        List<KeyValuePair<string, string>> GenTranscriptList(string input, int type);
        List<List<string>> GenPronunciationAndTranscriptList(string input, int type);
    }
}
