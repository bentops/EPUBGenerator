using eBdb.EpubReader;
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
        #region General Project Info
        public String ProjectPath { get; set; }
        public String ProjectName { get; set; }
        public String EpubProjectPath { get; private set; }
        public String EpubName { get; private set; }
        public String PackageName { get; private set; }
        public SortedList<int, String> ContentList { get; private set; }

        public String EpubPath { get { return Path.Combine(ResourcesPath, EpubName); } }

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

        private String _DictionaryName;
        public String DictionaryPath { get { return Path.Combine(SavesPath, _DictionaryName); } }
        public Dictionary<String, List<String>> Dictionary { get; private set; }
        #endregion

        #region Directories
        public String ResourcesPath { get { return Project.GetDirectory(ProjectPath, "Resources"); } }
        public String PackageResourcesPath { get { return Project.GetDirectory(ProjectPath, "Resources", PackageName); } }
        public String SavesPath { get { return Project.GetDirectory(ProjectPath, "Saves"); } }
        public String PackageSavesPath { get { return Project.GetDirectory(ProjectPath, "Saves", PackageName); } }
        public String AudioSavesPath { get { return Project.GetDirectory(ProjectPath, "Saves", "Audio"); } }
        public String TempPath { get { return Project.GetDirectory(ProjectPath, "Temp"); } }
        public String ExportPath { get { return Project.GetDirectory(ProjectPath, "Export"); } }
        #endregion

        #region Editing Project Info
        public bool IsSaved { get; set; }
        public State CurrentState { get; set; }
        public RunWord CurrentRunWord { get; set; }
        public Content CurrentContent { get; set; }
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

            Project.Synthesizer.TempPath = TempPath;

            ContentList = new SortedList<int, string>();
            SetContentList(EpubFile.TOC);

            _DictionaryName = "Dictionary.txt";
            Dictionary = new Dictionary<String, List<String>>();
            using (StreamReader streamReader = new StreamReader("Resources/Modified_Dictionary.txt"))
            {
                while (!streamReader.EndOfStream)
                {
                    String[] line = streamReader.ReadLine().Split(' ');
                    if (Dictionary.ContainsKey(line[0]))
                        Dictionary[line[0]].AddRange(line);
                    else
                        Dictionary.Add(line[0], line.ToList());
                    Dictionary[line[0]] = Dictionary[line[0]].Distinct().ToList();
                }
                streamReader.Close();
            }
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

        public void Dispose()
        {
            if (_EpubFile != null)
                _EpubFile.Dispose();
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

            foreach (XAttribute attribute in xProject.Attributes())
            {
                String value = attribute.Value;
                switch (attribute.Name.ToString())
                {
                    case "name": ProjectName = value; break;
                    case "package": PackageName = value; break;
                    case "epub": EpubName = value; break;
                    case "dict": _DictionaryName = value; break;
                }
            }

            Dictionary = new Dictionary<String, List<String>>();
            using (StreamReader streamReader = new StreamReader(DictionaryPath))
            {
                while (!streamReader.EndOfStream)
                {
                    String[] line = streamReader.ReadLine().Split(' ');
                    if (Dictionary.ContainsKey(line[0]))
                        Dictionary[line[0]].AddRange(line);
                    else
                        Dictionary.Add(line[0], line.ToList());
                    Dictionary[line[0]] = Dictionary[line[0]].Distinct().ToList();
                }
                streamReader.Close();
            }

            Project.Synthesizer.TempPath = TempPath;

            ContentList = new SortedList<int, String>();
            foreach (XElement xContent in xProject.Element("Contents").Elements("Content"))
            {
                int order = -1;
                String src = "";
                foreach (XAttribute attribute in xContent.Attributes())
                {
                    String value = attribute.Value;
                    switch (attribute.Name.ToString())
                    {
                        case "order": order = int.Parse(value); break;
                        case "src": src = value; break;
                        case "selected": /*/ DO WHAT? /*/ break;
                    }
                }
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
            xProject.Add(new XAttribute("dict", _DictionaryName));

            XElement xContents = new XElement("Contents");
            foreach (KeyValuePair<int, String> content in ContentList)
            {
                XElement xContent = new XElement("Content");
                xContent.Add(new XAttribute("order", content.Key));
                xContent.Add(new XAttribute("src", content.Value));
                if (CurrentContent != null && content.Key == CurrentContent.Order)
                    xContent.Add(new XAttribute("selected", ""));
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

            using (StreamWriter streamWriter = new StreamWriter(DictionaryPath))
            {
                foreach (List<String> list in Dictionary.Values)
                    streamWriter.WriteLine(String.Join(" ", list));
                streamWriter.Close();
            }
        }
        #endregion
    }
}
