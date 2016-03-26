using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class ContentBlock : Block
    {
        public override string B_ID { get { return ProjectProperties.ContentBlockID(ID); } }
        
        #region ----------- NEW PROJECT ------------
        public ContentBlock(int id, String text, Content content)
        {
            ID = id;
            Text = text;
            Content = content;

            Sentences = new LinkedList<Sentence>();
            foreach (int startIdx in Split(Text))
                new Sentence(startIdx, this);
        }
        #endregion

        #region ----------- SAVE PROJECT ------------
        public override XElement ToXml()
        {
            XElement xBlock = new XElement("ContentBlock");
            xBlock.Add(new XAttribute("id", B_ID));
            xBlock.Add(new XElement("Text", Text));
            XElement xSentences = new XElement("Sentences");
            foreach (Sentence sentence in Sentences)
                xSentences.Add(sentence.ToXml());
            xBlock.Add(xSentences);
            return xBlock;
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
        public ContentBlock(XElement xBlock, Content content)
        {
            Content = content;
            ID = int.Parse(xBlock.Attribute("id").Value.Split('-')[1]);
            Text = xBlock.Element("Text").Value;
            Sentences = new LinkedList<Sentence>();
            foreach (XElement xSentence in xBlock.Element("Sentences").Elements("Sentence"))
                new Sentence(xSentence, this);
        }
        #endregion
    }
}
