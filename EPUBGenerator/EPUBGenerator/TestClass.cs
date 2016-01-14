using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using eBdb.EpubReader;
using System.Xml.Linq;
using System.IO;

namespace EPUBGenerator
{
    public class TestClass
    {
        public static string Create(String file)
        {
            //string path = @"C:\Users\xinghbtong.Baitongs\Documents\Top\Chula\Year 4\Senior Project\Project\EPUB\";
            string path = "" + "";
            Epub epub = new Epub(path + file);
            string output = "" + ((ContentData)epub.Content[2]).GetContentAsPlainText();
            for (int i = 110; i < epub.Content.Count; i++)
            {
                string content = ((ContentData)epub.Content[i]).GetContentAsPlainText();
                //output += "------------- [" + i + "]\r\n";
                output += content + "\r\n";
            }
            return output;
        }

        private static string getContentData(XElement element)
        {
            string output = "";
            foreach (XNode node in element.Nodes()) 
            {
                if (node is XText)
                    output += ((XText)node).Value + "\r\n";
                else if (node is XElement)
                    output += getContentData((XElement)node);
            }
            return output;
        }

        private static string getAllContents(List<NavPoint> TOC, string savePath)
        {
            string output = "----------NavPoint----------\r\n";
            foreach (NavPoint np in TOC)
            {
                output += "ID: " + np.ID + "\r\n";
                output += "Title: " + np.Title + "\r\n";
                output += "Source: " + np.Source + "\r\n";
                output += "Order: " + np.Order + "\r\n";
                output += "<<<<< CONTENT >>>>>\r\n";
                if (np.ContentData != null)
                {
                    XElement root = XElement.Parse(np.ContentData.Content);
                    XNamespace xns = root.Attribute("xmlns") != null ? root.Attribute("xmlns").Value : XNamespace.None;
                    output += getContentData(root.Element(xns + "body"));
                    Directory.CreateDirectory(savePath);
                    Console.WriteLine(Path.Combine(savePath, np.Source));
                    StreamWriter sw = new StreamWriter(Path.Combine(savePath, np.Source));
                    sw.Write(root);
                    sw.Close();
                }
                output += "<<<<< \\CONTENT >>>>>\r\n";
                output += getAllContents(np.Children, savePath);
            }
            return output;
        }

        public static string reCreate(string file, string savePath)
        {
            //string path = @"C:\Users\xinghbtong.Baitongs\Documents\Top\Chula\Year 4\Senior Project\Project\EPUB\";
            string path = "";
            Epub epub = new Epub(path + file);
            return getAllContents(epub.TOC, savePath);
        }
    }
}
