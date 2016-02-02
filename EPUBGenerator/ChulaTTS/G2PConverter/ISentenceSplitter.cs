using System.Collections.Generic;

namespace ChulaTTS.G2PConverter
{
    public interface ISentenceSplitter
    {
        List<KeyValuePair<string, int>> Split(string input);
    }
}
