using eBdb.EpubReader;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Content
    {
        private int count;

        public String CID { get { return "C" + Order; } }

        public String NavID { get; private set; }
        public XElement Root { get; private set; }
        public XNamespace Xns { get; private set; }
        public String Source { get; private set; }
        public String Title { get; private set; }
        public int Order { get; private set; }
        public List<Block> Blocks { get; private set; }

        #region ----------- NEW PROJECT ------------

        #region Constructor
        public Content(NavPoint Nav) 
        {
            NavID = Nav.ID;
            Root = XElement.Parse(Nav.ContentData.Content);
            Xns = Root.Attribute("xmlns") != null ? Root.Attribute("xmlns").Value : XNamespace.None;
            Source = Nav.Source;
            Title = Nav.Title;
            Order = Nav.Order;
            GetBlocks();
        }
        #endregion

        #region Private Methods
        private void GetBlocks()
        {
            count = 0;
            Blocks = new List<Block>();
            GetBlocks(Root.Element(Xns + "body"));
        }

        private void GetBlocks(XElement CurNode)
        {
            foreach (XNode ChildNode in CurNode.Nodes())
            {
                if (ChildNode is XText)
                {
                    XText TextNode = ChildNode as XText;
                    Block block = new Block(count++, TextNode.Value, this);
                    TextNode.Value = block.BID;
                    Blocks.Add(block);
                }
                else if (ChildNode is XElement)
                    GetBlocks(ChildNode as XElement);
            }
        }
        #endregion

        public XElement ToXml()
        {
            XElement xContent = new XElement("Content");
            xContent.Add(new XAttribute("id", NavID));
            xContent.Add(new XAttribute("src", Source));
            xContent.Add(new XAttribute("title", Title));
            xContent.Add(new XAttribute("order", Order));
            foreach (Block block in Blocks)
                xContent.Add(block.ToXml());
            return xContent;
        }
        #endregion

        #region ----------- OPEN PROJECT ------------
        #endregion
    }
}