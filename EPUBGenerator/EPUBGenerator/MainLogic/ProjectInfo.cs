﻿using eBdb.EpubReader;
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
        public String EpubProjectPath { get; private set; }
        public String EpubName { get; private set; }
        public String PackageName { get; private set; }
        public SortedList<int, String> ContentList { get; private set; }

        public String EpubPath { get { return Path.Combine(Resources, EpubName); } }

        private Epub _EpubFile;
        public Epub EpubFile
        {
            get
            {
                if (_EpubFile == null)
                    _EpubFile = new Epub(EpubPath);
                return _EpubFile;
            }
        }

        #region Directories
        public String Resources { get { return Project.GetDirectory(ProjectPath, "Resources"); } }
        public String PackageResources { get { return Project.GetDirectory(ProjectPath, "Resources", PackageName); } }
        public String Saves { get { return Project.GetDirectory(ProjectPath, "Saves"); } }
        public String PackageSaves { get { return Project.GetDirectory(ProjectPath, "Saves", PackageName); } }
        public String AudioSaves { get { return Project.GetDirectory(ProjectPath, "Saves", "Audio"); } }
        public String Temp { get { return Project.GetDirectory(ProjectPath, "Temp"); } }
        public String Export { get { return Project.GetDirectory(ProjectPath, "Export"); } }
        #endregion
        
        #region ----------- NEW PROJECT ------------
        public ProjectInfo(String epubPath, String projPath)
        {
            ProjectPath = Project.GetDirectory(projPath);
            ProjectName = Path.GetFileNameWithoutExtension(ProjectPath);
            EpubProjectPath = Path.Combine(ProjectPath, ProjectName + ".epubproj");
            EpubName = Path.GetFileName(epubPath);
            File.Copy(epubPath, EpubPath);
            PackageName = EpubFile.GetOpfDirectory();

            Project.Synthesizer.TempPath = Temp;

            ContentList = new SortedList<int, string>();
            SetContentList(EpubFile.TOC);
        }

        private void SetContentList(List<NavPoint> navPoints)
        {
            foreach (NavPoint nav in navPoints)
            {
                if (nav.ContentData != null)
                    ContentList.Add(nav.Order, nav.Source);
                SetContentList(nav.Children);
            }
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
        public ProjectInfo(String epubProjPath)
        {
            EpubProjectPath = epubProjPath;
            ProjectPath = Path.GetDirectoryName(epubProjPath);
            XElement xProject;
            using (StreamReader streamReader = new StreamReader(epubProjPath))
            {
                xProject = XElement.Parse(streamReader.ReadToEnd());
                streamReader.Close();
            }
            ProjectName = xProject.Attribute("name").Value;
            PackageName = xProject.Attribute("package").Value;
            EpubName = xProject.Attribute("epub").Value;

            Project.Synthesizer.TempPath = Temp;

            ContentList = new SortedList<int, String>();
            foreach (XElement xContent in xProject.Element("Contents").Elements("Content"))
            {
                int order = int.Parse(xContent.Attribute("order").Value);
                String src = xContent.Attribute("src").Value;
                ContentList.Add(order, src);
            }
        }
        #endregion

        #region ----------- SAVE PROJECT ------------
        public void Save()
        {
            XElement xProject = new XElement("Project");
            xProject.Add(new XAttribute("name", ProjectName));
            xProject.Add(new XAttribute("package", PackageName));
            xProject.Add(new XAttribute("epub", EpubName));

            XElement xContents = new XElement("Contents");
            foreach (KeyValuePair<int, String> content in ContentList)
            {
                XElement xContent = new XElement("Content");
                xContent.Add(new XAttribute("order", content.Key));
                xContent.Add(new XAttribute("src", content.Value));
                xContents.Add(xContent);
            }
            xProject.Add(xContents);

            XElement xPictures = new XElement("Pictures");
            xProject.Add(xPictures);
            
            using (StreamWriter streamWriter = new StreamWriter(EpubProjectPath))
            {
                streamWriter.Write(xProject);
                streamWriter.Close();
            }
        }
        #endregion
    }
}
