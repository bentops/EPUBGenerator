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
    }
}
