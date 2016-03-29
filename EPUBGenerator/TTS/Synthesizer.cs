using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using PairSI = System.Collections.Generic.KeyValuePair<string, int>;
using PairSS = System.Collections.Generic.KeyValuePair<string, string>;

namespace TTS
{
    public class Synthesizer
    {
        private SentenceSplitter sentenceSplitter;
        private CPreprocessor preprocessor; // NOT USE ANYMORE
        private CG2P g2p;
        private CPhonemeConverter phonemeConverter;
        private CSynthesizer synthesizer;
        private static string silence = @"sil;7;0|";
        
        public List<int> TextIndexList { get; private set; }
        public List<long> ByteIndexList { get; private set; }

        public string TempPath
        {
            get { return synthesizer.TempPath; }
            set { synthesizer.TempPath = value; }
        }

        public Synthesizer()
        {
            sentenceSplitter = new SentenceSplitter();
            preprocessor = new CPreprocessor();
            g2p = new CG2P();
            phonemeConverter = new CPhonemeConverter();
            synthesizer = new CSynthesizer();
        }


        private List<PairSI> Split(string input)
        {
            List<PairSI> output = new List<PairSI>();
            int offset = 0;
            foreach (PairSI pair in sentenceSplitter.Split(input))
            {
                string text = pair.Key;

                int index = input.IndexOf(text, offset);
                if (index < 0)
                    throw new Exception("Mismatch - -");

                if (offset < index)
                    output.Add(new PairSI(input.Substring(offset, index - offset), 6));
                output.Add(pair);
                offset = index + text.Length;
            }
            if (offset < input.Length)
                output.Add(new PairSI(input.Substring(offset), 6));
            return output;
        }


        // Return Total Bytes of Output Wav File.
        public long Synthesize(string inputText, string outputPath)
        {
            if (inputText == null || outputPath == null)
                throw new Exception("Null inputText or outputPath.");

            ClearTemp();

            TextIndexList = new List<int> { 0 };
            ByteIndexList = new List<long> { 0 };

            int startIndex = 0;
            long totalBytes = 0;
            WaveFormat waveFormat = null;
            List<string> waveList = new List<string>();

            // SPLIT INTO SENTENCES BY TYPE
            List<PairSI> sList = Split(inputText);
            if (sList.Count == 0)
                sList.Add(new PairSI(" ", 6));
            foreach (PairSI sPair in sList)
            {
                string text = sPair.Key;
                int type = sPair.Value;
                Console.WriteLine(text + "/" + type);

                List<string> phonemeList = new List<string>();
                string longPhoneme = "";

                #region ----------- Gen Phoneme -----------
                List<PairSS> tList = g2p.GenTranscriptList(text, type);
                foreach (PairSS tPair in tList)
                {
                    string cutWord = tPair.Key;
                    string transcript = tPair.Value;
                    string phoneme = phonemeConverter.Convert(transcript, type);
                    Console.WriteLine("Phon: " + phoneme + " Trans: " + transcript + " Word: " + cutWord);
                    if (type != 2 && type != 6)
                        phoneme = TrimPhoneme(phoneme);
                    phonemeList.Add(phoneme);

                    int index = inputText.IndexOf(cutWord, startIndex);
                    if (index < 0)
                        throw new Exception("WRONG TEXT INDEX: " + cutWord + " " + startIndex + " " + inputText);
                    startIndex = index + cutWord.Length;
                    TextIndexList.Add(startIndex);
                }
                if (type != 2)
                {
                    foreach (string phoneme in phonemeList)
                        longPhoneme += phoneme;
                    if (type != 6)
                        longPhoneme = silence + longPhoneme + silence;
                }
                else
                {
                    if (phonemeList.Count == 0)
                        longPhoneme = " ";
                    else
                        longPhoneme = phonemeList[0];
                }
                #endregion

                #region ------------ Gen Sound ------------
                string wavPath = SoundSynthesize(longPhoneme, type);
                string durPath = Path.ChangeExtension(wavPath, ".dur");

                // Check wheter every WavFiles has the same wav-format.
                waveList.Add(wavPath);
                using (WaveFileReader waveFileReader = new WaveFileReader(wavPath))
                {
                    if (waveFormat == null)
                        waveFormat = waveFileReader.WaveFormat;
                    else if (!waveFormat.Equals(waveFileReader.WaveFormat))
                        throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                }

                // Get Duration
                if (type != 2 && type != 6)
                {
                    using (StreamReader streamReader = new StreamReader(durPath))
                    {
                        streamReader.ReadLine(); // First SIL
                        foreach (string phoneme in phonemeList)
                        {
                            int sIndex = 0;
                            int tIndex = 0;
                            while ((tIndex = phoneme.IndexOf('|', sIndex)) >= 0)
                            {
                                string[] line = streamReader.ReadLine().Split(' ');
                                string durPhon = line[2].Split("-+".ToCharArray())[1];
                                if (phoneme.IndexOf(durPhon, sIndex) != sIndex)
                                    throw new Exception("Mismatch phoneme and duration");
                                sIndex = tIndex + 1;

                                if (tIndex == phoneme.Length - 1)
                                    ByteIndexList.Add(totalBytes + GetByteFromDur(line[1]));
                            }
                        }

                        string dur = streamReader.ReadLine().Split(' ')[1];
                        totalBytes += GetByteFromDur(dur);

                        if (!streamReader.EndOfStream)
                            throw new Exception("Invalid Duration.");
                        streamReader.Close();
                    }
                }
                else if (type == 6)
                {
                    using (StreamReader streamReader = new StreamReader(durPath))
                    {
                        foreach (string phoneme in phonemeList)
                        {
                            int sIndex = 0;
                            int tIndex = 0;
                            while ((tIndex = phoneme.IndexOf('|', sIndex)) >= 0)
                            {
                                string[] line = streamReader.ReadLine().Split(' ');
                                string durPhon = line[2].Split("-+".ToCharArray())[1];
                                if (phoneme.IndexOf(durPhon, sIndex) != sIndex)
                                    throw new Exception("Mismatch phoneme and duration");
                                sIndex = tIndex + 1;

                                if (tIndex == phoneme.Length - 1)
                                {
                                    ByteIndexList.Add(totalBytes + GetByteFromDur(line[1]));
                                    totalBytes += GetByteFromDur(line[1]);
                                }
                            }
                        }
                        if (!streamReader.EndOfStream)
                            throw new Exception("Invalid Duration.");
                        streamReader.Close();
                    }
                }
                else
                {
                    using (WaveFileReader waveFileReader = new WaveFileReader(wavPath))
                    {
                        totalBytes += waveFileReader.Length;
                        ByteIndexList.Add(totalBytes);
                    }
                }
                #endregion

                if (TextIndexList.Count != ByteIndexList.Count)
                    throw new Exception("Wrong Index & Byte Calculation: " + TextIndexList.Count + ", " + ByteIndexList.Count);
            }

            int bytes = ConcatWavFiles(waveList, waveFormat, outputPath);

            if (bytes != totalBytes)
                throw new Exception("Miscalculating Total Bytes.");
            return totalBytes;
        }

        private string TrimPhoneme(string phoneme)
        {
            if (phoneme.IndexOf(silence) == 0)
                phoneme = phoneme.Substring(silence.Length);
            int postIdx = phoneme.Length - silence.Length;
            if (postIdx >= 0 && phoneme.IndexOf(silence, postIdx) == postIdx)
                phoneme = phoneme.Substring(0, postIdx);
            if (phoneme.Length == 0)
                phoneme = silence;
            return phoneme;
        }
        private string SoundSynthesize(string phoneme, int type)
        {
            string randID = "";
            string wavPath = "";
            string durPath = "";
            do
            {
                Random random = new Random();
                randID = random.Next(1000000).ToString("D6");
                wavPath = Path.Combine(TempPath, randID + ".wav");
                durPath = Path.Combine(TempPath, randID + ".dur");
            } while (File.Exists(wavPath) || File.Exists(durPath));

            synthesizer.Synthesize(phoneme, type, randID, TempPath);
            Console.WriteLine("Synthesizing: " + phoneme + ": " + randID);
            return wavPath;
        }
        private int ConcatWavFiles(List<string> waveList, WaveFormat waveFormat, string outputPath)
        {
            int bytes = 0;
            if (waveFormat != null)
            {
                byte[] buffer = new byte[1024];
                int read;
                using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputPath, waveFormat))
                {
                    foreach (string sourceFile in waveList)
                    {
                        using (WaveFileReader waveFileReader = new WaveFileReader(sourceFile))
                        {
                            while ((read = waveFileReader.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                waveFileWriter.Write(buffer, 0, read);
                                bytes += read;
                            }
                        }
                    }
                }
            }

            return bytes;
        }

        public long Synthesize(List<List<string>> sentence, string outputPath)
        {
            Console.WriteLine("-----------------------------------------");
            ByteIndexList = new List<long>() { 0 };
            List<List<PairSI>> wordPhonemes = new List<List<PairSI>>();
            foreach (List<string> word in sentence)
            {
                List<PairSI> subPhonemes = new List<PairSI>();
                foreach (string syllable in word)
                    GetSubPhoneme(syllable, subPhonemes);
                wordPhonemes.Add(subPhonemes);
            }

            List<PairSI> nPhonemes = NormalizePhoneme(wordPhonemes);
            long totalBytes = 0;
            int curWord = 0;
            int curSubWord = 0;
            WaveFormat waveFormat = null;
            List<string> waveList = new List<string>();
            foreach (PairSI phoneme in nPhonemes)
            {
                int type = phoneme.Value;
                string wavPath = SoundSynthesize(phoneme.Key, phoneme.Value);
                string durPath = Path.ChangeExtension(wavPath, ".dur");
                
                // Check wheter every WavFiles has the same wav-format.
                waveList.Add(wavPath);
                using (WaveFileReader waveFileReader = new WaveFileReader(wavPath))
                {
                    if (waveFormat == null)
                        waveFormat = waveFileReader.WaveFormat;
                    else if (!waveFormat.Equals(waveFileReader.WaveFormat))
                        throw new InvalidOperationException("Can't concatenate WAV Files that don't share the same format");
                }

                #region Get Duration
                if (type == 1)
                {
                    using (StreamReader streamReader = new StreamReader(durPath))
                    {
                        string lastDur = "0";
                        while (!streamReader.EndOfStream)
                        {
                            if (wordPhonemes[curWord][curSubWord].Value != type)
                                throw new Exception("MisAlignment");
                            string phon = wordPhonemes[curWord][curSubWord].Key;
                            int sIndex = 0;
                            int tIndex = 0;
                            while ((tIndex = phon.IndexOf('|', sIndex)) >= 0)
                            {
                                string[] line = streamReader.ReadLine().Split(' ');
                                string durPhon = line[2].Split("-+".ToCharArray())[1];
                                if (phon.IndexOf(durPhon, sIndex) != sIndex)
                                    throw new Exception("Mismatch phoneme and duration");
                                sIndex = tIndex + 1;
                                lastDur = line[1];
                            }
                            if (++curSubWord >= wordPhonemes[curWord].Count)
                            {
                                ByteIndexList.Add(totalBytes + GetByteFromDur(lastDur));
                                curWord++;
                                curSubWord = 0;
                            }
                        }
                        streamReader.Close();
                        totalBytes += GetByteFromDur(lastDur);
                    }
                }
                else
                {
                    using (WaveFileReader waveFileReader = new WaveFileReader(wavPath))
                    {
                        totalBytes += waveFileReader.Length;
                        waveFileReader.Close();
                    }
                    if (++curSubWord >= wordPhonemes[curWord].Count)
                    {
                        ByteIndexList.Add(totalBytes);
                        curWord++;
                        curSubWord = 0;
                    }
                }
                #endregion
            }

            int bytes = ConcatWavFiles(waveList, waveFormat, outputPath);

            if (bytes != totalBytes)
                throw new Exception("Miscalculating Total Bytes.");
            return totalBytes;
        }

        private List<PairSI> NormalizePhoneme(List<List<PairSI>> wordPhonemes)
        {
            List<PairSI> nPhoneme = new List<PairSI>();
            int oldType = 2;
            for (int i = 0; i < wordPhonemes.Count; i++)
            {
                List<PairSI> subPhonemes = wordPhonemes[i];
                string fText = subPhonemes[0].Key;
                int fType = subPhonemes[0].Value;

                if (fType == 1)
                    if (fType == oldType)
                    {
                        int last = nPhoneme.Count - 1;
                        PairSI lastPhon = nPhoneme[last];
                        nPhoneme[last] = new PairSI(lastPhon.Key + fText, fType);
                    }
                    else
                    {
                        subPhonemes[0] = new PairSI(silence + fText, fType);
                        nPhoneme.Add(subPhonemes[0]);
                    }
                else
                {
                    if (fType != oldType)
                    {
                        int last = nPhoneme.Count - 1;
                        PairSI lastPhon = nPhoneme[last];
                        nPhoneme[last] = new PairSI(lastPhon.Key + silence, oldType);

                        int lastSubWordIdx = wordPhonemes[i - 1].Count - 1;
                        PairSI lastWord = wordPhonemes[i - 1][lastSubWordIdx];
                        wordPhonemes[i - 1][lastSubWordIdx] = new PairSI(lastWord.Key + silence, oldType);
                    }
                    nPhoneme.Add(subPhonemes[0]);
                }
                for (int j = 1; j < subPhonemes.Count; j++)
                    nPhoneme.Add(subPhonemes[j]);
                oldType = nPhoneme[nPhoneme.Count - 1].Value;
            }
            int lastPhonIdx = nPhoneme.Count - 1;
            PairSI phon = nPhoneme[lastPhonIdx];
            if (phon.Value == 1)
            {
                nPhoneme[lastPhonIdx] = new PairSI(phon.Key + silence, phon.Value);
                int lastWordIdx = wordPhonemes.Count - 1;
                int lastSubWordIdx = wordPhonemes[lastWordIdx].Count - 1;
                PairSI lastWord = wordPhonemes[lastWordIdx][lastSubWordIdx];
                wordPhonemes[lastWordIdx][lastSubWordIdx] = new PairSI(lastWord.Key + silence, lastWord.Value);
            }
            return nPhoneme;
        }
        
        private void GetSubPhoneme(string syllable, List<PairSI> subPhonemes)
        {
            // SPLIT BY TYPE
            List<PairSI> sList = sentenceSplitter.Split(syllable);
            if (sList.Count == 0)
                sList.Add(new PairSI(syllable, 2));
            foreach (PairSI pair in sList)
            {
                string text = pair.Key;
                int type = pair.Value;
                string phoneme = ConvertToPhoneme(text, type);
                if (type != 2)
                {
                    phoneme = TrimPhoneme(phoneme);
                    type = 1;
                }
                else
                    phoneme += " ";
                int last = subPhonemes.Count - 1;
                if (last < 0)
                    subPhonemes.Add(new PairSI(phoneme, type));
                else
                {
                    PairSI lastPhon = subPhonemes[last];
                    if (lastPhon.Value == type)
                    {
                        phoneme = lastPhon.Key + phoneme;
                        subPhonemes.RemoveAt(last);
                    }
                    else if (type == 2)
                    {
                        subPhonemes.RemoveAt(last);
                        subPhonemes.Add(new PairSI(lastPhon.Key + silence, lastPhon.Value));
                    }
                    else
                        phoneme = silence + phoneme;
                    subPhonemes.Add(new PairSI(phoneme, type));
                }
            }
        }
        
        private string ConvertToPhoneme(string text, int type)
        {
            string tmp = g2p.GenTranscript(text, type);
            Console.WriteLine(">>>" + tmp);
            return phonemeConverter.Convert(tmp, type);
        }


        private static long GetByteFromDur(string dur)
        {
            #region Calculating Byte Position
            // Sampling Nth
            //  = Time / 1Sec * Freq
            //  = Time / 10000000 * 16000
            //  = Time / 625

            // Byte Mth
            //  = Nth * (BitPerSampling / 8)
            //  = Nth * (16 / 8)
            //  = Nth * 2
            #endregion
            return long.Parse(dur) * 2 / 625;
        }
        private void ClearTemp()
        {
            if (TempPath != Path.GetTempPath())
            {
                foreach (string file in Directory.GetFiles(TempPath))
                {
                    switch (Path.GetExtension(file))
                    {
                        case ".wav":
                        case ".dur":
                        case ".lab":
                            File.Delete(file);
                            break;
                    }
                }
            }
        }
    }
}
