﻿using eBdb.EpubReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Content
    {
        public String CID { get { return "C" + Order; } }

        public String NavID { get; private set; }
        public XElement Root { get; private set; }
        public XNamespace Xns { get; private set; }
        public String Source { get; private set; }
        public String Title { get; private set; }
        public int Order { get; private set; }

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
        public Content(NavPoint Nav)
        {
            NavID = Nav.ID;
            Root = XElement.Parse(Nav.ContentData.Content);
            Xns = Root.Attribute("xmlns") != null ? Root.Attribute("xmlns").Value : XNamespace.None;
            Source = Nav.Source;
            Title = Nav.Title;
            Order = Nav.Order;

            Blocks = new List<Block>();
            GetBlocks(Root.Element(Xns + "body"));
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
        public XElement ToXml()
        {
            XElement xContent = new XElement("Content");
            xContent.Add(new XAttribute("id", NavID));
            xContent.Add(new XAttribute("title", Title));
            xContent.Add(new XAttribute("order", Order));
            foreach (Block block in Blocks)
                xContent.Add(block.ToXml());
            return xContent;
        }
        public void RunSentenceID()
        {
            int count = 0;
            foreach (Block block in Blocks)
            {
                Sentence.RunID(block.Sentences, count);
                count += block.Sentences.Count;
            }
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
        public Content(String contentPath, ProjectInfo projectInfo)
        {
            Source = contentPath;
            using (StreamReader streamReader = new StreamReader(projectInfo.GetContentResource(this)))
            {
                Root = XElement.Parse(streamReader.ReadToEnd());
                Xns = Root.Attribute("xmlns") != null ? Root.Attribute("xmlns").Value : XNamespace.None;
                streamReader.Close();
            }
            using (StreamReader streamReader = new StreamReader(projectInfo.GetContentSave(this)))
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