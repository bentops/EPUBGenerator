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
            string[] fCons1 = new string[] { "�", "�", "�", "�", "�", "�", "�", "�" };
            string[] fCons2 = new string[] { "�", "�", "�", "�", "�", "�", "�", "�", "�" };
            WordStructureUnionRule allRules = new WordStructureUnionRule
                (
                "��͹,k-@@-n^-1", "��͹,z-@@-n^-1", "����,z-@@-m^-2", "��ʹ,z-@@-t^-3",
                "���,k-aa-w^-2", "���,c-aa-w^-2", "����,kl-aa-w^-2", "���,th-aa-w^-3",
                new WordStructurePronunciationRule("-�", "a", false, noFCons),
                new WordStructurePronunciationRule("-" + "��"[1], "a", false, new string[] { "�", "�", "�", "�", "�", "�", "�" }),
                new WordStructurePronunciationRule("-�", "aa", true),
                new WordStructurePronunciationRule("-" + "��"[1], "i", false),
                new WordStructurePronunciationRule("-" + "��"[1], "ii", true),
                new WordStructurePronunciationRule("-" + "��"[1], "v", false),
                new WordStructurePronunciationRule("-" + "��"[1], "vv", true, fCons2),
                new WordStructurePronunciationRule("-" + "��"[1], "u", false),
                new WordStructurePronunciationRule("-" + "��"[1], "uu", true),
                new WordStructurePronunciationRule("�-�", "e", false, noFCons),
                new WordStructurePronunciationRule("�-" + "��"[1], "e", false, fCons1),
                new WordStructurePronunciationRule("�-", "e", false, fCons1, new bool[] { false, true, false, true, true }),
                new WordStructurePronunciationRule("�-", "ee", true),
                new WordStructurePronunciationRule("�-�", "x", false, noFCons),
                new WordStructurePronunciationRule("�-" + "��"[1], "x", false, fCons1),
                new WordStructurePronunciationRule("�-", "x", false, fCons1, new bool[] { false, true, false, true, true }),
                new WordStructurePronunciationRule("�-", "xx", true),
                new WordStructurePronunciationRule("�-�", "o", false, noFCons),
                new WordStructurePronunciationRule("-", "o", false, fCons1),
                new WordStructurePronunciationRule("�-", "oo", true),
                new WordStructurePronunciationRule("�-��", "@", false, noFCons),
                new WordStructurePronunciationRule("-" + "��"[1] + "�", "@", false, fCons1),
                new WordStructurePronunciationRule("-�", "@", false, fCons1, new bool[] { false, true, false, true, true }),
                new WordStructurePronunciationRule("-�", "@", false, new bool[] { true, false, false }, new string[] { "�", "�", "�", "�" }, new bool[] { false, false, true, false, false }),
                new WordStructurePronunciationRule("-�", "@", false, new bool[] { false, true, false }, fCons1, new bool[] { false, false, true, false, false }),
                new WordStructurePronunciationRule("-�", "@@", true),
                new WordStructurePronunciationRule("�-��", "q", false, noFCons),
                new WordStructurePronunciationRule("�-" + "��"[1], "q", false, fCons1, new bool[] { false, true, false, true, true }),
                new WordStructurePronunciationRule("�-�", "qq", true, noFCons),
                new WordStructurePronunciationRule("�-" + "��"[1], "qq", true, fCons1),
                new WordStructurePronunciationRule("�-" + "��"[1] + "��", "ia", false, noFCons),
                new WordStructurePronunciationRule("�-" + "��"[1] + "�", "iia", true),
                new WordStructurePronunciationRule("�-" + "��"[1] + "��", "va", false, noFCons),
                new WordStructurePronunciationRule("�-" + "��"[1] + "�", "vva", true),
                new WordStructurePronunciationRule("-" + "��"[1] + "��", "ua", false, noFCons),
                new WordStructurePronunciationRule("-" + "��"[1] + "�", "uua", true, noFCons),
                new WordStructurePronunciationRule("-�", "uua", true, new string[] { "�", "�", "�", "�", "�", "�", "�" }),
                new WordStructurePronunciationRule("�-�", "a", true, noFCons, "�")
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
