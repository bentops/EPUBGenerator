using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructureUnionRule : WordStructureRuleList, IWordStructureRule
    {
        public WordStructureUnionRule(params object[] objects)
            : base(objects) { }

        public Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation)
        {
            Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>();
            foreach (IWordStructureRule wordStructureRule in this)
            {
                Dictionary<string, string[]> subMatchedPrefix = wordStructureRule.GetMatchedPrefix(word, prePronunciation);
                if (subMatchedPrefix != null)
                {
                    foreach (string key in subMatchedPrefix.Keys)
                        if (!matchedPrefix.ContainsKey(key))
                            matchedPrefix.Add(key, subMatchedPrefix[key]);
                }
            }
            return matchedPrefix;
        }
    }
}
