using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ChulaTTS.Synthesizer
{
    public class Alpha1 : ISynthesizer
    {
        private double speechRate = 1.0;
        private List<string> ModelList;
        private string CurModel;
        private string param;
        private string TempPath;
        private string TempName;

        // ---------------------
        private Phone2Lab P2L;
        private SynthesizerEngine Synthesizer;
        // ---------------------

        public Alpha1()
        {
            // -------------
            P2L = new Phone2Lab();
            Synthesizer = new SynthesizerEngine();
            // -------------
            this.TempPath = "";
            
            if (this.TempPath == "")
                this.TempPath = "tmp";
            
            
            Random random = new Random();
            do
            {
                this.TempName = random.Next(10000000, 99999999).ToString();
            }
            while (File.Exists(this.TempPath + "\\" + this.TempName + ".lab"));
        }

        public void SetModel(String ModelName)
        {
            this.CurModel = ModelName;
        }

        public List<string> GetModel()
        {
            this.ModelList = new List<string>();
            foreach (string path in Directory.GetFiles("model\\", "*.htsvoice"))
            {
                if (File.Exists("model\\" + Path.GetFileNameWithoutExtension(path) + ".conf"))
                    this.ModelList.Add(Path.GetFileNameWithoutExtension(path));
            }
            return this.ModelList;
        }

        public void SetFrequency(int fs)
        {
        }

        public void SetPitch(double pitch)
        {
        }

        public void SetSpeed(double speed)
        {
            this.speechRate = speed;
        }

        public string About()
        {
            return "HTS Engine 1.5";
        }

        public byte[] Synthesis(List<KeyValuePair<string, int>> inp)
        {
            this.dfile(Path.Combine(this.TempPath, this.TempName + ".lab"));
            this.dfile(Path.Combine(this.TempPath, this.TempName + ".wav"));
            this.dfile(Path.Combine(this.TempPath, this.TempName + ".dur"));
            string str = "";
            foreach (KeyValuePair<string, int> keyValuePair in inp)
                str += P2L.G5T5(keyValuePair.Key);
            if (str.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length <= 2)
                return new byte[10] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(this.TempPath, this.TempName + ".lab")))
            {
                streamWriter.Write(str);
                streamWriter.Close();
            }
            this.param = " -r " + (object)this.speechRate + " ";
            Synthesizer.SynthesisR2(Path.Combine(this.TempPath, this.TempName + ".lab"), this.TempName, ("model\\" + this.CurModel), this.param, (this.TempPath + "\\"));
           
            FileInfo fileInfo = new FileInfo(Path.Combine(this.TempPath, this.TempName + ".wav"));
            while (fileInfo.Length == 0L)
            {
                Synthesizer.SynthesisR2(Path.Combine(this.TempPath, this.TempName + ".lab"), this.TempName, ("model\\" + this.CurModel), this.param, (this.TempPath + "\\"));
                fileInfo = new FileInfo(Path.Combine(this.TempPath, this.TempName + ".wav"));
                Thread.Sleep(100);
            }
            byte[] numArray = (byte[])null;
            bool flag = false;
            while (!flag)
            {
                try
                {
                    using (BinaryReader binaryReader = new BinaryReader((Stream)File.Open(Path.Combine(this.TempPath, this.TempName + ".wav"), FileMode.Open)))
                    {
                        binaryReader.ReadBytes(40);
                        int count = binaryReader.ReadInt32();
                        numArray = binaryReader.ReadBytes(count);
                        binaryReader.Close();
                    }
                    flag = true;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
            return numArray;
        }

        private void dfile(string FileName)
        {
            try
            {
                File.Delete(FileName);
            }
            catch (Exception)
            {
                Thread.Sleep(100);
                this.dfile(FileName);
            }
        }

        public void Dispose()
        {
            Synthesizer.Dispose();
        }
    }
}
