using System;

using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Chula.SLS.TTS.C2SSegmentator;

namespace ThaiSpeechSynthesizer
{
    class WordStructureThaiDictRule : IWordStructureRule
    {
        struct StringWrapper
        {
            byte[] _bytes;

            public byte[] Bytes
            {
                get { return _bytes; }
                set { _bytes = value; }
            }

            public StringWrapper(byte[] bytes)
            {
                _bytes = bytes;
            }

            public override string ToString()
            {
                return Encoding.GetEncoding("windows-874").GetString(_bytes, 0, _bytes.Length);
            }
        }
        //struct StringWrapper
        //{
        //    int _startIndex;
        //    byte _length;

        //    public int StartIndex
        //    {
        //        get { return _startIndex; }
        //        set { _startIndex = value; }
        //    }
        //    public byte Length
        //    {
        //        get { return _length; }
        //        set { _length = value; }
        //    }

        //    public StringWrapper(int startIndex, byte length)
        //    {
        //        _startIndex = startIndex;
        //        _length = length;
        //    }

        //    public override string ToString()
        //    {
        //        FileStream fileStream = File.OpenRead(ThaiDictFileName);
        //        StreamReader streamReader = new StreamReader(fileStream, Encoding.GetEncoding("windows-874"));
        //        fileStream.Position = _startIndex;
        //        char[] charArray = new char[_length];
        //        streamReader.Read(charArray, 0, charArray.Length);
        //        streamReader.Close();
        //        return new string(charArray);
        //    }
        //    public string ToString(FileStream fileStream)
        //    {
        //        StreamReader streamReader = new StreamReader(fileStream, Encoding.GetEncoding("windows-874"));
        //        fileStream.Position = _startIndex;
        //        char[] charArray = new char[_length];
        //        streamReader.Read(charArray, 0, charArray.Length);
        //        return new string(charArray);
        //    }
        //}
        //class ThaiDictFileWrapper
        //{
        //    FileStream _fileStream;

        //    public FileStream FileStream
        //    {
        //        get { return _fileStream; }
        //        set { _fileStream = value; }
        //    }

        //    public KeyValuePair<StringWrapper, StringWrapper> this[int index]
        //    {
        //        get
        //        {
        //            StringWrapper word;
        //            StringWrapper pronunciation;
        //            _fileStream.Position = index * 6;
        //            byte[] startIndexBytes = new byte[4];
        //            _fileStream.Read(startIndexBytes, 0, 4);
        //            int startIndex = BitConverter.ToInt32(startIndexBytes, 0);
        //            byte wordLength = (byte)_fileStream.ReadByte();
        //            byte pronunciationLength = (byte)_fileStream.ReadByte();
        //            word = new StringWrapper(startIndex, wordLength);
        //            pronunciation = new StringWrapper(startIndex + wordLength + 1, pronunciationLength);
        //            return new KeyValuePair<StringWrapper, StringWrapper>(word, pronunciation);
        //        }
        //    }
        //    public int Count
        //    {
        //        get { return (int)(_fileStream.Length / 10); }
        //    }
        //}

        static WordStructureThaiDictRule SingletonInstance = null;

        internal static WordStructureThaiDictRule Instance
        {
            get
            {
                if (SingletonInstance == null)
                    SingletonInstance = new WordStructureThaiDictRule();
                return WordStructureThaiDictRule.SingletonInstance;
            }
        }

        static readonly string ThaiDictFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + @"\Dicts\ThaiWordDict.txt";
        static readonly string ThaiDictIndexFileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName) + @"\Dicts\ThaiWordDict.idx";

        //List<KeyValuePair<string, string>> _thaiDictList = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<StringWrapper, StringWrapper>> _thaiDictList = new List<KeyValuePair<StringWrapper, StringWrapper>>();
        //ThaiDictFileWrapper _thaiDictList = new ThaiDictFileWrapper();
        bool _isInitialized = false;

        public bool IsInitialized
        {
            get { return _isInitialized; }
            private set { _isInitialized = value; }
        }

        private WordStructureThaiDictRule() { }

        public void InitializeThaiDict()
        {
            //int lastCommaIndex = 0;
            //int startIndex = 0;
            //int length = 0;
            //for (int count = 0; count < Properties.Resources.ThaiWordDict.Length; count++)
            //{
            //    char ch = Properties.Resources.ThaiWordDict[count];
            //    if (ch == ',')
            //        lastCommaIndex = count;
            //    if (IsThaiChar(ch) || ch == ',' || ch == '-')
            //        length++;
            //    else
            //    {
            //        if (length > 0)
            //        {
            //            StringWrapper word = new StringWrapper(startIndex, lastCommaIndex - startIndex);
            //            StringWrapper pronunciation = new StringWrapper(lastCommaIndex + 1, count - (lastCommaIndex + 1));
            //            _thaiDictList.Add(new KeyValuePair<StringWrapper, StringWrapper>(word, pronunciation));
            //        }
            //        startIndex = count + 1;
            //        length = 0;
            //    }
            //}
            if (File.Exists(ThaiDictFileName))
            {
                //int startIndex = 0;
                Encoding thaiEncoding = Encoding.GetEncoding("windows-874");
                StreamReader streamReader = new StreamReader(ThaiDictFileName, thaiEncoding);
                //FileStream indexFileStream = File.OpenWrite(ThaiDictIndexFileName);
                while (!streamReader.EndOfStream)
                {
                    string s = streamReader.ReadLine();
                    string[] strArray = s.Split(',');
                    if (strArray.Length == 2)
                    {
                        StringWrapper word = new StringWrapper(thaiEncoding.GetBytes(strArray[0]));
                        StringWrapper pronunciation = new StringWrapper(thaiEncoding.GetBytes(strArray[1]));
                        _thaiDictList.Add(new KeyValuePair<StringWrapper, StringWrapper>(word, pronunciation));

                        //_thaiDictList.Add(new KeyValuePair<string, string>(strArray[0], strArray[1]));

                        //StringWrapper word = new StringWrapper(startIndex, (byte)strArray[0].Length);
                        //StringWrapper pronunciation = new StringWrapper(startIndex + strArray[0].Length + 1, (byte)strArray[1].Length);
                        //_thaiDictList.Add(new KeyValuePair<StringWrapper, StringWrapper>(word, pronunciation));

                        //byte[] bytes;
                        //bytes = BitConverter.GetBytes(startIndex);
                        //indexFileStream.Write(bytes, 0, bytes.Length);
                        //indexFileStream.WriteByte((byte)strArray[0].Length);
                        //indexFileStream.WriteByte((byte)strArray[1].Length);
                    }
                    //startIndex += s.Length + 2;
                }
                //indexFileStream.Close();
                streamReader.Close();
            } else{
                throw new C2SSegmentatorException("Dictionary not found. Using rule method only.");                
            }
            this.IsInitialized = true;
        }

        public Dictionary<string, string[]> GetMatchedPrefix(string word, string[] prePronunciation)
        {
            if (!this.IsInitialized)
                this.InitializeThaiDict();

            //FileStream thaiDictFileStream = File.OpenRead(ThaiDictFileName);
            //FileStream indexFileStream = File.OpenRead(ThaiDictIndexFileName);
            //_thaiDictList.FileStream = indexFileStream;
            Dictionary<string, string[]> matchedPrefix = new Dictionary<string, string[]>();
            string subWord = "";
            int wordIndex = 0;
            foreach (char ch in word)
            {
                subWord += ch;
                wordIndex = this.BinarySearch(subWord, wordIndex, _thaiDictList.Count - 1);
                if (wordIndex >= _thaiDictList.Count)
                    break;
                string foundThaiWord = _thaiDictList[wordIndex].Key.ToString();
                if (foundThaiWord.Length < subWord.Length || foundThaiWord.Substring(0, subWord.Length) != subWord)
                    break;
                if (foundThaiWord == subWord)
                    matchedPrefix.Add(foundThaiWord, _thaiDictList[wordIndex].Value.ToString().Split('-'));
            }
            //thaiDictFileStream.Close();
            //indexFileStream.Close();
            return matchedPrefix;
        }

        private static bool IsThaiChar(char ch)
        {
            return ch >= 'ก' && ch <= "อ๎"[1];
        }

        private int BinarySearch(string searchingWord, int low, int hi)
        {
            int mid;
            while (low <= hi)
            {
                mid = (low + hi) / 2;
                int compareResult = ThaiStringCompare(searchingWord, _thaiDictList[mid].Key.ToString());
                if (compareResult == 0)
                    return mid;
                else if (compareResult > 0)
                    low = mid + 1;
                else if (compareResult < 0)
                    hi = mid - 1;
            }
            return low;
        }
        private static int ThaiStringCompare(string x, string y)
        {
            for (int count = 0; count < x.Length && count < y.Length; count++)
                if (x[count] < y[count])
                    return -1;
                else if (x[count] > y[count])
                    return 1;
            if (x.Length < y.Length)
                return -1;
            else if (x.Length > y.Length)
                return 1;
            else
                return 0;
        }
    }
}
