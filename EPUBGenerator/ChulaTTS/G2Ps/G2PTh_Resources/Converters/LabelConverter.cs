using System;
using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    public static class LabelConverter
    {
        struct Phoneme
        {
            private string _phone;
            private string _tone;

            public string Phone { get { return _phone; } }
            public string Tone { get { return _tone; } }

            public Phoneme(string phone, string tone)
            {
                _phone = phone;
                _tone = tone;
            }
        }

        public static string[] ToLabel(string[] phonemeTexts)
        {
            if (phonemeTexts == null || phonemeTexts.Length == 0)
                return new string[0];

            List<Phoneme> phonemeList = SyllableToPhoneTone(phonemeTexts);

            string[] labels = new string[phonemeList.Count];

            string pre2Phone = "XXX";
            string pre1Phone = "XXX";
            string curPhone = "XXX";
            string next1Phone = 1 < phonemeList.Count ? phonemeList[1].Phone : "XXX";
            string next2Phone = 2 < phonemeList.Count ? phonemeList[2].Phone : "XXX";
            for (int count = 0; count < phonemeList.Count; count++)
            {
                pre2Phone = pre1Phone;
                pre1Phone = curPhone;
                curPhone = next1Phone;
                next1Phone = next2Phone;
                next2Phone = count + 2 < phonemeList.Count ? phonemeList[count + 2].Phone : "XXX";
                string tone = phonemeList[count].Tone;

                labels[count] = String.Format("{0}_{1}-{2}+{3}={4}/A:{5}", pre2Phone, pre1Phone, curPhone, next1Phone, next2Phone, tone);
            }

            return labels;
        }

        private static List<Phoneme> SyllableToPhoneTone(string[] phonemeTexts)
        {
            List<Phoneme> phonemeList = new List<Phoneme>();

            foreach (string syl in phonemeTexts)
            {
                if (syl == "$-$-$-$" || syl == "" || syl == " ")
                    phonemeList.Add(new Phoneme("sil", "x"));
                else
                {
                    string[] t = syl.Split('-');
                    string tone = t[t.Length - 1];
                    phonemeList.Add(new Phoneme(t[0], tone));
                    phonemeList.Add(new Phoneme(t[1], tone));
                    if (t.Length == 4 && t[2] != "z^")
                        phonemeList.Add(new Phoneme(t[2], tone));
                }
            }

            return phonemeList;
        }
    }
}
