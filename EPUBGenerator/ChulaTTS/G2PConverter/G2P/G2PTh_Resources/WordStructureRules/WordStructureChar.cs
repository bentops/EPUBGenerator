using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructureChar : IWordStructureRule
    {
        public static readonly WordStructureChar Cons = new WordStructureChar
            (
            '¡', '¢', "£¢", '¤', "¥¤", "¦¤", '§', '¨', '©', 'ª', '«',
            "¬ª", "­Â", "®´", "¯µ", "°¶", "±·", "²·", "³¹", '´', 'µ', '¶',
            '·', "¸·", '¹', 'º', '»', '¼', '½', '¾', '¿', "À¾", 'Á',
            'Â', 'Ã', 'Å', 'Ç', "ÈÊ", "ÉÊ", 'Ê', 'Ë', "ÌÅ", 'Í', 'Î'
            );
        public static readonly WordStructureChar HighCons = new WordStructureChar
            (
            '¼', '½', '¶', "°¶", '¢', "£¢", "ÈÊ", "ÉÊ", "Ê", "Ë", "©"
            );
        public static readonly WordStructureChar MiddleCons = new WordStructureChar
            (
            '¡', '¨', '´', 'µ', "®´", "¯µ", 'º', '»', 'Í'
            );
        public static readonly WordStructureChar LowAloneCons = new WordStructureChar
            (
            '§', "­Â", "³¹", '¹', 'Á', 'Â', 'Ã', 'Å', 'Ç', "ÌÅ"
            );
        public static readonly WordStructureChar FCons = new WordStructureChar
            (
            '¡', "¤¡", "¦¡",
            '´', "¨´", "©´", "ª´", "«´", "¬´", "®´", "¯´", "±´", "²´", "µ´", "·´", "¸´", "È´", "É´", "Ê´",
            'º', "»º", "¾º", "¿º", "Àº",
            '¹', "­¹", "³¹", "Ã¹", "Å¹", "Ì¹",
            'Á',
            'Â',
            'Ç',
            '§'
            );
        public static readonly WordStructureChar DeadFCons = new WordStructureChar
            (
            '¡', "¤¡", "¦¡", "¢¡",
            '´', "¨´", "©´", "ª´", "«´", "¬´", "®´", "¯´", "±´", "²´", "µ´", "·´", "¸´", "È´", "É´", "Ê´", "¶´",
            'º', "»º", "¾º", "¿º", "Àº"
            );
        public static readonly WordStructureChar FConsNoYorYak = WordStructureChar.FCons - 'Â';
        public static readonly WordStructureChar FConsNoRorRuer = WordStructureChar.FCons - 'Ã';
        public static readonly WordStructureChar FConsNoYorYakRorRuer = WordStructureChar.FCons - 'Â' - 'Ã';
        public static readonly WordStructureChar RorHunFCons = new WordStructureChar
            (
            "³¹", 'Á', '¡', "É´", "¤¡", "¦¡", "ª´", "¨´", "¶´", "»º", "¾º", "¸´", "Àº", "È´"
            );
        public static readonly WordStructureChar USara = new WordStructureChar
            (
            "ÍÖ"[1],
            "ÍÑ"[1],
            "Í×"[1],
            "ÍÔ"[1],
            "ÍÕ"[1]
            );
        public static readonly WordStructureChar DSara = new WordStructureChar
            (
            "ÍØ"[1],
            "ÍÙ"[1]
            );
        public static readonly WordStructureChar RSara = new WordStructureChar
            (
            'Ð', 'Ò'
            );
        public static readonly WordStructureChar Tone = new WordStructureChar
            (
            "Íè"[1],
            "Íé"[1],
            "Íê"[1],
            "Íë"[1]
            );

        public static WordStructureChar FromChar(char c)
        {
            return new WordStructureChar(c);
        }
        public static WordStructureChar operator -(WordStructureChar x, char ch)
        {
            WordStructureChar wordStructureChar = new WordStructureChar(x);
            wordStructureChar._acceptedCharPronunciationDict.Remove(ch);
            return wordStructureChar;
        }

        Dictionary<char, string> _acceptedCharPronunciationDict = new Dictionary<char, string>();

        public string this[char index] { get { return _acceptedCharPronunciationDict[index]; } }

        public WordStructureChar(params object[] objects)
        {
            foreach (object obj in objects)
            {
                if (obj is char)
                    _acceptedCharPronunciationDict.Add((char)obj, "" + (char)obj);
                else if (obj is string)
                {
                    string s = (string)obj;
                    if (s.Length >= 1)
                        _acceptedCharPronunciationDict.Add((char)s[0], s.Substring(1));
                    else
                        throw new ArgumentException("String format is invalid.");
                } else
                    throw new ArgumentException("Mismatched type of arguments.");
            }
        }
        public WordStructureChar(WordStructureChar that)
        {
            _acceptedCharPronunciationDict = new Dictionary<char, string>(that._acceptedCharPronunciationDict);
        }

        public Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation)
        {
            Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>();
            if (word.Length > 0 && _acceptedCharPronunciationDict.ContainsKey(word[0]))
            {
                string[] newPronunciation = (string[])prePronunciation.Clone();
                newPronunciation[newPronunciation.Length - 1] += _acceptedCharPronunciationDict[word[0]];
                matchedPrefix.Add(word.Substring(0, 1), newPronunciation);
            }
            return matchedPrefix;
        }

        public bool IsCharMatched(char ch)
        {
            return _acceptedCharPronunciationDict.ContainsKey(ch);
        }

        public WordStructureChar RemovePronunciationDict()
        {
            WordStructureChar wordStructureChar = new WordStructureChar(this);
            foreach (char key in _acceptedCharPronunciationDict.Keys)
                wordStructureChar._acceptedCharPronunciationDict[key] = "";
            return wordStructureChar;
        }
    }
}
