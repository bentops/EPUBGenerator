﻿using System;
using System.Collections.Generic;
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
        }
        #endregion
    }
}