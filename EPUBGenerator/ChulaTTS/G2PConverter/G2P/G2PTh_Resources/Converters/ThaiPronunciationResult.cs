using System;

using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    public class ThaiPronunciationResult
    {
        string[] _pronunciations;
        KeyValuePair<string, string[]>[] _segmentedWords;

        public string[] Pronunciations { get { return _pronunciations; } }
        public KeyValuePair<string, string[]>[] SegmentedWords { get { return _segmentedWords; } }

        public ThaiPronunciationResult(string[] pronunciations, KeyValuePair<string, string[]>[] segmentedWords)
        {
            _pronunciations = pronunciations;
            _segmentedWords = segmentedWords;
        }
    }
}
