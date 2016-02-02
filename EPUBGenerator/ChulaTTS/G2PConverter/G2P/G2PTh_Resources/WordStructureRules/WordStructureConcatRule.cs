using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructureConcatRule : WordStructureRuleList, IWordStructureRule
    {
        public WordStructureConcatRule(params object[] objects)
            : base(objects) { }

        //public string[] GetMatchedPrefix(string word)
        //{
        //    string[] possibleWords = new string[1] { word };
        //    foreach (IWordStructureRule wordStructureRule in _wordStructureRuleList)
        //    {
        //        List<string> nextPossibleWordList = new List<string>();
        //        foreach (string checkingWord in possibleWords)
        //        {
        //            string[] matchedPrefix = wordStructureRule.GetMatchedPrefix(checkingWord);
        //            if (matchedPrefix != null && matchedPrefix.Length > 0)
        //            {
        //                foreach (string prefix in matchedPrefix)
        //                    nextPossibleWordList.Add(checkingWord.Substring(prefix.Length, checkingWord.Length - prefix.Length));
        //            }
        //        }
        //        if (nextPossibleWordList.Count <= 0)
        //            return null;
        //        possibleWords = nextPossibleWordList.ToArray();
        //    }
        //    List<string> resultMatchedPrefixList = new List<string>();
        //    foreach (string checkingWord in possibleWords)
        //        resultMatchedPrefixList.Add(word.Substring(0, word.Length - checkingWord.Length));
        //    return resultMatchedPrefixList.ToArray();
        //}

        //public bool IsPrefixMatched(string word, string[] prePronunciation, out Dictionary<string, string[]> matchedResult)
        //{
        //    string[] possibleWords = new string[1] { word };
        //    foreach (IWordStructureRule wordStructureRule in _wordStructureRuleList)
        //    {
        //        List<string> nextPossibleWordList = new List<string>();
        //        foreach (string checkingWord in possibleWords)
        //        {
        //            string[] matchedPrefix = wordStructureRule.GetMatchedPrefix(checkingWord);
        //            if (matchedPrefix != null && matchedPrefix.Length > 0)
        //            {
        //                foreach (string prefix in matchedPrefix)
        //                    nextPossibleWordList.Add(checkingWord.Substring(prefix.Length, checkingWord.Length - prefix.Length));
        //            }
        //        }
        //        if (nextPossibleWordList.Count <= 0)
        //            return null;
        //        possibleWords = nextPossibleWordList.ToArray();
        //    }
        //    List<string> resultMatchedPrefixList = new List<string>();
        //    foreach (string checkingWord in possibleWords)
        //        resultMatchedPrefixList.Add(word.Substring(0, word.Length - checkingWord.Length));
        //    return resultMatchedPrefixList.ToArray();
        //}

        public Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation)
        {
            Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>();
            matchedPrefix.Add("", prePronunciation);
            foreach (IWordStructureRule wordStructureRule in this)
            {
                Dictionary<string, string[]> nextMatchedPrefix = new Dictionary<string, string[]>();
                foreach (string prefix in matchedPrefix.Keys)
                {
                    Dictionary<string, string[]> subMatchedPrefix = wordStructureRule.GetMatchedPrefix(word.Remove(0, prefix.Length), matchedPrefix[prefix]);
                    if (subMatchedPrefix != null)
                    {
                        foreach (string key in subMatchedPrefix.Keys)
                            if (!nextMatchedPrefix.ContainsKey(prefix + key))
                                nextMatchedPrefix.Add(prefix + key, subMatchedPrefix[key]);
                    }
                }
                if (nextMatchedPrefix.Count <= 0)
                    return new Dictionary<string, string[]>();
                matchedPrefix = nextMatchedPrefix;
            }
            return matchedPrefix;
        }
    }
}
