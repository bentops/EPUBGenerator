using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTS.PhonemeConverters
{
    class NullPhonemeConverter : IPhonemeConverter
    {
        public string[] C2Pronunciation(string input)
        {
            throw new NotImplementedException();
        }

        public string Convert(string input)
        {
            string sil = @"sil;7;0|";
            string output = "";
            int len = input.Length;
            while (0 < len--)
                output += sil;
            return output;
        }
    }
}
