using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using TTS.Synthesizers;

namespace TTS
{
    public class SynthesizerBlock
    {
        private Dictionary<int, int> SynthesizerReturnType;
        private Dictionary<int, int> SynthesizerMap;
        private byte[] CurDat;
        private int _frequency;
        private double _pitch;
        private double _speed;
        // ---------------------
        private Dictionary<int, ISynthesizer> Synthesizer;
        // ---------------------

        public SynthesizerBlock(string fname)
        {
            // ------------------
            Synthesizer = new Dictionary<int, ISynthesizer>();
            Synthesizer.Add(1, new Alpha1());

            SynthesizerReturnType = new Dictionary<int, int>();
            SynthesizerReturnType.Add(1, 1);

            SynthesizerMap = new Dictionary<int, int>();
            SynthesizerMap.Add(1, 1);
            SynthesizerMap.Add(2, 2);
            SynthesizerMap.Add(3, 1);
            SynthesizerMap.Add(4, 1);
            SynthesizerMap.Add(5, 1);

            this._frequency = 16000;
            this._pitch = 1.0;
            this._speed = 1.0;
        }

        public void Reset()
        {
            foreach (KeyValuePair<int, ISynthesizer> Syn in Synthesizer)
                Syn.Value.Dispose();
        }

        public void SetFrequency(int frequency)
        {
            foreach (KeyValuePair<int, ISynthesizer> Syn in Synthesizer)
                Syn.Value.SetFrequency(frequency);
            this._frequency = frequency;
        }

        public void SetPitch(double pitch)
        {
            foreach (KeyValuePair<int, ISynthesizer> Syn in Synthesizer)
                Syn.Value.SetPitch(pitch);
            this._pitch = pitch;
        }

        public void SetSpeed(double speed)
        {
            foreach (KeyValuePair<int, ISynthesizer> Syn in Synthesizer)
                Syn.Value.SetSpeed(speed);
            this._speed = speed;
        }
        /*
        public void Synthesis(List<KeyValuePair<string, int>> inp)
        {
            long num1 = 0;
            int index1 = -1;
            List<KeyValuePair<string, int>> list1 = new List<KeyValuePair<string, int>>();
            List<byte[]> list2 = new List<byte[]>();
            byte[] numArray1 = new byte[1];
            foreach (KeyValuePair<string, int> keyValuePair in inp)
            {
                if (index1 == -1)
                    index1 = this.SynthesizerMap[keyValuePair.Value];
                else if (index1 != this.SynthesizerMap[keyValuePair.Value])
                {
                    if (this.SynthesizerReturnType[index1] == 1)
                    {
                        numArray1 = Synthesizer[index1].Synthesize(list1);
                        num1 += (long)numArray1.Length;
                    }
                    #region
                    if (this.SynthesizerReturnType[index1] == 2)
                    {
                        // OHHHHHHHHHH WE GOT PROBLEM HEREEEEEEEEEEEEEEEEEEEEE
                        string str = (string)(object)Synthesizer[index1].Synthesize(list1);
                        FileInfo fileInfo = new FileInfo(str);
                        bool flag = false;
                        while (!flag)
                        {
                            try
                            {
                                using (BinaryReader binaryReader = new BinaryReader((Stream)File.Open(str, FileMode.Open)))
                                {
                                    binaryReader.ReadBytes(44);
                                    num1 = num1 + fileInfo.Length - 44L;
                                    numArray1 = binaryReader.ReadBytes(Convert.ToInt32(fileInfo.Length - 44L));
                                    binaryReader.Close();
                                }
                                flag = true;
                            }
                            catch
                            {
                                Thread.Sleep(100);
                            }
                        }
                    }
                    #endregion
                    list2.Add(numArray1);
                    list1.Clear();
                    index1 = this.SynthesizerMap[keyValuePair.Value];
                }
                list1.Add(keyValuePair);
            }
            if (this.SynthesizerReturnType[index1] == 1)
            {
                numArray1 = Synthesizer[index1].Synthesize(list1);
                num1 += (long)numArray1.Length;
            }
            if (this.SynthesizerReturnType[index1] == 2)
            {
                // OHHHHHHHHHHHHHH WE GOT PROBLEM HEREEEEEEEEEEEEEEEE
                string str = (string)(object)Synthesizer[index1].Synthesize(list1);
                FileInfo fileInfo = new FileInfo(str);
                bool flag = false;
                while (!flag)
                {
                    try
                    {
                        using (BinaryReader binaryReader = new BinaryReader((Stream)File.Open(str, FileMode.Open)))
                        {
                            binaryReader.ReadBytes(44);
                            num1 = num1 + fileInfo.Length - 44L;
                            numArray1 = binaryReader.ReadBytes(Convert.ToInt32(fileInfo.Length - 44L));
                            binaryReader.Close();
                        }
                        flag = true;
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            list2.Add(numArray1);
            this.CurDat = new byte[num1 + 44L];
            int index2 = 44;
            foreach (byte[] numArray2 in list2)
            {
                numArray2.CopyTo((Array)this.CurDat, index2);
                index2 += numArray2.Length;
            }
            char[] chArray1 = "RIFF".ToCharArray(0, 4);
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3] = (byte)chArray1[index3];
            uint num2 = Convert.ToUInt32(num1 + 36L);
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3 + 4] = (byte)(num2 >> index3 * 8 & (uint)byte.MaxValue);
            char[] chArray2 = "WAVE".ToCharArray(0, 4);
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3 + 8] = (byte)chArray2[index3];
            char[] chArray3 = "fmt ".ToCharArray(0, 4);
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3 + 12] = (byte)chArray3[index3];
            uint num3 = 16;
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3 + 16] = (byte)(num3 >> index3 * 8 & (uint)byte.MaxValue);
            ushort num4 = 1;
            for (int index3 = 0; index3 <= 1; ++index3)
                this.CurDat[index3 + 20] = (byte)((int)num4 >> index3 * 8 & (int)byte.MaxValue);
            ushort num5 = 1;
            for (int index3 = 0; index3 <= 1; ++index3)
                this.CurDat[index3 + 22] = (byte)((int)num5 >> index3 * 8 & (int)byte.MaxValue);
            uint num6 = Convert.ToUInt32(this._frequency);
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3 + 24] = (byte)(num6 >> index3 * 8 & (uint)byte.MaxValue);
            uint num7 = Convert.ToUInt32(this._frequency * 2);
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3 + 28] = (byte)(num7 >> index3 * 8 & (uint)byte.MaxValue);
            ushort num8 = 2;
            for (int index3 = 0; index3 <= 1; ++index3)
                this.CurDat[index3 + 32] = (byte)((int)num8 >> index3 * 8 & (int)byte.MaxValue);
            ushort num9 = 16;
            for (int index3 = 0; index3 <= 1; ++index3)
                this.CurDat[index3 + 34] = (byte)((int)num9 >> index3 * 8 & (int)byte.MaxValue);
            char[] chArray4 = "data".ToCharArray(0, 4);
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3 + 36] = (byte)chArray4[index3];
            uint num10 = Convert.ToUInt32(num1);
            for (int index3 = 0; index3 <= 3; ++index3)
                this.CurDat[index3 + 40] = (byte)(num10 >> index3 * 8 & (uint)byte.MaxValue);
        }
        */
        public List<KeyValuePair<int, String>> GetSynthesizer()
        {
            List<KeyValuePair<int, String>> SynList = new List<KeyValuePair<int, String>>();
            foreach (KeyValuePair<int, ISynthesizer> Syn in Synthesizer)
                SynList.Add(new KeyValuePair<int, String>(Syn.Key, Syn.Value.About()));
            return SynList;
        }

        public List<String> GetModel(int SynSlot)
        {
            return Synthesizer[SynSlot].GetModel();
        }

        public void SetModel(int SynSlot, string ModelName)
        {
            Synthesizer[SynSlot].SetModel(ModelName);
        }

        public void PlaySound()
        {
            new SoundPlayer((Stream)new MemoryStream(this.CurDat)).PlaySync();
        }

        public void SaveFile(string fname)
        {
            using (BinaryWriter binaryWriter = new BinaryWriter((Stream)File.Open(fname, FileMode.Create)))
                binaryWriter.Write(this.CurDat);
        }
    }
}