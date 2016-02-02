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

        private List<string> segmentedWords;
        private List<string> pronunciations;
        private List<string> transcripts;
        private bool cutDictSuccess;

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
        
        private void Process(string input)
        {
            segmentedWords = new List<string>();
            pronunciations = new List<string>();
            transcripts = new List<string>();
            cutDictSuccess = false;

            int startIndex = 0;
            int length = input.Length;
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
                            segmentedWords.Add(subStr);
                            pronunciations.Add(subStr);
                            transcripts.Add(kvPair.Value.Replace('|', ' '));
                            startIndex = length;
                            length = input.Length;
                            found = true;
                            if (startIndex == input.Length)
                                cutDictSuccess = true;
                            break;
                        }
                    }
                }
                if (!found) --length;
            }
            if (!cutDictSuccess)
            {
                segmentedWords = new List<string>();
                pronunciations = new List<string>();
                transcripts = new List<string>();

                KeyValuePair<string, string[]>[] segWords = _twordsegment.Segments(input);
                foreach (KeyValuePair<string, string[]> kv in segWords)
                {
                    segmentedWords.Add(kv.Key);
                    string pronun = "";
                    foreach (string pr in kv.Value)
                        pronun += pr;
                    pronunciations.Add(pronun);
                    transcripts.Add(_twordsegment.ConvertPhoneme(kv.Value).Replace('|', ' '));
                }
            }
        }

        // NEW Return transcript
        public string GenTranscript(string input)
        {
            Process(input);
            string transcript = "";
            foreach (string tr in transcripts)
                transcript += tr + (cutDictSuccess ? ";" : "");
            return transcript.Replace('|', ' ');
        }

        // NEW Return List<{segmentedWord, transcript}>
        public List<KeyValuePair<string, string>> GenTranscriptList(string input)
        {
            Process(input);
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            KeyValuePair<string, string> kvPair;
            for (int i = 0; i < segmentedWords.Count; i++)
            {
                kvPair = new KeyValuePair<string, string>(segmentedWords[i], transcripts[i]);
                list.Add(kvPair);
            }
            return list;
        }

        // NEW Return List<{segmentedword, pronunciation, transcript}>
        public List<List<string>> GenPronunciationAndTranscriptList(string input)
        {
            Process(input);
            List<List<string>> list = new List<List<string>>();
            List<string> item;
            for (int i = 0; i < segmentedWords.Count; i++)
            {
                item = new List<string>() { segmentedWords[i], pronunciations[i], transcripts[i] };
                list.Add(item);
            }
            return list;
        }

        /*
        Original
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
                return _twordsegment.ConvertPhoneme(Segments).Replace("|", " ");
            }
            return output.Replace("|", " ");
        }
        */
    }
}