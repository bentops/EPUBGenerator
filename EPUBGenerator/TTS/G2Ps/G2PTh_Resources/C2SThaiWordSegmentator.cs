using System;
using System.Collections.Generic;
using System.Text;
using ThaiSpeechSynthesizer;
namespace Chula.SLS.TTS.C2SSegmentator
{
    public class C2SThaiWordSegmentator
    {
        #region IThaiWordSegmentator Members
        
        public string[] Segment(string thaiWordString)
        {
            ThaiPronunciationResult result = ThaiPronunciationConverter.Instance.ToThaiPronunciation(thaiWordString);
            return result.Pronunciations;
        }
        
        public KeyValuePair<string, string[]>[] Segments(string thaiWordString)
        {
            ThaiPronunciationResult result = ThaiPronunciationConverter.Instance.ToThaiPronunciation(thaiWordString);
            return result.SegmentedWords;
        }

        public string ConvertPhoneme(string[] thaiWord)
        {

            string[] result = ThaiPhonemeConverter.Instance.ToThaiPhoneme(thaiWord);
            return string.Join(" ", result) + " ";
        }

        #endregion
    }
}
