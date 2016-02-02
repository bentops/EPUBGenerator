using Chula.SLS.TTS.C2SSegmentator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ChulaTTS.G2PConverter.G2P
{
    public class G2PTh
    {
        private Dictionary<string, List<KeyValuePair<string, string>>> DictFile;
        private C2SThaiWordSegmentator _twordsegment;

        public G2PTh()
        {
            DictFile = new Dictionary<string, List<KeyValuePair<string, string>>>();
            _twordsegment = new C2SThaiWordSegmentator();

            StreamReader streamReader = new StreamReader("dict.txt", Encoding.Default);
            string text;
            while ((text = streamReader.ReadLine()) != null)
            {
                if (text.Trim() != null)
                {
                    string phon = streamReader.ReadLine();
                    int len = text.Length;
                    string key = text.Substring(0, 1) + text.Substring(len - 1) + len;
                    if (DictFile.ContainsKey(key))
                        DictFile[key].Add(new KeyValuePair<string, string>(text, phon));
                    else
                    {
                        List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                        list.Add(new KeyValuePair<string, string>(text, phon));
                        DictFile.Add(key, list);
                    }
                }
            }
            streamReader.Close();
        }
        
        public string GenTranscript(string input)
        {
            int startIndex = 0;
            int length = input.Length;
            bool success = false;
            string output = "";
            while (length - startIndex > 1)
            {
                string subStr = input.Substring(startIndex, length - startIndex);
                int len = subStr.Length;
                string key = subStr.Substring(0, 1) + subStr.Substring(len - 1) + len;
                bool found = false;
                if (DictFile.ContainsKey(key))
                {
                    foreach (KeyValuePair<string, string> kvPair in DictFile[key])
                    {
                        if (kvPair.Key == subStr)
                        {
                            output = output + kvPair.Value + ";";
                            startIndex = length;
                            length = input.Length;
                            found = true;
                            if (startIndex == input.Length)
                                success = true;
                            break;
                        }
                    }
                }
                if (!found) --length;
            }
            if (!success)
            {
                string[] Segments = _twordsegment.Segment(input);
                foreach (String s in Segments) Console.WriteLine(s);
                return _twordsegment.ConvertPhoneme(Segments).Replace("|", " ");
            }
            return output.Replace("|", " ");
        }

        // MY NEW VERSION
        // List<{segmentedword, pronunciation, transcript}>
        public List<List<string>> GenTranscripts(string input)
        {
            List<List<string>> transcripts = new List<List<string>>();
            int startIndex = 0;
            int length = input.Length;
            bool success = false;
            while (length - startIndex > 1)
            {
                string subStr = input.Substring(startIndex, length - startIndex);
                int len = subStr.Length;
                string key = subStr.Substring(0, 1) + subStr.Substring(len - 1) + len;
                bool found = false;
                if (DictFile.ContainsKey(key))
                {
                    foreach (KeyValuePair<string, string> kvPair in DictFile[key])
                    {
                        if (kvPair.Key == subStr)
                        {
                            //transcript = transcript + kvPair.Value + ";";
                            string text = kvPair.Key;
                            string transcript = kvPair.Value.Replace('|', ' ');
                            transcripts.Add(new List<string>() { text, text, transcript });
                            startIndex = length;
                            length = input.Length;
                            found = true;
                            if (startIndex == input.Length)
                                success = true;
                            break;
                        }
                    }
                }
                if (!found) --length;
            }

            if (!success)
            {
                KeyValuePair<string, string[]>[] segmentedWords = _twordsegment.Segments(input);
                transcripts = new List<List<string>>();
                foreach (KeyValuePair<string, string[]> kv in segmentedWords)
                {
                    string text = kv.Key;
                    string pronun = "";
                    foreach (string pr in kv.Value)
                        pronun += pr;
                    string transcript = _twordsegment.ConvertPhoneme(kv.Value).Replace('|', ' ');
                    transcripts.Add(new List<string>() { text, pronun, transcript });
                }
            }
            return transcripts;
        }
    }
}