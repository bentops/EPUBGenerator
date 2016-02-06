using ChulaTTS.G2PConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPUBGenerator.MainLogic
{
    public class Tools
    {
        public static ISentenceSplitter SentenceSplitter { get; set; }
        public static IPreprocessor Preprocessor { get; set; }
        public static IG2P G2P { get; set; }
        public static IPhonemeConverter PhonemeConverter { get; set; }
        
        public static bool IsReady()
        {
            return Preprocessor != null && G2P != null && PhonemeConverter != null;
        }

        public static List<KeyValuePair<String, String>> GetPhonemeList(String input, int type)
        {
            input = Preprocessor.Process(input, type);
            List<KeyValuePair<String, String>> list = G2P.GenTranscriptList(input, type);
            for (int i = 0; i < list.Count; i++)
            {
                String text = list[i].Key;
                String phon = PhonemeConverter.Convert(list[i].Value, type);
                list[i] = new KeyValuePair<String, String>(text, phon);
            }
            return list;
        }

        public static String GetPhoneme(String input, int type)
        {
            input = Preprocessor.Process(input, type);
            input = G2P.GenTranscript(input, type);
            input = PhonemeConverter.Convert(input, type);
            return input;
        }
    }
}
