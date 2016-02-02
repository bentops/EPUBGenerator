using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructureOptionalRule : IWordStructureRule
    {
        public static readonly WordStructureOptionalRule Cons = new WordStructureOptionalRule(WordStructureChar.Cons);
        public static readonly WordStructureOptionalRule FCons = new WordStructureOptionalRule(WordStructureChar.FCons);
        public static readonly WordStructureOptionalRule FConsNoYorYakRorRuer = new WordStructureOptionalRule(WordStructureChar.FConsNoYorYakRorRuer);
        public static readonly WordStructureOptionalRule FConsNoRorRuer = new WordStructureOptionalRule(WordStructureChar.FConsNoRorRuer);
        public static readonly WordStructureOptionalRule Tone = new WordStructureOptionalRule(WordStructureChar.Tone);

        IWordStructureRule _wordStructureRule;

        public WordStructureOptionalRule(IWordStructureRule wordStructureRule)
        {
            if (wordStructureRule == null)
                throw new ArgumentNullException("wordStructureRule");
            _wordStructureRule = wordStructureRule;
        }

        public Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation)
        {
            Dictionary<string, string[]> matchedPrefix = null;
            matchedPrefix = _wordStructureRule.GetMatchedPrefix(word, prePronunciation);
            if (matchedPrefix == null)
                matchedPrefix = new Dictionary<string, string[]>();
            matchedPrefix.Add("", prePronunciation);
            return matchedPrefix;
        }
    }
}
