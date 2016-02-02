using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    public class ThaiPhonemeConverter
    {
        private static ThaiPhonemeConverter SingletonInstance = null;

        public static ThaiPhonemeConverter Instance
        {
            get
            {
                if (SingletonInstance == null)
                    SingletonInstance = new ThaiPhonemeConverter();
                return ThaiPhonemeConverter.SingletonInstance;
            }
        }

        IWordStructureRule _thaiPronunciationStructureRule;
        bool _isInitialized = false;

        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set { _isInitialized = value; }
        }

        private ThaiPhonemeConverter() { }

        public void InitializePronunciationRules()
        {
            string[] noFCons = new string[] { "" };
            string[] fCons1 = new string[] { "น", "ม", "ย", "ว", "ง", "ก", "ด", "บ" };
            string[] fCons2 = new string[] { "น", "ม", "ย", "ว", "ง", "ก", "ด", "บ", "อ" };
            WordStructureUnionRule allRules = new WordStructureUnionRule
                (
                "ก่อน,k-@@-n^-1", "อ่อน,z-@@-n^-1", "อ้อม,z-@@-m^-2", "อ๊อด,z-@@-t^-3",
                "เก้า,k-aa-w^-2", "เจ้า,c-aa-w^-2", "เกล้า,kl-aa-w^-2", "เท้า,th-aa-w^-3",
                new WordStructurePronunciationRule("-ะ", "a", false, noFCons),
                new WordStructurePronunciationRule("-" + "อั"[1], "a", false, new string[] { "น", "ม", "ย", "ง", "ก", "ด", "บ" }),
                new WordStructurePronunciationRule("-า", "aa", true),
                new WordStructurePronunciationRule("-" + "อิ"[1], "i", false),
                new WordStructurePronunciationRule("-" + "อี"[1], "ii", true),
                new WordStructurePronunciationRule("-" + "อึ"[1], "v", false),
                new WordStructurePronunciationRule("-" + "อื"[1], "vv", true, fCons2),
                new WordStructurePronunciationRule("-" + "อุ"[1], "u", false),
                new WordStructurePronunciationRule("-" + "อู"[1], "uu", true),
                new WordStructurePronunciationRule("เ-ะ", "e", false, noFCons),
                new WordStructurePronunciationRule("เ-" + "อ็"[1], "e", false, fCons1),
                new WordStructurePronunciationRule("เ-", "e", false, fCons1, new bool[] { false, true, false, true, true }),
                new WordStructurePronunciationRule("เ-", "ee", true),
                new WordStructurePronunciationRule("แ-ะ", "x", false, noFCons),
                new WordStructurePronunciationRule("แ-" + "อ็"[1], "x", false, fCons1),
                new WordStructurePronunciationRule("แ-", "x", false, fCons1, new bool[] { false, true, false, true, true }),
                new WordStructurePronunciationRule("แ-", "xx", true),
                new WordStructurePronunciationRule("โ-ะ", "o", false, noFCons),
                new WordStructurePronunciationRule("-", "o", false, fCons1),
                new WordStructurePronunciationRule("โ-", "oo", true),
                new WordStructurePronunciationRule("เ-าะ", "@", false, noFCons),
                new WordStructurePronunciationRule("-" + "อ็"[1] + "อ", "@", false, fCons1),
                new WordStructurePronunciationRule("-อ", "@", false, fCons1, new bool[] { false, true, false, true, true }),
                new WordStructurePronunciationRule("-อ", "@", false, new bool[] { true, false, false }, new string[] { "ม", "ย", "ว", "ง" }, new bool[] { false, false, true, false, false }),
                new WordStructurePronunciationRule("-อ", "@", false, new bool[] { false, true, false }, fCons1, new bool[] { false, false, true, false, false }),
                new WordStructurePronunciationRule("-อ", "@@", true),
                new WordStructurePronunciationRule("เ-อะ", "q", false, noFCons),
                new WordStructurePronunciationRule("เ-" + "อิ"[1], "q", false, fCons1, new bool[] { false, true, false, true, true }),
                new WordStructurePronunciationRule("เ-อ", "qq", true, noFCons),
                new WordStructurePronunciationRule("เ-" + "อิ"[1], "qq", true, fCons1),
                new WordStructurePronunciationRule("เ-" + "อี"[1] + "ยะ", "ia", false, noFCons),
                new WordStructurePronunciationRule("เ-" + "อี"[1] + "ย", "iia", true),
                new WordStructurePronunciationRule("เ-" + "อื"[1] + "อะ", "va", false, noFCons),
                new WordStructurePronunciationRule("เ-" + "อื"[1] + "อ", "vva", true),
                new WordStructurePronunciationRule("-" + "อั"[1] + "วะ", "ua", false, noFCons),
                new WordStructurePronunciationRule("-" + "อั"[1] + "ว", "uua", true, noFCons),
                new WordStructurePronunciationRule("-ว", "uua", true, new string[] { "น", "ม", "ย", "ง", "ก", "ด", "บ" }),
                new WordStructurePronunciationRule("เ-า", "a", true, noFCons, "ว")
                );
            _thaiPronunciationStructureRule = allRules;

            this.IsInitialized = true;
        }

        public string ToThaiPhoneme(string thaiPronunciation)
        {
            return this.ToThaiPhoneme(new string[] { thaiPronunciation })[0];
        }
        public string[] ToThaiPhoneme(string[] thaiPronunciations)
        {
            if (!this.IsInitialized)
                this.InitializePronunciationRules();

            string[] thaiPhonemes = new string[thaiPronunciations.Length];
            for (int count = 0; count < thaiPronunciations.Length; count++)
            {
                string s = thaiPronunciations[count];
                Dictionary<string, string[]> matchedPrefix = _thaiPronunciationStructureRule.GetMatchedPrefix(s, new string[] { "" });
                thaiPhonemes[count] = matchedPrefix.ContainsKey(s) ? matchedPrefix[s][0] : "$-$-$-$";
            }
            return thaiPhonemes;
        }
    }
}
