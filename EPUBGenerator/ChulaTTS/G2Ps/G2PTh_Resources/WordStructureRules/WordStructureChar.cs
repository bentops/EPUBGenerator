using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructureChar : IWordStructureRule
    {
        public static readonly WordStructureChar Cons = new WordStructureChar
            (
            '�', '�', "��", '�', "��", "��", '�', '�', '�', '�', '�',
            "��", "��", "��", "��", "��", "��", "��", "��", '�', '�', '�',
            '�', "��", '�', '�', '�', '�', '�', '�', '�', "��", '�',
            '�', '�', '�', '�', "��", "��", '�', '�', "��", '�', '�'
            );
        public static readonly WordStructureChar HighCons = new WordStructureChar
            (
            '�', '�', '�', "��", '�', "��", "��", "��", "�", "�", "�"
            );
        public static readonly WordStructureChar MiddleCons = new WordStructureChar
            (
            '�', '�', '�', '�', "��", "��", '�', '�', '�'
            );
        public static readonly WordStructureChar LowAloneCons = new WordStructureChar
            (
            '�', "��", "��", '�', '�', '�', '�', '�', '�', "��"
            );
        public static readonly WordStructureChar FCons = new WordStructureChar
            (
            '�', "��", "��",
            '�', "��", "��", "��", "��", "��", "��", "��", "��", "��", "��", "��", "��", "ȴ", "ɴ", "ʴ",
            '�', "��", "��", "��", "��",
            '�', "��", "��", "ù", "Ź", "̹",
            '�',
            '�',
            '�',
            '�'
            );
        public static readonly WordStructureChar DeadFCons = new WordStructureChar
            (
            '�', "��", "��", "��",
            '�', "��", "��", "��", "��", "��", "��", "��", "��", "��", "��", "��", "��", "ȴ", "ɴ", "ʴ", "��",
            '�', "��", "��", "��", "��"
            );
        public static readonly WordStructureChar FConsNoYorYak = WordStructureChar.FCons - '�';
        public static readonly WordStructureChar FConsNoRorRuer = WordStructureChar.FCons - '�';
        public static readonly WordStructureChar FConsNoYorYakRorRuer = WordStructureChar.FCons - '�' - '�';
        public static readonly WordStructureChar RorHunFCons = new WordStructureChar
            (
            "��", '�', '�', "ɴ", "��", "��", "��", "��", "��", "��", "��", "��", "��", "ȴ"
            );
        public static readonly WordStructureChar USara = new WordStructureChar
            (
            "��"[1],
            "��"[1],
            "��"[1],
            "��"[1],
            "��"[1]
            );
        public static readonly WordStructureChar DSara = new WordStructureChar
            (
            "��"[1],
            "��"[1]
            );
        public static readonly WordStructureChar RSara = new WordStructureChar
            (
            '�', '�'
            );
        public static readonly WordStructureChar Tone = new WordStructureChar
            (
            "��"[1],
            "��"[1],
            "��"[1],
            "��"[1]
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
