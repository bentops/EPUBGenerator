using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    interface IWordStructureRule
    {
        Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation);
    }
}
