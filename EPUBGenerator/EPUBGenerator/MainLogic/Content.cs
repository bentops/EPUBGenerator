using eBdb.EpubReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Content
    {
        public ProjectInfo ProjectInfo { get; private set; }
        public String CID { get { return "C" + Order; } }
        public String NavID { get; private set; }
        public XElement Root { get; private set; }
        public XNamespace Xns { get; private set; }
        public String Source { get; private set; }
        public String Title { get; private set; }
        public int Order { get; private set; }

        public String ContentAudio { get { return Project.GetDirectory(ProjectInfo.AudioSavesPath, CID); } }
        public String ContentResource { get { return Path.Combine(ProjectInfo.PackageResourcesPath, Source); } }
        public String ContentSave { get { return Path.Combine(ProjectInfo.SavesPath, Source); } }

        public bool Changed { get; set; }
        public Word SelectedWord { get; set; }

        public List<Block> Blocks { get; private set; }
        public int SentenceCount
        {
            get
            {
                int count = 0;
                foreach (Block block in Blocks)
                    count += block.Sentences.Count;
                return count;
            }
        }

        #region ----------- NEW PROJECT ------------
        public Content(NavPoint Nav, ProjectInfo projInfo)
        {
            ProjectInfo = projInfo;
            NavID = Nav.ID;
            Root = XElement.Parse(Nav.ContentData.Content);
            Xns = Root.Attribute("xmlns") != null ? Root.Attribute("xmlns").Value : XNamespace.None;
            Source = Nav.Source;
            Title = Nav.Title;
            Order = Nav.Order;

            Blocks = new List<Block>();
            GetBlocks(Root.Element(Xns + "body"));
            Changed = true;
        }

        private void GetBlocks(XElement curNode)
        {
            foreach (XNode childNode in curNode.Nodes())
            {
                if (childNode is XText)
                {
                    XText textNode = childNode as XText;
                    int id = Blocks.Count;
                    String text = textNode.Value.Trim();
                    Block block = new Block(id, text, this);
                    Blocks.Add(block);
                    textNode.Value = block.B_ID;
                }
                else if (childNode is XElement)
                    GetBlocks(childNode as XElement);
            }
        }
        #endregion

        #region ----------- SAVE PROJECT ------------
        public void Save()
        {
            if (!Changed)
                return;

            // Let Every Sentence Uses Randomized ID.
            foreach (Block block in Blocks)
                foreach (Sentence sentence in block.Sentences)
                    sentence.UseRandomizedID();

            // Remove Unused WAV Files: Those With Non-Randomized ID.
            foreach (String file in Directory.EnumerateFiles(ContentAudio, ProjectProperties.NonRandomPattern))
                File.Delete(file);

            // Let Every Sentence Uses Non-Randomized ID.
            int count = 0;
            foreach (Block block in Blocks)
                foreach (Sentence sentence in block.Sentences)
                    sentence.UseNonRandomizedID(++count);

            // Remove Unused WAV Files: Those With Randomized ID.
            foreach (String file in Directory.EnumerateFiles(ContentAudio, ProjectProperties.RandomPattern))
                File.Delete(file);

            XElement xContent = new XElement("Content");
            xContent.Add(new XAttribute("id", NavID));
            xContent.Add(new XAttribute("title", Title));
            xContent.Add(new XAttribute("order", Order));
            foreach (Block block in Blocks)
                xContent.Add(block.ToXml());

            using (StreamWriter streamWriter = new StreamWriter(ContentSave))
            {
                streamWriter.Write(xContent);
                streamWriter.Close();
            }

            Changed = false;
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
        public Content(String contentPath, ProjectInfo projectInfo)
        {
            Source = contentPath;
            ProjectInfo = projectInfo;
            using (StreamReader streamReader = new StreamReader(ContentResource))
            {
                Root = XElement.Parse(streamReader.ReadToEnd());
                Xns = Root.Attribute("xmlns") != null ? Root.Attribute("xmlns").Value : XNamespace.None;
                streamReader.Close();
            }
            using (StreamReader streamReader = new StreamReader(ContentSave))
            {
                XElement xContent = XElement.Parse(streamReader.ReadToEnd());
                foreach (XAttribute attribute in xContent.Attributes())
                {
                    String value = attribute.Value;
                    switch (attribute.Name.ToString())
                    {
                        case "id": NavID = value; break;
                        case "title": Title = value; break;
                        case "order": Order = int.Parse(value); break;
                    }
                }

                Blocks = new List<Block>();
                foreach (XElement xBlock in xContent.Elements("Block"))
                    Blocks.Add(new Block(xBlock, this));
                streamReader.Close();
            }
        }
        #endregion
    }
}