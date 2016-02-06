using System;

using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructureString : IWordStructureRule
    {
        string _structureString;
        string[] _pronunciationStrings;

        public WordStructureString(string structureString, string[] pronunciationStrings)
        {
            if (pronunciationStrings.Length < 1)
                throw new ArgumentException("pronunciationStrings's length is less than 1", "pronunciationStrings");
            _structureString = structureString;
            _pronunciationStrings = pronunciationStrings;
        }
        public WordStructureString(string formatString)
        {
            string[] strArray = formatString.Split(',');
            if (strArray.Length == 1)
            {
                _structureString = strArray[0];
                _pronunciationStrings = new string[] { strArray[0] };
            } else if (strArray.Length == 2)
            {
                _structureString = strArray[0];
                _pronunciationStrings = strArray[1].Split(' ');
            } else
                throw new ArgumentException("String format is invalid.", "formatString");
        }

        public Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation)
        {
            Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>();
            if (word.Length >= _structureString.Length && word.Substring(0, _structureString.Length) == _structureString)
            {
                string[] newPronunciation = new string[prePronunciation.Length + _pronunciationStrings.Length - 1];
                prePronunciation.CopyTo(newPronunciation, 0);
                newPronunciation[prePronunciation.Length - 1] += _pronunciationStrings[0];
                for (int count = 1; count < _pronunciationStrings.Length; count++)
                    newPronunciation[prePronunciation.Length - 1 + count] = _pronunciationStrings[count];
                matchedPrefix.Add(_structureString, newPronunciation);
            }
            return matchedPrefix;
        }
    }
}
