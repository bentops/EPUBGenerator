using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChulaTTS.Synthesizer
{
    public class Phone2Lab
    {
        public string G5T3(string inp)
        {
            string str1 = "";
            List<Phone2Lab.phoneme> list = new List<Phone2Lab.phoneme>();
            Phone2Lab.phoneme phoneme;
            phoneme.Phoneme = "sil";
            phoneme.Tone = "0";
            phoneme.Pos = "";
            list.Add(phoneme);
            list.Add(phoneme);
            foreach (string str2 in inp.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                char[] chArray = new char[1] { ';' };
                string[] strArray = str2.Split(chArray);
                phoneme.Phoneme = strArray[0];
                phoneme.Tone = strArray[1];
                list.Add(phoneme);
            }
            phoneme.Phoneme = "sil";
            phoneme.Tone = "0";
            list.Add(phoneme);
            list.Add(phoneme);
            for (int index = 2; index < list.Count - 2; ++index)
                str1 = str1 + list[index - 2].Phoneme + "_" + list[index - 1].Phoneme + "-" + list[index].Phoneme + "+" + list[index + 1].Phoneme + "=" + list[index + 2].Phoneme + "/A:" + list[index - 1].Tone + "-" + list[index].Tone + "+" + list[index + 1].Tone + "\n";
            return str1;
        }

        public string G5T3P3(string inp)
        {
            string str1 = "";
            List<Phone2Lab.phoneme> list = new List<Phone2Lab.phoneme>();
            Phone2Lab.phoneme phoneme;
            phoneme.Phoneme = "sil";
            phoneme.Tone = "7";
            phoneme.Pos = "0";
            list.Add(phoneme);
            list.Add(phoneme);
            foreach (string str2 in inp.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                char[] chArray = new char[1] { ';' };
                string[] strArray = str2.Split(chArray);
                phoneme.Phoneme = strArray[0];
                phoneme.Tone = strArray[1];
                phoneme.Pos = strArray[2];
                list.Add(phoneme);
            }
            phoneme.Phoneme = "sil";
            phoneme.Tone = "7";
            phoneme.Pos = "0";
            list.Add(phoneme);
            list.Add(phoneme);
            for (int index = 2; index < list.Count - 2; ++index)
                str1 = str1 + list[index - 2].Phoneme + "_" + list[index - 1].Phoneme + "-" + list[index].Phoneme + "+" + list[index + 1].Phoneme + "=" + list[index + 2].Phoneme + "/A:" + list[index - 1].Tone + "-" + list[index].Tone + "+" + list[index + 1].Tone + "/P:" + list[index - 1].Pos + "A" + list[index].Pos + "B" + list[index + 1].Pos + "\n";
            return str1;
        }

        public string G5T3P3S(string inp)
        {
            string str1 = "";
            List<Phone2Lab.phoneme> list = new List<Phone2Lab.phoneme>();
            Phone2Lab.phoneme phoneme;
            phoneme.Phoneme = "sil";
            phoneme.Tone = "7";
            phoneme.Pos = "S";
            list.Add(phoneme);
            list.Add(phoneme);
            foreach (string str2 in inp.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                char[] chArray = new char[1] { ';' };
                string[] strArray = str2.Split(chArray);
                phoneme.Phoneme = strArray[0];
                phoneme.Tone = strArray[1];
                phoneme.Pos = strArray[2];
                list.Add(phoneme);
            }
            phoneme.Phoneme = "sil";
            phoneme.Tone = "7";
            phoneme.Pos = "S";
            list.Add(phoneme);
            list.Add(phoneme);
            for (int index = 2; index < list.Count - 2; ++index)
                str1 = str1 + list[index - 2].Phoneme + "_" + list[index - 1].Phoneme + "-" + list[index].Phoneme + "+" + list[index + 1].Phoneme + "=" + list[index + 2].Phoneme + "/A:" + list[index - 1].Tone + "-" + list[index].Tone + "+" + list[index + 1].Tone + "/P:" + list[index - 1].Pos + "A" + list[index].Pos + "B" + list[index + 1].Pos + "\n";
            return str1;
        }

        public string G5T5(string inp)
        {
            string str1 = "";
            List<Phone2Lab.phoneme> list = new List<Phone2Lab.phoneme>();
            Phone2Lab.phoneme phoneme;
            phoneme.Phoneme = "sil";
            phoneme.Tone = "0";
            phoneme.Pos = "";
            list.Add(phoneme);
            list.Add(phoneme);
            foreach (string str2 in inp.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                char[] chArray = new char[1] { ';' };
                string[] strArray = str2.Split(chArray);
                phoneme.Phoneme = strArray[0];
                phoneme.Tone = strArray[1];
                list.Add(phoneme);
            }
            phoneme.Phoneme = "sil";
            phoneme.Tone = "0";
            list.Add(phoneme);
            list.Add(phoneme);
            for (int index = 2; index < list.Count - 2; ++index)
                str1 = str1 + list[index - 2].Phoneme + "_" + list[index - 1].Phoneme + "-" + list[index].Phoneme + "+" + list[index + 1].Phoneme + "=" + list[index + 2].Phoneme + "/A:" + list[index - 2].Tone + "_" + list[index - 1].Tone + "-" + list[index].Tone + "+" + list[index + 1].Tone + "=" + list[index + 2].Tone + "\n";
            return str1;
        }

        public string G7T7(string inp)
        {
            string str1 = "";
            List<Phone2Lab.phoneme> list = new List<Phone2Lab.phoneme>();
            Phone2Lab.phoneme phoneme;
            phoneme.Phoneme = "sil";
            phoneme.Tone = "0";
            phoneme.Pos = "";
            list.Add(phoneme);
            list.Add(phoneme);
            list.Add(phoneme);
            foreach (string str2 in inp.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                char[] chArray = new char[1] { ';' };
                string[] strArray = str2.Split(chArray);
                phoneme.Phoneme = strArray[0];
                phoneme.Tone = strArray[1];
                list.Add(phoneme);
            }
            phoneme.Phoneme = "sil";
            phoneme.Tone = "0";
            list.Add(phoneme);
            list.Add(phoneme);
            list.Add(phoneme);
            for (int index = 3; index < list.Count - 3; ++index)
                str1 = str1 + list[index - 3].Phoneme + "<" + list[index - 2].Phoneme + "_" + list[index - 1].Phoneme + "-" + list[index].Phoneme + "+" + list[index + 1].Phoneme + "=" + list[index + 2].Phoneme + ">" + list[index + 3].Phoneme + "/A:" + list[index - 3].Tone + "<" + list[index - 2].Tone + "_" + list[index - 1].Tone + "-" + list[index].Tone + "+" + list[index + 1].Tone + "=" + list[index + 2].Tone + ">" + list[index + 3].Tone + "\n";
            return str1;
        }

        private struct phoneme
        {
            public string Phoneme;
            public string Tone;
            public string Pos;
        }
    }
}
