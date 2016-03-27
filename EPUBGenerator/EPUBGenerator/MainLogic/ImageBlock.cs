using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class ImageBlock : Block
    {
        public override string B_ID { get { return ProjectProperties.ImageBlockID(ID); } }

        public String Source { get; private set; }
        public RunImage Run { get; set; }

        public String ImageResource { get { return Project.GetDirectory(ProjectInfo.PackageResourcesPath, Source); } }
        public bool IsEdited { get; set; }

        #region ----------- NEW PROJECT ------------
        public ImageBlock(int id, XElement node, Content content)
        {
            ID = id;
            Content = content;
            Text = " ";
            foreach (XAttribute attribute in node.Attributes())
            {
                switch (attribute.Name.ToString())
                {
                    case "src": Source = attribute.Value; break;
                    case "alt": Text = attribute.Value; break;
                }
            }
            Sentences = new LinkedList<Sentence>();
            foreach (int startIdx in Split(Text))
                new Sentence(startIdx, this);
            using (ZipArchive archive = ZipFile.Open(ProjectInfo.EpubPath, ZipArchiveMode.Read))
            {
                String imgZipPath = Path.Combine(ProjectInfo.PackageName, Source);
                ZipArchiveEntry imgEntry = archive.GetEntry(imgZipPath.Replace('\\', '/'));
                if (imgEntry != null)
                    imgEntry.ExtractToFile(ImageResource);
            }
        }
        #endregion

        #region ----------- SAVE PROJECT ------------
        public override XElement ToXml()
        {
            XElement xBlock = new XElement("ImageBlock");
            xBlock.Add(new XAttribute("id", B_ID));
            xBlock.Add(new XAttribute("src", Source));
            xBlock.Add(new XElement("Text", Text));
            XElement xSentences = new XElement("Sentences");
            foreach (Sentence sentence in Sentences)
                xSentences.Add(sentence.ToXml());
            xBlock.Add(xSentences);
            IsEdited = false;
            return xBlock;
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
        public ImageBlock(XElement xBlock, Content content)
        {
            Content = content;
            ID = int.Parse(xBlock.Attribute("id").Value.Split('-')[1]);
            Source = xBlock.Attribute("src").Value;
            Text = xBlock.Element("Text").Value;
            if (String.IsNullOrWhiteSpace(Text))
                Text = " ";
            Sentences = new LinkedList<Sentence>();
            foreach (XElement xSentence in xBlock.Element("Sentences").Elements("Sentence"))
                new Sentence(xSentence, this);
        }
        #endregion
        
        #region ----------- EDIT PROJECT ------------
        public void SetAltText(String text)
        {
            Text = text;
            Sentences = new LinkedList<Sentence>();
            foreach (int startIdx in Split(Text))
                new Sentence(startIdx, this);

            foreach (Sentence sentence in Sentences)
                sentence.Synthesize();
            IsEdited = true;
            Content.Changed = true;
        }
        #endregion
    }
}
