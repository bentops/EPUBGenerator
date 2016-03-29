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
        public int ID { get; private set; }
        public String CID { get { return "C" + ID; } }
        public XElement Root { get; private set; }
        public XNamespace Xns { get; private set; }
        public String Source { get; private set; }

        public String ContentAudio { get { return Project.GetDirectory(ProjectInfo.AudioSavesPath, CID); } }
        public String ContentResource { get { return Project.GetDirectory(ProjectInfo.PackageResourcesPath, Source); } }
        public String ContentSave { get { return Project.GetDirectory(ProjectInfo.PackageSavesPath, Source); } }

        public bool Changed { get; set; }
        public Word SelectedWord { get; set; }
        
        public List<Block> Blocks { get; private set; }
        public List<ContentBlock> ContentBlocks { get; private set; }
        public List<ImageBlock> ImageBlocks { get; private set; }
        public int TotalSentences
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
        public Content(int id, ContentData cData, ProjectInfo projInfo)
        {
            ProjectInfo = projInfo;
            ID = id;
            Source = cData.FileName;
            Root = XElement.Parse(cData.Content);
            Xns = Root.Attribute("xmlns") != null ? Root.Attribute("xmlns").Value : XNamespace.None;

            Blocks = new List<Block>();
            ContentBlocks = new List<ContentBlock>();
            ImageBlocks = new List<ImageBlock>();
            GetBlocks(Root.Element(Xns + "body"));
            Changed = true;

            // Save Content Structure in @"ProjDir\Resources\Package"
            using (StreamWriter streamWriter = new StreamWriter(ContentResource))
            {
                streamWriter.Write(Root);
                streamWriter.Close();
            }
        }

        private void GetBlocks(XElement curNode)
        {
            if (curNode.Name.Equals(Xns + "img"))
            {
                int id = ImageBlocks.Count;
                ImageBlock iBlock = new ImageBlock(id, curNode, this);
                ImageBlocks.Add(iBlock);
                curNode.SetAttributeValue("id", iBlock.B_ID);
                Blocks.Add(iBlock);
            }
            foreach (XNode childNode in curNode.Nodes())
            {
                if (childNode is XText)
                {
                    XText textNode = childNode as XText;
                    int id = ContentBlocks.Count;
                    String text = textNode.Value;
                    ContentBlock cBlock = new ContentBlock(id, text, this);
                    ContentBlocks.Add(cBlock);
                    textNode.Value = cBlock.B_ID;
                    Blocks.Add(cBlock);
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
            xContent.Add(new XAttribute("id", ID));
            XElement xBlocks = new XElement("Blocks");
            foreach (Block block in Blocks)
                xBlocks.Add(block.ToXml());
            xContent.Add(xBlocks);

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
                ID = int.Parse(xContent.Attribute("id").Value);
                Blocks = new List<Block>();
                ContentBlocks = new List<ContentBlock>();
                ImageBlocks = new List<ImageBlock>();
                foreach (XElement xBlock in xContent.Element("Blocks").Elements())
                {
                    if (xBlock.Name == "ContentBlock")
                    {
                        ContentBlock block = new ContentBlock(xBlock, this);
                        ContentBlocks.Add(block);
                        Blocks.Add(block);
                    }
                    else
                    {
                        ImageBlock block = new ImageBlock(xBlock, this);
                        ImageBlocks.Add(block);
                        Blocks.Add(block);
                    }
                }
                streamReader.Close();
            }
        }
        #endregion
    }
}