using System;
using Wirote2TSync;

namespace TTS.PhonemeConverters
{
    class ThaiPhonemeConverter : IPhonemeConverter
    {
        private Converter converter = new Converter();

        public string[] C2Pronunciation(string input)
        {
            throw new NotImplementedException();
        }

        public string Convert(string input)
        {
            return converter.Conversion4(input);
        }
    }
}
