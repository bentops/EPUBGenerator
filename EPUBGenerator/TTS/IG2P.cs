using System.Collections.Generic;

namespace TTS
{
    public interface IG2P
    {
        string GenTranscript(string input);
        List<KeyValuePair<string, string>> GenTranscriptList(string input);
        List<List<string>> GenPronunciationAndTranscriptList(string input);
    }
}
