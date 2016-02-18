using TTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NAudio.Wave;

namespace EPUBGenerator.MainLogic
{
    class Tools
    {
        public static SentenceSplitter SentenceSplitter { private get; set; }
        public static CPreprocessor Preprocessor { private get; set; }
        public static CG2P G2P { private get; set; }
        public static CPhonemeConverter PhonemeConverter { private get; set; }
        public static CSynthesizer Synthesizer { private get; set; }

        public static bool IsReady()
        {
            return Preprocessor != null && G2P != null && PhonemeConverter != null && Synthesizer != null;
        }

        public static List<KeyValuePair<String, int>> Split(String textBlock)
        {
            return SentenceSplitter.Split(textBlock);
        }

        public static LinkedList<Word> GenWordList(Sentence sentence)
        {
            LinkedList<Word> words = new LinkedList<Word>();
            String processedText = Preprocessor.Process(sentence.Text, sentence.Type);
            foreach (KeyValuePair<String, String> pair in G2P.GenTranscriptList(processedText, sentence.Type))
            {
                String text = pair.Key;
                String phoneme = PhonemeConverter.Convert(pair.Value, sentence.Type);

                if (sentence.Type != 2)
                {
                    if (phoneme.Length >= 8 && phoneme.Substring(0, 8).Equals(@"sil;7;0|"))
                        phoneme = phoneme.Substring(8);

                    int len = phoneme.Length - 8;
                    if (len >= 0 && phoneme.Substring(len).Equals(@"sil;7;0|"))
                        phoneme = phoneme.Substring(0, len);

                    if (phoneme.Length == 0)
                        phoneme = @"sil;7;0|";
                }

                Word word = new Word(text, phoneme, sentence);
                word.Node = words.AddLast(word);
            }
            return words;
        }

        public static void Synthesize(Content content, String audioPath, Project.ProgressUpdater updater)
        {
            foreach (Block block in content.Blocks)
            {
                foreach (Sentence sentence in block.Sentences)
                {
                    Synthesizer.Synthesize(sentence.Phoneme, sentence.Type, sentence.BSID, audioPath);
                    
                    if (sentence.Type != 2)
                    {
                        String durFile = Path.Combine(Synthesizer.TempPath, sentence.BSID + ".dur");
                        using (StreamReader streamReader = new StreamReader(durFile))
                        {
                            streamReader.ReadLine();

                            foreach (Word word in sentence.Words)
                            {
                                int count = word.Phoneme.Split("|".ToCharArray()).Length - 1;
                                int wBegin = 0;
                                int wEnd = 0;

                                #region Calculating Byte Position
                                /*
                                Sampling Nth
                                 = Time / 1Sec * Freq
                                 = Time / 10000000 * 16000
                                 = Time / 625

                                Byte Mth
                                 = Nth * (BitPerSampling / 8)
                                 = Nth * (16 / 8)
                                 = Nth * 2
                                */
                                #endregion
                                for (int i = 0; i < count; i++)
                                {
                                    String[] line = streamReader.ReadLine().Split(" ".ToCharArray());
                                    if (i == 0)
                                        wBegin = int.Parse(line[0]) * 2 / 625;
                                    else if (i == count - 1)
                                        wEnd = int.Parse(line[1]) * 2 / 625;
                                }
                                word.SetPosition(wBegin, wEnd);
                            }
                            String dur = streamReader.ReadLine().Split(" ".ToCharArray())[1];
                            sentence.Bytes = int.Parse(dur) * 2 / 625;

                            streamReader.Close();
                        }
                    }
                    else if (sentence.Type == 2)
                    {
                        String audioFile = Path.Combine(audioPath, sentence.BSID + ".wav");
                        using (WaveFileReader waveFileReader = new WaveFileReader(audioFile))
                        {
                            sentence.Bytes = (int)waveFileReader.Length;
                            sentence.Words.First.Value.SetPosition(44, sentence.Bytes);
                        }
                    }

                    updater.UpCount();
                    if (updater.Cancelled)
                        return;
                }
            }
        }








        public static String GetPhoneme(String input, int type)
        {
            input = Preprocessor.Process(input, type);
            input = G2P.GenTranscript(input, type);
            input = PhonemeConverter.Convert(input, type);
            return input;
        }

        public static void Synthesize(String input, int type, string id)
        {
            Synthesizer.Synthesize(input, type, id, "");
        }
    }
}
