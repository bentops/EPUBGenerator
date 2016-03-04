using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;

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

        private List<int> textIdxList;
        private List<long> byteIdxList;

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

        // Return Total Bytes of Output Wav File.
        public long Synthesize(string inputText, string outputPath)
        {
            if (inputText == null || outputPath == null)
                throw new Exception("Null inputText or outputPath.");

            ClearTemp();

            textIdxList = new List<int> { 0 };
            byteIdxList = new List<long> { 0 };

            int startIndex = 0;
            long totalBytes = 0;
            WaveFormat waveFormat = null;
            List<string> waveList = new List<string>();

            // SPLIT INTO SENTENCES BY TYPE
            List<KeyValuePair<string, int>> sList = sentenceSplitter.Split(inputText);
            if (sList.Count == 0)
                sList.Add(new KeyValuePair<string, int>(inputText, 2));
            foreach (KeyValuePair<string, int> sPair in sList)
            {
                string text = sPair.Key;
                int type = sPair.Value;

                List<string> phonemeList = new List<string>();
                string longPhoneme = "";

                #region ----------- Gen Phoneme -----------
                List<KeyValuePair<string, string>> tList = g2p.GenTranscriptList(text, type);
                foreach (KeyValuePair<string, string> tPair in tList)
                {
                    string cutWord = tPair.Key;
                    string transcript = tPair.Value;
                    string phoneme = phonemeConverter.Convert(transcript, type);
                    if (type != 2)
                    {
                        if (phoneme.IndexOf(silence) == 0)
                            phoneme = phoneme.Substring(silence.Length);
                        int postIdx = phoneme.Length - silence.Length;
                        if (postIdx >= 0 && phoneme.IndexOf(silence, postIdx) == postIdx)
                            phoneme = phoneme.Substring(0, postIdx);
                        if (phoneme.Length == 0)
                            phoneme = silence;
                    }
                    phonemeList.Add(phoneme);

                    int index = inputText.IndexOf(cutWord, startIndex);
                    if (index < 0)
                        throw new Exception("WRONG TEXT INDEX: " + cutWord + " " + startIndex + " " + inputText);
                    startIndex = index + cutWord.Length;
                    textIdxList.Add(startIndex);
                }
                if (type != 2)
                {
                    foreach (string phoneme in phonemeList)
                        longPhoneme += phoneme;
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

                synthesizer.Synthesize(longPhoneme, type, randID, TempPath);

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
                if (type != 2)
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
                                    byteIdxList.Add(totalBytes + GetByteFromDur(line[1]));
                            }
                        }

                        string dur = streamReader.ReadLine().Split(' ')[1];
                        totalBytes += GetByteFromDur(dur);

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
                        byteIdxList.Add(totalBytes);
                    }
                }
                #endregion

                if (textIdxList.Count != byteIdxList.Count)
                    throw new Exception("Wrong Index & Byte Calculation: " + textIdxList.Count + ", " + byteIdxList.Count);
            }

            #region -------- Concat WavFile --------
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
            #endregion

            if (bytes != totalBytes)
                throw new Exception("Miscalculating Total Bytes.");
            return totalBytes;
        }

        public void Synthesize(List<string> inputList)
        {

        }

        public List<int> GetTextIndexList()
        {
            return textIdxList;
        }

        public List<long> GetByteIndexList()
        {
            return byteIdxList;
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
