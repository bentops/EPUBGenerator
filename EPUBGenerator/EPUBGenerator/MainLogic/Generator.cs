using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eBdb.EpubReader;
using System.ComponentModel;
using System.Threading;
using System.Xml.Linq;
using System.IO;

namespace EPUBGenerator.MainLogic
{
    class Generator
    {
        private Epub epub;
        private BackgroundWorker bw;
        private String projPath;
        private List<Chapter> Chapters;

        public Generator()
        {
        }

        public void CreateEpub(String epubPath, String projPath, BackgroundWorker bw)
        {
            this.bw = bw;
            this.projPath = projPath;
            this.epub = new Epub(epubPath);
            this.Chapters = new List<Chapter>();
            this.GetAllChapters(epub.TOC);
            Console.WriteLine("Total: " + Chapters.Count);
            int i = 0;
            foreach (Chapter Ch in Chapters)
            {
                Ch.GetContent();

                StreamWriter sw = new StreamWriter(System.IO.Path.Combine(projPath, Ch.Source));
                sw.Write(Ch.Root);
                sw.Close();

                i++;
                bw.ReportProgress(i * 100 / Chapters.Count);
                Thread.Sleep(100);
            }
        }

        private void GetAllChapters(List<NavPoint> TOC)
        {
            foreach (NavPoint Nav in TOC)
            {
                if (Nav.ContentData != null)
                    this.Chapters.Add(new Chapter(Nav));
                GetAllChapters(Nav.Children);
            }
        }
        
    }
}
