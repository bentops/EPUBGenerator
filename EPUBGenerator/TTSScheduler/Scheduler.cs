using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading;
using System.IO;



namespace TTSScheduler
{
    public class Scheduler
    {

        private static Int32 runInt;
        private static List<String> SchGrapheme = new List<String>();
        private static List<List<KeyValuePair<String, Int32>>> SchPhoneme = new List<List<KeyValuePair<String, Int32>>>();
        private static List<String> SchWave = new List<String>();

        private static Assembly A_G2P;
        private static MethodInfo M_G2P;
        private static Object O_G2P;


        private static Assembly A_Syn;
        private static MethodInfo M_Syn_1;
        private static MethodInfo M_Syn_2;
        private static MethodInfo M_Syn_3;
        private static MethodInfo M_Syn_4;
        private static MethodInfo M_Syn_5;
        private static MethodInfo M_Syn_6;
        private static MethodInfo M_Syn_7;
        private static MethodInfo M_Syn_8;
        private static Object O_Syn;

        private static WaveLib.WaveOutPlayer m_Player;

        private static WaveLib.FifoStream m_Fifo = new WaveLib.FifoStream();

        private static byte[] m_PlayBuffer;
        private static byte[] m_RecBuffer;

        public Scheduler(String g2pconfig, String synconfig)
        {
            //Directory.SetCurrentDirectory("D:\\TTSProject\\SynthesisTool3\\SynthesisTool1\\bin\\Debug\\");
            runInt = 1;

            A_G2P = Assembly.LoadFrom("G2P.dll");
            M_G2P = A_G2P.GetTypes()[0].GetMethod("G2PConversion");
            O_G2P = A_G2P.CreateInstance("G2P.G2P", false, BindingFlags.ExactBinding, null, new Object[] { g2pconfig }, null, null);


            A_Syn = Assembly.LoadFrom("SynthesizerBlock.dll");
            M_Syn_1 = A_Syn.GetTypes()[0].GetMethod("GetModel");
            M_Syn_2 = A_Syn.GetTypes()[0].GetMethod("SetModel");
            M_Syn_3 = A_Syn.GetTypes()[0].GetMethod("Synthesis");
            M_Syn_4 = A_Syn.GetTypes()[0].GetMethod("PlaySound");
            M_Syn_5 = A_Syn.GetTypes()[0].GetMethod("StopWave");
            M_Syn_6 = A_Syn.GetTypes()[0].GetMethod("Reset");
            M_Syn_7 = A_Syn.GetTypes()[0].GetMethod("SaveFile");
            M_Syn_8 = A_Syn.GetTypes()[0].GetMethod("SetFrequency");
            O_Syn = A_Syn.CreateInstance("SynthesizerBlock.SynthesizerBlock", false, BindingFlags.ExactBinding, null, new Object[] { synconfig }, null, null);

            WaveLib.WaveFormat fmt = new WaveLib.WaveFormat(16000, 16, 1);
            m_Player = new WaveLib.WaveOutPlayer(0, fmt, 4000, 3, new WaveLib.BufferFillEventHandler(Filler));


        }

        private void Filler(IntPtr data, int size)
        {
            if (m_PlayBuffer == null || m_PlayBuffer.Length < size)
                m_PlayBuffer = new byte[size];
            if (m_Fifo.Length >= size)
                m_Fifo.Read(m_PlayBuffer, 0, size);
            else
                for (int i = 0; i < m_PlayBuffer.Length; i++)
                    m_PlayBuffer[i] = 0;
            System.Runtime.InteropServices.Marshal.Copy(m_PlayBuffer, 0, data, size);
        }


        public List<String> GetSound()
        {
            return (List<String>)M_Syn_1.Invoke(O_Syn, new Object[] { 1 });
        }

        public void SetSound(String inx)
        {
            M_Syn_2.Invoke(O_Syn, new Object[] {1, inx });
        }

        private static Thread T_G2P = new Thread(new ThreadStart(Run_G2P));
        private static Thread T_Syn = new Thread(new ThreadStart(Run_Syn));
        private static Thread T_Ply = new Thread(new ThreadStart(Run_Ply));



        public void Synthesis(String inp)
        {
            SchGrapheme.Add(inp);
            if (SchGrapheme.Count == 1)
            {
                T_G2P.Abort();
                T_G2P = new Thread(new ThreadStart(Run_G2P));
                T_G2P.Start();
            }
        }

        public void Synthesis2(String inp)
        {
            List<KeyValuePair<String, Int32>> G2Pout = new List<KeyValuePair<String, Int32>>();
            G2Pout = (List<KeyValuePair<String, Int32>>)M_G2P.Invoke(O_G2P, new Object[] { inp });
            //MethodInfo setSpeed = A_Syn.GetTypes()[0].GetMethod("SetSpeed");
            //setSpeed.Invoke(O_Syn, new Object[] { 2.0 });
            //M_Syn_8.Invoke(O_Syn, new Object[] { 32000 });
            M_Syn_3.Invoke(O_Syn, new Object[] { G2Pout});
            M_Syn_4.Invoke(O_Syn, new Object[] { });
            M_Syn_7.Invoke(O_Syn, new Object[] { "tmp.wav"});
        }

        public void Clear()
        {
            M_Syn_6.Invoke(O_Syn, new Object[] { });
            if (m_Player != null)
                try
                {
                    m_Player.Dispose();
                }
                finally
                {
                    m_Player = null;
                }
            m_Fifo.Flush(); // clear all pending data
        }

        private static void Run_G2P()
        {
            long startTime = DateTime.Now.Ticks;
            while(SchGrapheme.Count != 0)
            {
                SchPhoneme.Add((List<KeyValuePair<String, Int32>>)M_G2P.Invoke(O_G2P, new Object[] { SchGrapheme[0] }));
                SchGrapheme.RemoveAt(0);
                long endTime2 = DateTime.Now.Ticks;
                TimeSpan ts = new TimeSpan(endTime2 - startTime);
                Console.WriteLine("Finished G2P "+ts.Seconds.ToString()+":"+ts.Milliseconds.ToString());
                if (SchPhoneme.Count == 1)
                {
                    T_Syn.Abort();
                    T_Syn = new Thread(new ThreadStart(Run_Syn));
                    T_Syn.Start();
                }

            }
            long endTime = DateTime.Now.Ticks;
            TimeSpan ts2 = new TimeSpan(endTime - startTime);
            Console.WriteLine("Finished Run_G2P " + ts2.Seconds.ToString() + ":" + ts2.Milliseconds.ToString());
        }

        private static void Run_Syn()
        {
            long startTime = DateTime.Now.Ticks;
            while (SchPhoneme.Count != 0)
            {
                M_Syn_3.Invoke(O_Syn, new Object[] { SchPhoneme[0] });
                long endTime2 = DateTime.Now.Ticks;
                String tmpname = Path.GetTempFileName();
                M_Syn_7.Invoke(O_Syn, new Object[] { tmpname });
                SchWave.Add(tmpname);
                SchPhoneme.RemoveAt(0);
                if (SchWave.Count == 1)
                {
                    T_Ply.Abort();
                    T_Ply = new Thread(new ThreadStart(Run_Ply));
                    T_Ply.Start();
                }
                runInt++;
            }
            long endTime = DateTime.Now.Ticks;
            TimeSpan ts2 = new TimeSpan(endTime - startTime);
            Console.WriteLine("Finished Run_Syn " + ts2.Seconds.ToString() + ":" + ts2.Milliseconds.ToString());

        }

        private static void Run_Ply()
        {
            while (SchWave.Count != 0)
            {
                
                //M_Syn_4.Invoke(O_Syn, new Object[] { SchWave[0] });
                BinaryReader binReader = new BinaryReader(File.Open(SchWave[0], FileMode.Open));
                FileInfo Finf = new FileInfo(SchWave[0]);
                int flength = System.Convert.ToInt32(Finf.Length) - 44;

                m_RecBuffer = new byte[flength];
                binReader.Read(m_RecBuffer, 0, 44);
                binReader.Read(m_RecBuffer, 0, flength);
                m_Fifo.Write(m_RecBuffer, 0, m_RecBuffer.Length);

                binReader.Close();
                File.Delete(SchWave[0]);
                SchWave.RemoveAt(0);

            }
        }

        public void ClearList()
        {
            
            T_Ply.Abort();
            T_G2P.Abort();
            T_Syn.Abort();
            foreach (String buff in SchWave)
            {
                try
                {
                    File.Delete(buff);
                }
                catch 
                {
                    }
            }
            SchWave.Clear();
            SchPhoneme.Clear();
            SchGrapheme.Clear();
            m_Fifo.Flush();
        }

        public String GenBaseForm(String inp)
        {
            List<KeyValuePair<String, Int32>> outp = (List<KeyValuePair<String, Int32>>)M_G2P.Invoke(O_G2P, new Object[] { inp });

            String rout = "";
            foreach (KeyValuePair<String, Int32> item in outp)
            {
                rout = rout + item.Key + " ";
            }

            return rout;
        }
       
    }
}
