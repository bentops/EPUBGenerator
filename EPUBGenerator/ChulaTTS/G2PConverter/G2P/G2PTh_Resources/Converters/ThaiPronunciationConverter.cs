using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    public class ThaiPronunciationConverter
    {
        private static ThaiPronunciationConverter SingletonInstance = null;

        public static ThaiPronunciationConverter Instance
        {
            get
            {
                if (SingletonInstance == null)
                    SingletonInstance = new ThaiPronunciationConverter();
                return SingletonInstance;
            }
        }

        IWordStructureRule _thaiWordStructureRule;
        bool _isInitialized = false;

        //Dictionary<string, bool> _availableSyllableDict = new Dictionary<string, bool>();
        int _maxSearchingResult = 1000;

        string[] _lastSegmentedWordPronunciation = new string[0];

        //Dictionary<string, Dictionary<string, string[]>> _cachedThaiDictMatchedPrefix = new Dictionary<string, Dictionary<string, string[]>>();
        //Dictionary<string, Dictionary<string, string[]>> _cachedThaiRuleMatchedPrefix = new Dictionary<string, Dictionary<string, string[]>>();
        Dictionary<string, List<KeyValuePair<string, string[]>>> _cachedSegmentedWordList = new Dictionary<string, List<KeyValuePair<string, string[]>>>();
        Dictionary<string, int> _cachedResultScore = new Dictionary<string, int>();
        Dictionary<string, int> _cachedSegmentedWordsLength = new Dictionary<string, int>();

        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set { _isInitialized = value; }
        }

        public int MaxSearchingResult
        {
            get { return _maxSearchingResult; }
            set { _maxSearchingResult = value; }
        }

        private ThaiPronunciationConverter() { }

        public void InitializeThaiWordRules()
        {
            WordStructureThaiDictRule.Instance.InitializeThaiDict();
            //foreach (string s in Properties.Resources.AvailableSyllableDict.Split(' '))
            //    if (!_availableSyllableDict.ContainsKey(s))
            //        _availableSyllableDict.Add(s, true);

            IWordStructureRule tcc;
            IWordStructureRule tcc1;
            IWordStructureRule tcc2;
            IWordStructureRule tcc3;
            IWordStructureRule exceptionRule = new WordStructureUnionRule
                (
                    "ก็,ก้อ",
                    "อย่าง,หฺย่าง", "อยู่,หฺยู่", "อย่า,หฺย่า", "อยาก,หฺยาก",
                    "ฤๅ,รือ", "ฤา,รือ", "ฦๅ,ลือ", "ฦา,ลือ", "ฤ,รึ",
                    "มุข,มุก", "สุข,สุก",
                    "ฤก,เริก", "ฤท,ริด", "น้ำ,น้าม", "ศีร,สี", "กฤษ,กฺริด", "ไทย,ทัย", "ได้,ด้าย", "ไม้,ม้าย", "จริง,จิง"
                );
            IWordStructureRule leadConsRule1 = new WordStructureUnionRule
                (
                "หง,หฺง", "หญ,หฺย", "หน,หฺน", "หม,หฺม", "หย,หฺย", "หร,หฺร", "หล,หฺล", "หว,หฺว",
                "ทร,ซ", "สร,ส", "ศร,ส"
                );
            IWordStructureRule leadConsRule2 = new WordStructureUnionRule
                (
                "กร,กฺร", "ขร,ขฺร", "คร,คฺร", "ตร,ตฺร", "ปร,ปฺร", "พร,พฺร", "ฟร,ฟฺร"
                );
            IWordStructureRule leadConsRule3 = new WordStructureUnionRule
                (
                "กล,กฺล", "ขล,ขฺล", "คล,คฺล", "ปล,ปฺล", "พล,พฺล", "ฟล,ฟฺล"
                );
            IWordStructureRule leadConsRule4 = new WordStructureUnionRule
                (
                "กว,กฺว", "ขว,ขฺว", "คว,คฺว"
                );
            IWordStructureRule consRule = new WordStructureUnionRule
                (
                WordStructureChar.Cons,
                leadConsRule1,
                leadConsRule2,
                leadConsRule3,
                leadConsRule4,
                WordStructureTwoChar.Instance
                );
            IWordStructureRule karunRule;
            IWordStructureRule middleKarunRule;
            {
                IWordStructureRule karunConsRule = WordStructureChar.Cons.RemovePronunciationDict();
                IWordStructureRule wordStructureRule = new WordStructureUnionRule
                    (
                    "ษมณ์,",
                    new WordStructureConcatRule(new WordStructureOptionalRule(karunConsRule), karunConsRule, "อ์"[1] + ","),
                    new WordStructureConcatRule(karunConsRule, new WordStructureUnionRule("อุ"[1] + ",", "อิ"[1] + ","), "อ์"[1] + ",")
                    );
                karunRule = new WordStructureOptionalRule(wordStructureRule);
                middleKarunRule = new WordStructureOptionalRule(new WordStructureConcatRule(karunConsRule, "อ์"[1] + ","));
            }
            {
                tcc1 = new WordStructureUnionRule
                    (
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, 'ะ'),
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, 'า', middleKarunRule, new WordStructureOptionalRule(new WordStructureUnionRule(WordStructureChar.FCons, "ตร,ด", "ติ,ด", "ถ,ด", "รถ,ด"))),
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, 'ว', WordStructureChar.FCons),
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, 'อ', middleKarunRule, WordStructureOptionalRule.FCons),
                    new WordStructureConcatRule("อ็"[1], 'อ', middleKarunRule, WordStructureChar.FCons),
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, WordStructureChar.FCons - 'ย' - 'ร' - 'ว'),
                    new WordStructureConcatRule("อิ"[1], WordStructureOptionalRule.Tone, new WordStructureOptionalRule(new WordStructureUnionRule(WordStructureChar.FConsNoYorYakRorRuer, "ตร,ด"))),
                    new WordStructureConcatRule("อี"[1], WordStructureOptionalRule.Tone, WordStructureOptionalRule.FConsNoYorYakRorRuer),
                    new WordStructureConcatRule("อึ"[1], WordStructureOptionalRule.Tone, WordStructureOptionalRule.FConsNoRorRuer),
                    new WordStructureConcatRule("อื"[1], WordStructureOptionalRule.Tone, new WordStructureUnionRule(WordStructureChar.FConsNoRorRuer, 'อ')),
                    new WordStructureConcatRule("อุ"[1], WordStructureOptionalRule.Tone, new WordStructureOptionalRule(new WordStructureUnionRule(WordStructureChar.FCons, "ติ,ด"))),
                    new WordStructureConcatRule("อู"[1], WordStructureOptionalRule.Tone, WordStructureOptionalRule.FCons),
                    new WordStructureConcatRule("," + "อั"[1], WordStructureOptionalRule.Tone, "อำ"[1] + ",ม"),
                    new WordStructureConcatRule("อั"[1], WordStructureOptionalRule.Tone, new WordStructureUnionRule(WordStructureChar.FConsNoRorRuer, "ตร,ด", "ติ,ด", "วะ"))
                    );
            }
            {
                tcc2 = new WordStructureUnionRule
                    (
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, new WordStructureUnionRule("ะ", "า", "าะ", "อะ")),
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, new WordStructureUnionRule(WordStructureChar.FConsNoYorYak, "ตุ,ด")),
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, 'อ'),
                    WordStructureChar.Tone,
                    new WordStructureConcatRule("อ็"[1], WordStructureChar.FConsNoYorYakRorRuer),
                    new WordStructureConcatRule("อิ"[1], WordStructureOptionalRule.Tone, middleKarunRule, WordStructureChar.FConsNoRorRuer),
                    new WordStructureConcatRule("อี"[1], WordStructureOptionalRule.Tone, 'ย', new WordStructureOptionalRule(new WordStructureUnionRule(WordStructureChar.FConsNoYorYak, 'ะ'))),
                    new WordStructureConcatRule("อื"[1], WordStructureOptionalRule.Tone, 'อ', new WordStructureOptionalRule(new WordStructureUnionRule(WordStructureChar.FConsNoRorRuer, 'ะ')))
                    );
            }
            {
                tcc3 = new WordStructureUnionRule
                    (
                    new WordStructureConcatRule(WordStructureOptionalRule.Tone, new WordStructureUnionRule(WordStructureChar.FConsNoYorYak, 'ะ')),
                    new WordStructureConcatRule("อ็"[1], WordStructureChar.FConsNoYorYak)
                    );
            }
            {
                IWordStructureRule wordStructureRule = new WordStructureUnionRule
                    (
                    exceptionRule,
                    new WordStructureConcatRule(new WordStructureUnionRule(WordStructureChar.Cons, WordStructureTwoChar.Instance), "รร," + "อั"[1], new WordStructureUnionRule(WordStructureChar.RorHunFCons, ",น")),
                    new WordStructureConcatRule(consRule, tcc1),
                    new WordStructureConcatRule(new WordStructureUnionRule(WordStructureChar.Cons, leadConsRule1, WordStructureTwoChar.Instance), "ร,อน"),
                    new WordStructureConcatRule('เ', consRule, "," + "อิ"[1], WordStructureOptionalRule.Tone, 'ย'),
                    new WordStructureConcatRule('เ', consRule, new WordStructureOptionalRule(tcc2)),
                    new WordStructureConcatRule('แ', consRule, new WordStructureOptionalRule(tcc3)),
                    new WordStructureConcatRule(new WordStructureUnionRule("ไ,", "ใ,"), consRule, "," + "อั"[1], WordStructureOptionalRule.Tone, ",ย"),
                    new WordStructureConcatRule('โ', new WordStructureUnionRule(leadConsRule2, leadConsRule3, WordStructureChar.Cons, leadConsRule1), WordStructureOptionalRule.Tone, new WordStructureOptionalRule(new WordStructureUnionRule(WordStructureChar.FCons, 'ะ')))
                    );
                tcc = new WordStructureConcatRule(wordStructureRule, karunRule);
            }
            _thaiWordStructureRule = tcc;

            this.IsInitialized = true;
        }

        public ThaiPronunciationResult ToThaiPronunciation(string word)
        {
            if (!this.IsInitialized)
                this.InitializeThaiWordRules();

            List<KeyValuePair<string, string[]>> resultSegmentedWordList = new List<KeyValuePair<string, string[]>>();
            List<string> resultPronunciationList = new List<string>();
            if (word.Length == 1 && WordStructureChar.Cons.IsCharMatched(word[0]))
            {
                Dictionary<string, string[]> matchedPrefix = WordStructureChar.Cons.GetMatchedPrefix(word, new string[] { "" });
                if (matchedPrefix != null && matchedPrefix.ContainsKey(word.Substring(0, 1)))
                {
                    string pronunciation = matchedPrefix[word.Substring(0, 1)][0] + 'อ';
                    resultPronunciationList.Add(pronunciation);
                    resultSegmentedWordList.Add(new KeyValuePair<string, string[]>(word, new string[] { pronunciation }));
                    _lastSegmentedWordPronunciation = new string[] { pronunciation };
                }
            } else
            {
                bool isFirst = true;
                while (word.Length > 0)
                {
                    if (!isFirst)
                    {
                        if (word[0] == ' ' || word[0] == '\n')
                        {
                            resultSegmentedWordList.Add(new KeyValuePair<string, string[]>(word.Substring(0, 1), new string[] { "$" }));
                            resultPronunciationList.Add("$");
                        } else if (word[0] == 'ๆ')
                        {
                            resultSegmentedWordList.Add(new KeyValuePair<string, string[]>(word.Substring(0, 1), _lastSegmentedWordPronunciation));
                            resultPronunciationList.AddRange(_lastSegmentedWordPronunciation);
                        } else
                        {
                            // If there has any word left, segment 1 char after that to a thai word.
                            Dictionary<string, string[]> matchedPrefix = WordStructureChar.Cons.GetMatchedPrefix(word, new string[] { "" });
                            if (matchedPrefix != null && matchedPrefix.ContainsKey(word.Substring(0, 1)))
                            {
                                string pronunciation = matchedPrefix[word.Substring(0, 1)][0] + 'ะ';
                                resultSegmentedWordList.Add(new KeyValuePair<string, string[]>(word.Substring(0, 1), new string[] { pronunciation }));
                                resultPronunciationList.Add(pronunciation);
                                _lastSegmentedWordPronunciation = new string[] { pronunciation };
                            }
                        }
                        word = word.Substring(1);
                    }
                    // Try segmenting a thai word
                    //List<KeyValuePair<string, string[]>> currentSegmentedWordList = new List<KeyValuePair<string, string[]>>();
                    //List<string> currentPronunciationList = new List<string>();
                    //List<KeyValuePair<string, string[]>> subResultSegmentedWordList = new List<KeyValuePair<string, string[]>>();
                    //List<string> subResultPronunciationList = new List<string>();
                    //int resultLeftWordLength = word.Length;
                    //double resultScore = 0;
                    //int numSolutionFound = 0;
                    ////_cachedThaiDictMatchedPrefix.Clear();
                    ////_cachedThaiRuleMatchedPrefix.Clear();
                    //this.ToThaiPronunciation(word,
                    //    currentSegmentedWordList, currentPronunciationList, 0,
                    //    subResultSegmentedWordList, subResultPronunciationList, ref resultLeftWordLength, ref resultScore,
                    //    ref numSolutionFound);
                    //resultPronunciationList.AddRange(subResultPronunciationList);
                    //resultSegmentedWordList.AddRange(subResultSegmentedWordList);
                    _cachedSegmentedWordList.Clear();
                    _cachedResultScore.Clear();
                    _cachedSegmentedWordsLength.Clear();
                    List<KeyValuePair<string, string[]>> subResultSegmentedWordList;
                    int subResultScore;
                    int subResultSegmentedWordsLength;
                    this.SegmentThaiSentence(word, out subResultSegmentedWordList, out subResultScore, out subResultSegmentedWordsLength);
                    resultSegmentedWordList.AddRange(subResultSegmentedWordList);
                    foreach (KeyValuePair<string, string[]> keyValuePair in subResultSegmentedWordList)
                        resultPronunciationList.AddRange(keyValuePair.Value);

                    if (subResultSegmentedWordList.Count > 0)
                        _lastSegmentedWordPronunciation = subResultSegmentedWordList[subResultSegmentedWordList.Count - 1].Value;
                    word = word.Substring(subResultSegmentedWordsLength);

                    isFirst = false;
                }
            }
            return new ThaiPronunciationResult(resultPronunciationList.ToArray(), resultSegmentedWordList.ToArray());
        }

        //private bool ToThaiPronunciation(string word,
        //    List<KeyValuePair<string, string[]>> currentSegmentedWordList, List<string> currentPronunciationList, int currentScore,
        //    List<KeyValuePair<string, string[]>> resultSegmentedWordList, List<string> resultPronunciationList, ref int resultLeftWordLength, ref double resultScore,
        //    ref int numSolutionFound)
        //{
        //    if (!_cachedThaiDictMatchedPrefix.ContainsKey(word))
        //        _cachedThaiDictMatchedPrefix.Add(word, WordStructureThaiDictRule.Instance.GetMatchedPrefix(word, new string[] { "" }));
        //    if (!_cachedThaiRuleMatchedPrefix.ContainsKey(word))
        //        _cachedThaiRuleMatchedPrefix.Add(word, _thaiWordStructureRule.GetMatchedPrefix(word, new string[] { "" }));
        //    Dictionary<string, string[]> thaiDictMatchedPrefix = _cachedThaiDictMatchedPrefix[word];
        //    Dictionary<string, string[]> thaiRuleMatchedPrefix = _cachedThaiRuleMatchedPrefix[word];
        //    Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>(thaiDictMatchedPrefix);
        //    foreach (KeyValuePair<string, string[]> keyValuePair in thaiRuleMatchedPrefix)
        //        if (!matchedPrefix.ContainsKey(keyValuePair.Key))
        //            matchedPrefix.Add(keyValuePair.Key, keyValuePair.Value);
        //    if (matchedPrefix != null && matchedPrefix.Count > 0)
        //    {
        //        string[] keys = new string[matchedPrefix.Count];
        //        matchedPrefix.Keys.CopyTo(keys, 0);
        //        Array.Sort<string>(keys, new Comparison<string>(delegate(string x, string y)
        //        {
        //            int firstCompareResult = (thaiDictMatchedPrefix.ContainsKey(y) ? 1 : 0) - (thaiDictMatchedPrefix.ContainsKey(x) ? 1 : 0);
        //            return firstCompareResult != 0 ? firstCompareResult : (y.Length - x.Length);
        //        }));

        //        foreach (string prefix in keys)
        //        {
        //            int nextScore = currentScore;
        //            if (thaiDictMatchedPrefix.ContainsKey(prefix))
        //                nextScore += prefix.Length;

        //            if (nextScore + word.Length - prefix.Length < resultScore)
        //                continue;

        //            string[] prefixPronunciations = matchedPrefix[prefix];
        //            currentSegmentedWordList.Add(new KeyValuePair<string, string[]>(prefix, prefixPronunciations));
        //            currentPronunciationList.AddRange(prefixPronunciations);
        //            if (this.ToThaiPronunciation(word.Substring(prefix.Length),
        //                currentSegmentedWordList, currentPronunciationList, nextScore,
        //                resultSegmentedWordList, resultPronunciationList, ref resultLeftWordLength,
        //                ref resultScore, ref numSolutionFound))
        //                return true;
        //            currentSegmentedWordList.RemoveAt(currentSegmentedWordList.Count - 1);
        //            currentPronunciationList.RemoveRange(currentPronunciationList.Count - prefixPronunciations.Length, prefixPronunciations.Length);
        //        }
        //    } else
        //    {
        //        if (word.Length < resultLeftWordLength
        //            || word.Length == resultLeftWordLength
        //            && (currentScore > resultScore
        //            || currentScore == resultScore && currentSegmentedWordList.Count < resultSegmentedWordList.Count))
        //        {
        //            resultSegmentedWordList.Clear();
        //            resultSegmentedWordList.AddRange(currentSegmentedWordList);
        //            resultPronunciationList.Clear();
        //            resultPronunciationList.AddRange(currentPronunciationList);
        //            resultLeftWordLength = word.Length;
        //            resultScore = currentScore;
        //        }
        //        if (++numSolutionFound >= this.MaxSearchingResult)
        //            return true;
        //    }
        //    return false;
        //}
        private void SegmentThaiSentence(string word,
            out List<KeyValuePair<string, string[]>> resultSegmentedWordList, out int resultScore, out int segmentedWordsLength)
        {
            if (_cachedSegmentedWordList.ContainsKey(word))
            {
                resultSegmentedWordList = _cachedSegmentedWordList[word];
                resultScore = _cachedResultScore[word];
                segmentedWordsLength = _cachedSegmentedWordsLength[word];
                return;
            }
            resultSegmentedWordList = new List<KeyValuePair<string, string[]>>();
            resultScore = 0;
            segmentedWordsLength = 0;
            Dictionary<string, string[]> thaiDictMatchedPrefix = WordStructureThaiDictRule.Instance.GetMatchedPrefix(word, new string[] { "" });
            Dictionary<string, string[]> thaiRuleMatchedPrefix = _thaiWordStructureRule.GetMatchedPrefix(word, new string[] { "" });
            Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>(thaiDictMatchedPrefix);
            foreach (KeyValuePair<string, string[]> keyValuePair in thaiRuleMatchedPrefix)
                if (!matchedPrefix.ContainsKey(keyValuePair.Key))
                    matchedPrefix.Add(keyValuePair.Key, keyValuePair.Value);
            if (matchedPrefix != null && matchedPrefix.Count > 0)
            {
                string[] keys = new string[matchedPrefix.Count];
                matchedPrefix.Keys.CopyTo(keys, 0);
                Array.Sort<string>(keys, new Comparison<string>(delegate(string x, string y)
                {
                    int firstCompareResult = (thaiDictMatchedPrefix.ContainsKey(y) ? 1 : 0) - (thaiDictMatchedPrefix.ContainsKey(x) ? 1 : 0);
                    return firstCompareResult != 0 ? firstCompareResult : (y.Length - x.Length);
                }));

                foreach (string prefix in keys)
                {
                    List<KeyValuePair<string, string[]>> subResultSegmentedWordList;
                    int subResultScore;
                    int subResultSegmentedWordLength;
                    this.SegmentThaiSentence(word.Substring(prefix.Length), out subResultSegmentedWordList, out subResultScore, out subResultSegmentedWordLength);
                    int currentScore = (thaiDictMatchedPrefix.ContainsKey(prefix) ? prefix.Length : 0) + subResultScore;
                    int currentSegmentedWordsLength = prefix.Length + subResultSegmentedWordLength;
                    if (currentSegmentedWordsLength > segmentedWordsLength
                        || currentSegmentedWordsLength == segmentedWordsLength
                        && (currentScore > resultScore
                        || currentScore == resultScore
                        && subResultSegmentedWordList.Count < resultSegmentedWordList.Count))
                    {
                        string[] prefixPronunciations = matchedPrefix[prefix];
                        resultSegmentedWordList = new List<KeyValuePair<string, string[]>>();
                        resultSegmentedWordList.Add(new KeyValuePair<string, string[]>(prefix, prefixPronunciations));
                        resultSegmentedWordList.AddRange(subResultSegmentedWordList);
                        resultScore = currentScore;
                        segmentedWordsLength = currentSegmentedWordsLength;
                    }
                }
            }
            _cachedSegmentedWordList.Add(word, resultSegmentedWordList);
            _cachedResultScore.Add(word, resultScore);
            _cachedSegmentedWordsLength.Add(word, segmentedWordsLength);
        }
    }
}
