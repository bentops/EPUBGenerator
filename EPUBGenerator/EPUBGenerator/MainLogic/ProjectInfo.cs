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
        public List<Content> Contents { get; private set; }
        public int TotalSentences
        {
            get
            {
                int total = 0;
                foreach (Content content in Contents)
                    total += content.TotalSentences;
                return total;
            }
        }

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
        public State CurrentState { get; set; }
        public Word CurrentWord
        {
            get { return CurrentContent == null ? null : CurrentContent.SelectedWord; }
            set { if (CurrentContent != null) CurrentContent.SelectedWord = value; }
        }
        public RunWord CurrentRunWord
        {
            get { return CurrentWord == null ? null : CurrentWord.Run; }
            set { CurrentWord = (value == null ? null : value.Word); }
        }
        public ARun CurrentARun
        {
            get
            {
                if (CurrentRunWord == null)
                    return null;
                if (CurrentRunWord.IsImage)
                    return CurrentRunWord.Image;
                return CurrentRunWord;
            }
        }
        public Content CurrentContent { get; private set; }
        public bool Changed { get; private set; }
        public bool IsSaved
        {
            get
            {
                bool save = !Changed;
                foreach (Content content in Contents)
                    save &= !content.Changed;
                return save;
            }
            set { Changed = !value; }
        }
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
            
            _DictionaryName = "Dictionary.txt";
            Dictionary = new Dictionary<String, List<String>>();
            using (StreamReader streamReader = new StreamReader(ProjectProperties.DictionaryPath))
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
            Contents = new List<Content>();
        }
        
        public void AddContent(Content content)
        {
            if (Contents == null)
                Contents = new List<Content>();
            Contents.Add(content);
        }

        public void Dispose()
        {
            if (_EpubFile != null)
            {
                _EpubFile.Dispose();
                _EpubFile = null;
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
            
            Contents = new List<Content>();
            foreach (XElement xContent in xProject.Element("Contents").Elements("Content"))
            {
                int order = -1;
                String src = "";
                bool selected = false;
                foreach (XAttribute attribute in xContent.Attributes())
                {
                    String value = attribute.Value;
                    switch (attribute.Name.ToString())
                    {
                        case "order": order = int.Parse(value); break;
                        case "src": src = value; break;
                        case "selected": selected = true; break;
                    }
                }

                Content content = new Content(src, this);
                int insertIdx = Contents.Count;
                while (insertIdx > 0 && order < Contents[insertIdx - 1].ID)
                    insertIdx--;
                Contents.Insert(insertIdx, content);

                if (selected)
                    CurrentContent = content;
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
            foreach (Content content in Contents)
            {
                XElement xContent = new XElement("Content");
                xContent.Add(new XAttribute("order", content.ID));
                xContent.Add(new XAttribute("src", content.Source));
                if (CurrentContent != null && content == CurrentContent)
                    xContent.Add(new XAttribute("selected", ""));
                xContents.Add(xContent);
                content.Save();
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
            Changed = false;
        }
        #endregion

        #region ----------- EDIT PROJECT ------------
        public void SelectContent(String source)
        {
            foreach (Content content in Contents)
                if (content.Source.Equals(source))
                {
                    CurrentContent = content;
                    return;
                }
        }
        #endregion
    }
}
