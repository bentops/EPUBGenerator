using TTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EPUBGenerator.MainLogic
{
    class Tools
    {
        public static SentenceSplitter SentenceSplitter { get; set; }
        public static CPreprocessor Preprocessor { get; set; }
        public static CG2P G2P { get; set; }
        public static CPhonemeConverter PhonemeConverter { get; set; }
        public static CSynthesizer Synthesizer { get; set; }

        public static bool IsReady()
        {
            return Preprocessor != null && G2P != null && PhonemeConverter != null && Synthesizer != null;
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

        public static MemoryStream Synthesize(String input, int type, string id)
        {
            Console.WriteLine("in");
            return Synthesizer.Synthesize(input, type, id);
        }
    }
}
