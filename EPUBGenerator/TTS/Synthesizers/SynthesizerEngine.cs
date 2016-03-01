using System;
using System.Diagnostics;
using System.IO;

namespace TTS.Synthesizers
{
    class SynthesizerEngine
    {
        public void Synthesis(string fname, string oname, string modelPath, string param)
        {
            Process process = new Process();
            Process.Start(new ProcessStartInfo("hts_engine.exe")
            {
                Arguments = "-td " + modelPath + "\\tree-dur.inf -tf " + modelPath + "\\tree-lf0.inf -tm " + modelPath + "\\tree-mgc.inf -md " + modelPath + "\\dur.pdf -mf " + modelPath + "\\lf0.pdf -mm " + modelPath + "\\mgc.pdf -dm " + modelPath + "\\mgc.win1 -dm " + modelPath + "\\mgc.win2 -dm " + modelPath + "\\mgc.win3 -df " + modelPath + "\\lf0.win1 -df " + modelPath + "\\lf0.win2 -df " + modelPath + "\\lf0.win3 " + param + " -cm " + modelPath + "\\gv-mgc.pdf -cf " + modelPath + "\\gv-lf0.pdf -b 0.0  -ow " + oname + ".wav " + fname + ".lab",
                RedirectStandardError = false,
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true
            }).WaitForExit();
        }

        public void SynthesisR2(string fname, string oname, string modelPath, double speed, string audioPath, string tempPath)
        {
            string[] arguments = new string[]
            {
                "-m " + modelPath + ".htsvoice",                    // model
                "-r " + speed,                                      // speed
                "-ow " + Path.Combine(audioPath, oname + ".wav"),   // outWav
                "-od " + Path.Combine(tempPath, oname + ".dur"),    // outDur
                fname
            };
            Process process = new Process();
            Process.Start(new ProcessStartInfo("hts_engine1.10-org.exe")
            {
                Arguments = string.Join(" ", arguments),
                RedirectStandardError = false,
                RedirectStandardOutput = false,
                UseShellExecute = false,
                CreateNoWindow = true
            }).WaitForExit();
        }

        public void Dispose()
        {
        }
    }
}
