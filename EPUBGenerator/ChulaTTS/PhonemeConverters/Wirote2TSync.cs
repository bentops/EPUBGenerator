using System;
using System.Collections.Generic;

namespace Wirote2TSync
{
    public class Converter
    {
        private string[] ThaiPV;

        public Converter()
        {
            this.ThaiPV = new string[24];
            this.ThaiPV[0] = "iia";
            this.ThaiPV[1] = "vva";
            this.ThaiPV[2] = "uua";
            this.ThaiPV[3] = "aa";
            this.ThaiPV[4] = "ii";
            this.ThaiPV[5] = "vv";
            this.ThaiPV[6] = "uu";
            this.ThaiPV[7] = "ee";
            this.ThaiPV[8] = "xx";
            this.ThaiPV[9] = "oo";
            this.ThaiPV[10] = "@@";
            this.ThaiPV[11] = "va";
            this.ThaiPV[12] = "qq";
            this.ThaiPV[13] = "ia";
            this.ThaiPV[14] = "ua";
            this.ThaiPV[15] = "a";
            this.ThaiPV[16] = "i";
            this.ThaiPV[17] = "v";
            this.ThaiPV[18] = "u";
            this.ThaiPV[19] = "e";
            this.ThaiPV[20] = "x";
            this.ThaiPV[21] = "o";
            this.ThaiPV[22] = "@";
            this.ThaiPV[23] = "q";
        }

        public string Conversion(string inp)
        {
            List<Converter.phoneme> list1 = new List<Converter.phoneme>();
            List<string> list2 = new List<string>();
            list1.Clear();
            Converter.phoneme phoneme;
            phoneme.Phoneme = "sil";
            phoneme.Tone = "7";
            phoneme.Pos = "0";
            list1.Add(phoneme);
            list1.Add(phoneme);
            list1.Add(phoneme);
            string[] strArray1 = inp.Split(' ');
            for (int index1 = 0; index1 < strArray1.Length; ++index1)
            {
                foreach (string str in strArray1[index1].Split("~".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] strArray2 = str.Split("}^'".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (strArray2.Length != 1)
                    {
                        for (int index2 = 1; index2 <= strArray2.Length; ++index2)
                            list2.Add(index2.ToString());
                    }
                    else
                        list2.Add("1");
                }
                string[] array = new string[list2.Count];
                list2.CopyTo(array);
                int index3 = 0;
                string[] strArray3 = strArray1[index1].Replace("?", "z").Replace("-", "z").Split("^'}~+\xF8C5".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                bool flag = false;
                for (int index2 = 0; index2 < strArray3.Length; ++index2)
                {
                    if (strArray3[index2].IndexOf("UUa") != -1)
                        strArray3[index2] = strArray3[index2].Replace("UUa", "vva");
                    else if (strArray3[index2].IndexOf("Ua") != -1)
                        strArray3[index2] = strArray3[index2].Replace("Ua", "va");
                    else if (strArray3[index2].IndexOf("UU") != -1)
                        strArray3[index2] = strArray3[index2].Replace("UU", "vv");
                    else if (strArray3[index2].IndexOf("@@") != -1)
                        strArray3[index2] = strArray3[index2].Replace("@@", "qq");
                    else if (strArray3[index2].IndexOf("OO") != -1)
                        strArray3[index2] = strArray3[index2].Replace("OO", "@@");
                    else if (strArray3[index2].IndexOf("U") != -1)
                        strArray3[index2] = strArray3[index2].Replace("U", "v");
                    else if (strArray3[index2].IndexOf("@") != -1)
                        strArray3[index2] = strArray3[index2].Replace("@", "q");
                    else if (strArray3[index2].IndexOf("O") != -1)
                        strArray3[index2] = strArray3[index2].Replace("O", "@");
                    if (strArray3[index2].IndexOf("N") != -1)
                        strArray3[index2] = strArray3[index2].Replace("N", "ng");
                    for (int index4 = 0; index4 <= 23; ++index4)
                    {
                        if (strArray3[index2].IndexOf(this.ThaiPV[index4]) != -1)
                        {
                            string[] separator = new string[1]
                            {
                                this.ThaiPV[index4]
                            };
                            string[] strArray2 = strArray3[index2].Split(separator, StringSplitOptions.None);
                            phoneme.Tone = strArray2[strArray2.Length - 1].Substring(strArray2[strArray2.Length - 1].Length - 1, 1);
                            if (strArray2[0].Length != 0 && !(strArray2[0] == "z"))
                            {
                                phoneme.Phoneme = strArray2[0];
                                phoneme.Pos = array[index3];
                                list1.Add(phoneme);
                            }
                            phoneme.Phoneme = this.ThaiPV[index4];
                            phoneme.Pos = array[index3];
                            list1.Add(phoneme);
                            if (strArray2[1].Length != 1 && !(strArray2[1].Substring(0, 1) == "z"))
                            {
                                phoneme.Phoneme = strArray2[1].Substring(0, strArray2[1].Length - 1) + "^";
                                phoneme.Pos = array[index3];
                                list1.Add(phoneme);
                            }
                            ++index3;
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    phoneme.Phoneme = "sil";
                    phoneme.Tone = "7";
                    phoneme.Pos = "0";
                    list1.Add(phoneme);
                }
            }
            phoneme.Phoneme = "sil";
            phoneme.Tone = "7";
            phoneme.Pos = "0";
            list1.Add(phoneme);
            list1.Add(phoneme);
            string str1 = "";
            for (int index = 2; index < list1.Count - 2; ++index)
                str1 = str1 + list1[index].Phoneme + ";" + list1[index].Tone + ";" + list1[index].Pos + "|";
            return str1;
        }

        public string Conversion3(string inp)
        {
            string[] strArray = this.C2Pronunciation(inp);
            string str1 = "";
            foreach (string str2 in strArray)
                str1 = str1 + str2 + " ";
            return str1;
        }

        public string IgnoreConversion(string inp)
        {
            return inp;
        }

        public string Conversion4(string inp)
        {
            string output = "sil;7;0|";
            char[] wordSplitter = new char[1] { ';' };
            foreach (string word in inp.Split(wordSplitter))
            {
                int count = 1;
                char[] sylSplitter = new char[1] { ' ' };
                foreach (string syl in word.Split(sylSplitter))
                {
                    char[] phonSplitter = new char[1] { '-' };
                    string[] phons = syl.Split(phonSplitter);
                    for (int i = 0; i < phons.Length - 1; ++i)
                    {
                        if (!phons[i].Equals("z") && !phons[i].Equals("z^"))
                            output = output + (object)phons[i] + ";" + phons[phons.Length - 1] + ";" + count.ToString() + "|";
                    }
                    ++count;
                }
            }
            return output + "sil;7;0|";
        }

        public string[] C2Pronunciation(string inp)
        {
            List<string> list = new List<string>();
            string str1 = inp;
            char[] chArray = new char[1]
            {
        ' '
            };
            foreach (string str2 in str1.Split(chArray))
            {
                string[] strArray1 = str2.Replace("?", "z").Replace("-", "z").Split("^'}~+\xF8C5".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                for (int index1 = 0; index1 < strArray1.Length; ++index1)
                {
                    if (strArray1[index1].IndexOf("UUa") != -1)
                        strArray1[index1] = strArray1[index1].Replace("UUa", "vva");
                    else if (strArray1[index1].IndexOf("Ua") != -1)
                        strArray1[index1] = strArray1[index1].Replace("Ua", "va");
                    else if (strArray1[index1].IndexOf("UU") != -1)
                        strArray1[index1] = strArray1[index1].Replace("UU", "vv");
                    else if (strArray1[index1].IndexOf("@@") != -1)
                        strArray1[index1] = strArray1[index1].Replace("@@", "qq");
                    else if (strArray1[index1].IndexOf("OO") != -1)
                        strArray1[index1] = strArray1[index1].Replace("OO", "@@");
                    else if (strArray1[index1].IndexOf("U") != -1)
                        strArray1[index1] = strArray1[index1].Replace("U", "v");
                    else if (strArray1[index1].IndexOf("@") != -1)
                        strArray1[index1] = strArray1[index1].Replace("@", "q");
                    else if (strArray1[index1].IndexOf("O") != -1)
                        strArray1[index1] = strArray1[index1].Replace("O", "@");
                    if (strArray1[index1].IndexOf("N") != -1)
                        strArray1[index1] = strArray1[index1].Replace("N", "ng");
                    for (int index2 = 0; index2 <= 23; ++index2)
                    {
                        if (strArray1[index1].IndexOf(this.ThaiPV[index2]) != -1)
                        {
                            string[] separator = new string[1]
                            {
                                this.ThaiPV[index2]
                            };
                            string[] strArray2 = strArray1[index1].Split(separator, StringSplitOptions.None);
                            string str3 = (strArray2[0].Length == 0 || strArray2[0] == "z" ? "z-" : strArray2[0] + "-") + this.ThaiPV[index2] + "-";
                            string str4 = (strArray2[1].Length == 1 || strArray2[1].Substring(0, 1) == "z" ? str3 + "z^-" : str3 + strArray2[1].Substring(0, strArray2[1].Length - 1) + "^-") + strArray1[index1].Substring(strArray1[index1].Length - 1);
                            list.Add(str4);
                            break;
                        }
                    }
                }
            }
            return list.ToArray();
        }

        public struct phoneme
        {
            public string Phoneme;
            public string Tone;
            public string Pos;
        }
    }
}