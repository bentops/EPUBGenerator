using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructureTwoChar : IWordStructureRule
    {
        static WordStructureTwoChar SingletonInstance = null;

        public static WordStructureTwoChar Instance
        {
            get
            {
                if (SingletonInstance == null)
                    SingletonInstance = new WordStructureTwoChar();
                return SingletonInstance;
            }
        }

        Dictionary<string, bool> _twoCharDict = new Dictionary<string, bool>();

        private WordStructureTwoChar()
        {
            foreach (string s in TTS.Properties.Resources.TwoConsDict.Split(' '))
                _twoCharDict.Add(s, true);
        }

        public Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation)
        {
            Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>();
            if (word.Length >= 2 && _twoCharDict.ContainsKey(word.Substring(0, 2)))
            {
                string[] newPronunciation = new string[prePronunciation.Length + 1];
                Array.Copy(prePronunciation, 0, newPronunciation, 1, prePronunciation.Length);
                newPronunciation[0] = WordStructureChar.Cons[word[0]] + "ะ";
                {
                    bool isHorHeubRequired = false;
                    if (WordStructureChar.LowAloneCons.IsCharMatched(word[1]))
                    {
                        if (WordStructureChar.HighCons.IsCharMatched(word[0]))
                            isHorHeubRequired = true;
                        else if (WordStructureChar.MiddleCons.IsCharMatched(word[0]))
                        {
                            if (word.Length >= 3
                                && (WordStructureChar.Tone.IsCharMatched(word[2]) || WordStructureChar.DeadFCons.IsCharMatched(word[2]))
                                || word.Length >= 4 && !WordStructureChar.Cons.IsCharMatched(word[2])
                                && (WordStructureChar.Tone.IsCharMatched(word[3]) || WordStructureChar.DeadFCons.IsCharMatched(word[3])))
                                isHorHeubRequired = true;
                        }
                    }
                    if (isHorHeubRequired)
                        newPronunciation[newPronunciation.Length - 1] += "หฺ";
                }
                newPronunciation[newPronunciation.Length - 1] += WordStructureChar.Cons[word[1]];
                matchedPrefix.Add(word.Substring(0, 2), newPronunciation);
            }
            return matchedPrefix;
        }
    }
}
