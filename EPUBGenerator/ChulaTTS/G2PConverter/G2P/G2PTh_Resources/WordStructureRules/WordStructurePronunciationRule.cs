using System;

using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructurePronunciationRule : IWordStructureRule
    {
        class DictResource
        {
            static DictResource SingletonInstance;

            public static DictResource Instance
            {
                get
                {
                    if (SingletonInstance == null)
                        SingletonInstance = new DictResource();
                    return DictResource.SingletonInstance;
                }
            }

            Dictionary<string, string> _middleConsPhonemeDict = new Dictionary<string, string>();
            Dictionary<string, string> _highConsPhonemeDict = new Dictionary<string, string>();
            Dictionary<string, string> _lowConsPhonemeDict = new Dictionary<string, string>();
            Dictionary<string, string> _consPhonemeDict = new Dictionary<string, string>();

            Dictionary<string, string> _aliveFConsPhonemeDict = new Dictionary<string, string>();
            Dictionary<string, string> _deadFConsPhonemeDict = new Dictionary<string, string>();
            Dictionary<string, string> _fConsPhonemeDict = new Dictionary<string, string>();

            Dictionary<string, string> _toneDict = new Dictionary<string, string>();

            public Dictionary<string, string> MiddleConsPhonemeDict { get { return _middleConsPhonemeDict; } }
            public Dictionary<string, string> HighConsPhonemeDict { get { return _highConsPhonemeDict; } }
            public Dictionary<string, string> LowConsPhonemeDict { get { return _lowConsPhonemeDict; } }
            public Dictionary<string, string> ConsPhonemeDict { get { return _consPhonemeDict; } }

            public Dictionary<string, string> AliveFConsPhonemeDict { get { return _aliveFConsPhonemeDict; } }
            public Dictionary<string, string> DeadFConsPhonemeDict { get { return _deadFConsPhonemeDict; } }
            public Dictionary<string, string> FConsPhonemeDict { get { return _fConsPhonemeDict; } }

            public Dictionary<string, string> ToneDict { get { return _toneDict; } }

            private DictResource()
            {
                {
                    string[] strArray = new string[] 
                    {
                        "ก,k", "จ,c", "ด,d", "ต,t", "บ,b", "ป,p", "อ,z",
                        "กฺร,kr", "ตฺร,tr", "ปฺร,pr",
                        "กฺล,kl", "ปฺล,pl",
                        "กฺว,kw"
                    };
                    foreach (string s in strArray)
                    {
                        string[] splittedString = s.Split(',');
                        _middleConsPhonemeDict.Add(splittedString[0], splittedString[1]);
                    }
                }
                {
                    string[] strArray = new string[] 
                    {
                        "ข,kh", "ฉ,ch", "ถ,th", "ผ,ph", "ฝ,f", "ส,s", "ห,h",
                        "ขฺร,khr",
                        "ขฺล,khl",
                        "ขฺว,khw",
                        "หฺง,ng", "หฺน,n", "หฺม,m", "หฺย,j", "หฺร,r", "หฺล,l", "หฺว,w"
                    };
                    foreach (string s in strArray)
                    {
                        string[] splittedString = s.Split(',');
                        _highConsPhonemeDict.Add(splittedString[0], splittedString[1]);
                    }
                }
                {
                    string[] strArray = new string[] 
                    {
                        "ค,kh", "ง,ng", "ช,ch", "ซ,s", "ท,th", "น,n", "พ,ph",
                        "ม,m", "ย,j", "ร,r", "ล,l", "ว,w", "ฟ,f", "ฮ,h",
                        "คฺร,khr", "ทฺร,thr", "พฺร,phr", "ฟฺร,fr",
                        "คฺล,khl", "พฺล,phl", "ฟฺล,fl",
                        "คฺว,khw"
                    };
                    foreach (string s in strArray)
                    {
                        string[] splittedString = s.Split(',');
                        _lowConsPhonemeDict.Add(splittedString[0], splittedString[1]);
                    }
                }
                {
                    foreach (KeyValuePair<string, string> v in _middleConsPhonemeDict)
                        _consPhonemeDict.Add(v.Key, v.Value);
                    foreach (KeyValuePair<string, string> v in _highConsPhonemeDict)
                        _consPhonemeDict.Add(v.Key, v.Value);
                    foreach (KeyValuePair<string, string> v in _lowConsPhonemeDict)
                        _consPhonemeDict.Add(v.Key, v.Value);
                }

                {
                    string[] strArray = new string[]
                    {
                        "น,n^", "ม,m^", "ย,j^", "ว,w^", "ง,ng^", "อ,z^", ",z^"
                    };
                    foreach (string s in strArray)
                    {
                        string[] splittedString = s.Split(',');
                        _aliveFConsPhonemeDict.Add(splittedString[0], splittedString[1]);
                    }
                }
                {
                    string[] strArray = new string[]
                    {
                        "ก,k^", "ด,t^", "บ,p^"
                    };
                    foreach (string s in strArray)
                    {
                        string[] splittedString = s.Split(',');
                        _deadFConsPhonemeDict.Add(splittedString[0], splittedString[1]);
                    }
                }
                {
                    foreach (KeyValuePair<string, string> v in _aliveFConsPhonemeDict)
                        _fConsPhonemeDict.Add(v.Key, v.Value);
                    foreach (KeyValuePair<string, string> v in _deadFConsPhonemeDict)
                        _fConsPhonemeDict.Add(v.Key, v.Value);
                }

                {
                    string[] strArray = new string[]
                    {
                        "0000,0", "0001,1", "0002,2", "0003,3", "0004,4",
                        "1000,4", "1001,1", "1002,2", "1003,3", "1004,4",
                        "2000,0", "2001,2", "2002,3", "2003,3", "2004,4",
                        "0010,1", "0011,1", "0012,2", "0013,3", "0014,4",
                        "1010,1", "1011,1", "1012,2", "1013,3", "1014,4",
                        "2010,2", "2011,2", "2012,3", "2013,3", "2014,4",

                        "0100,0", "0101,1", "0102,2", "0103,3", "0104,4",
                        "1100,4", "1101,1", "1102,2", "1103,3", "1104,4",
                        "2100,0", "2101,2", "2102,3", "2103,3", "2104,4",
                        "0110,1", "0111,1", "0112,2", "0113,3", "0114,4",
                        "1110,1", "1111,1", "1112,2", "1113,3", "1114,4",
                        "2110,3", "2111,1", "2112,3", "2113,3", "2114,4"
                    };
                    foreach (string s in strArray)
                    {
                        string[] splittedString = s.Split(',');
                        _toneDict.Add(splittedString[0], splittedString[1]);
                    }
                }
            }
        }

        string _preConsString;
        string _postConsString;
        string _vowelPhoneme;
        bool _isLongVowel;
        bool[] _consAllowed;
        List<string> _fConsAllowed;
        bool[] _toneAllowed;
        string _forceFCons;

        public WordStructurePronunciationRule(string vowelFormat, string vowelPhoneme, bool isLongVowel, IEnumerable<bool> consAllowed, IEnumerable<string> fConsAllowed, IEnumerable<bool> toneAllowed, string forceFCons)
        {
            string[] strArray = vowelFormat.Split('-');
            if (strArray.Length == 2)
            {
                _preConsString = strArray[0];
                _postConsString = strArray[1];
            } else
                throw new ArgumentException("pronunciationFormat is invalid", "pronunciationFormat");
            _isLongVowel = isLongVowel;
            _vowelPhoneme = vowelPhoneme;

            _consAllowed = new List<bool>(consAllowed).ToArray();
            _fConsAllowed = new List<string>(fConsAllowed);
            _toneAllowed = new List<bool>(toneAllowed).ToArray();

            _forceFCons = forceFCons;
        }
        public WordStructurePronunciationRule(string vowelFormat, string vowelPhoneme, bool isLongVowel, IEnumerable<bool> consAllowed, IEnumerable<string> fConsAllowed, IEnumerable<bool> toneAllowed)
            : this(vowelFormat, vowelPhoneme, isLongVowel, consAllowed, fConsAllowed, toneAllowed, "")
        {
        }
        public WordStructurePronunciationRule(string vowelFormat, string vowelPhoneme, bool isLongVowel, IEnumerable<string> fConsAllowed, IEnumerable<bool> toneAllowed, string forceFCons)
            : this(vowelFormat, vowelPhoneme, isLongVowel,
            new bool[] { true, true, true }, fConsAllowed, toneAllowed, forceFCons)
        {
        }
        public WordStructurePronunciationRule(string vowelFormat, string vowelPhoneme, bool isLongVowel, IEnumerable<string> fConsAllowed, IEnumerable<bool> toneAllowed)
            : this(vowelFormat, vowelPhoneme, isLongVowel,
            new bool[] { true, true, true }, fConsAllowed, toneAllowed, "")
        {
        }
        public WordStructurePronunciationRule(string vowelFormat, string vowelPhoneme, bool isLongVowel, IEnumerable<string> fConsAllowed, string forceFCons)
            : this(vowelFormat, vowelPhoneme, isLongVowel,
            new bool[] { true, true, true }, fConsAllowed, new bool[] { true, true, true, true, true }, forceFCons)
        {
        }
        public WordStructurePronunciationRule(string vowelFormat, string vowelPhoneme, bool isLongVowel, IEnumerable<string> fConsAllowed)
            : this(vowelFormat, vowelPhoneme, isLongVowel,
            new bool[] { true, true, true }, fConsAllowed, new bool[] { true, true, true, true, true }, "")
        {
        }
        public WordStructurePronunciationRule(string vowelFormat, string vowelPhoneme, bool isLongVowel)
            : this(vowelFormat, vowelPhoneme, isLongVowel,
            new bool[] { true, true, true }, new string[] { "น", "ม", "ย", "ว", "ง", "", "ก", "ด", "บ" }, new bool[] { true, true, true, true, true }, "")
        {
        }

        public Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation)
        {
            string orgWord = word;
            int toneTypeCode = 0;
            {
                char[] allToneChars = new char[] { "อ่"[1], "อ้"[1], "อ๊"[1], "อ๋"[1] };
                for (int count = 0; count < allToneChars.Length; count++)
                {
                    char ch = allToneChars[count];
                    int charIndex = word.IndexOf(ch);
                    if (charIndex >= 0)
                    {
                        toneTypeCode = (count + 1);
                        word = word.Remove(charIndex, 1);
                        break;
                    }
                }
            }
            int vowelTypeCode = _isLongVowel ? 0 : 1;
            
            //if (_toneAllowed[toneTypeCode] && word.StartsWith(_preConsString))
            if ((_toneAllowed[toneTypeCode] && (word.Substring(0, 1).Equals(_preConsString))) || (_toneAllowed[toneTypeCode] && word.StartsWith(_preConsString)))
            {
                if (_preConsString.Length > 0)
                    word = word.Substring(_preConsString.Length);
                int index = word.IndexOf(_postConsString, 1);
                if (index >= 0)
                {
                    string cons;
                    string fCons;
                    if (_postConsString.Length > 0)
                    {
                        cons = word.Substring(0, index);
                        fCons = word.Substring(index + _postConsString.Length);
                                                
                    } else
                    {
                        if (word.IndexOf("อฺ"[1]) >= 0)
                        {
                            cons = word.Substring(0, 3);
                            fCons = word.Substring(3);
                        } else
                        {
                            cons = word.Substring(0, 1);
                            fCons = word.Substring(1);
                        }
                    }
                    
                    string consPhoneme;
                    string fConsPhoneme;
                    string tonePhoneme;

                    int consTypeCode;
                    if (DictResource.Instance.MiddleConsPhonemeDict.ContainsKey(cons))
                        consTypeCode = 0;
                    else if (DictResource.Instance.HighConsPhonemeDict.ContainsKey(cons))
                        consTypeCode = 1;
                    else if (DictResource.Instance.LowConsPhonemeDict.ContainsKey(cons))
                        consTypeCode = 2;
                    else
                    {                        
                        return new Dictionary<string, string[]>();
                    }

                    int fConsTypeCode;
                    if (fCons.Length <= 0)
                        fConsTypeCode = _isLongVowel ? 0 : 1;
                    else
                        fConsTypeCode = DictResource.Instance.AliveFConsPhonemeDict.ContainsKey(fCons) ? 0 : 1;
                    

                    if (_consAllowed[consTypeCode] && _fConsAllowed.Contains(fCons))
                    {
                        consPhoneme = DictResource.Instance.ConsPhonemeDict[cons];
                        fConsPhoneme = DictResource.Instance.FConsPhonemeDict[fCons];
                        tonePhoneme = DictResource.Instance.ToneDict[consTypeCode.ToString() + vowelTypeCode.ToString() + fConsTypeCode.ToString() + toneTypeCode.ToString()];

                        if (_forceFCons.Length > 0)
                            fConsPhoneme = DictResource.Instance.FConsPhonemeDict[_forceFCons];

                        Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>();
                        string[] newPronunciation = (string[])prePronunciation.Clone();
                        newPronunciation[newPronunciation.Length - 1] += consPhoneme + '-' + _vowelPhoneme + '-' + fConsPhoneme + '-' + tonePhoneme;
                        matchedPrefix.Add(orgWord, newPronunciation);
                        return matchedPrefix;
                    }
                }
            }
            return new Dictionary<string, string[]>();
        }
    }
}
