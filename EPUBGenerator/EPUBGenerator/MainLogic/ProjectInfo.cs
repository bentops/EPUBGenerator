using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class ProjectInfo
    {
        public String ProjectPath { get; set; }
        public String ProjectName { get; set; }
        public String PackageName { get; private set; }

        public String Resources { get { return Path.Combine(ProjectPath, "Resources"); } }
        public String PackageResources { get { return Path.Combine(ProjectPath, "Resources", PackageName); } }
        public String Saves { get { return Path.Combine(ProjectPath, "Saves"); } }
        public String PackageSaves { get { return Path.Combine(ProjectPath, "Saves", PackageName); } }
        public String AudioSaves { get { return Path.Combine(ProjectPath, "Saves", "Audio"); } }
        public String Temp { get { return Path.Combine(ProjectPath, "Temp"); } }

        public List<String> ContentSources { get; private set; }

        public ProjectInfo(String epubProjPath)
        {
            ProjectPath = Path.GetDirectoryName(epubProjPath);

            XElement xProject;
            using (StreamReader streamReader = new StreamReader(epubProjPath))
            {
                xProject = XElement.Parse(streamReader.ReadToEnd());
                streamReader.Close();
            }

            ProjectName = xProject.Attribute("name").Value;
            PackageName = xProject.Attribute("package").Value;

            SortedList<int, String> cList = new SortedList<int, String>();
            foreach (XElement xContent in xProject.Element("Contents").Elements("Content"))
            {
                int order = int.Parse(xContent.Attribute("order").Value);
                String src = xContent.Attribute("src").Value;
                cList.Add(order, src);
            }

            ContentSources = new List<String>();
            foreach (String content in cList.Values)
                ContentSources.Add(content);
        }

        public String GetContentResource(Content content)
        {
            return Path.Combine(PackageResources, content.Source);
        }

        public String GetContentSave(Content content)
        {
            return Path.Combine(Saves, content.Source);
        }
    }
}
