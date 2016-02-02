using ChulaTTS.G2PConverter;
using eBdb.EpubReader;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Content
    {
        private int count;

        public String NavID { get; private set; }
        public XElement Root { get; private set; }
        public XNamespace Xns { get; private set; }
        public String Source { get; private set; }
        public String Title { get; private set; }
        public int Order { get; private set; }
        public List<Block> Blocks { get; private set; }

        public Content(NavPoint Nav) 
        {
            NavID = Nav.ID;
            Root = XElement.Parse(Nav.ContentData.Content);
            Xns = Root.Attribute("xmlns") != null ? Root.Attribute("xmlns").Value : XNamespace.None;
            Source = Nav.Source;
            Title = Nav.Title;
            Order = Nav.Order;

            try
            {
                GetBlocks();
            }
            catch (Exception e)
            {
                Console.WriteLine("EXCEPTION FOUND IN CONTENT: " + e.StackTrace);
            }
        }

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
                    Block block = new Block(count++, TextNode.Value);
                    TextNode.Value = block.BID;
                    Blocks.Add(block);
                }
                else if (ChildNode is XElement)
                    GetBlocks(ChildNode as XElement);
            }
        }

        /*
        private void GetBlocks(XElement CurNode)
        {
            List<XText> DelList = new List<XText>();
            List<XElement> InsList = new List<XElement>();
            foreach (XNode ChildNode in CurNode.Nodes())
            {
                if (ChildNode is XText)
                {
                    // DO SOME SPLITTER HERE //
                    XText TextNode = ChildNode as XText;
                    List<KeyValuePair<String, Int32>> SList = Tools.SentenceSplitter.Split(TextNode.Value);
                    foreach (KeyValuePair<String, Int32> Sp in SList)
                    {
                        if (String.IsNullOrWhiteSpace(Sp.Key)) continue;
                        Sentence St = new Sentence(Sp.Key, Sp.Value);
                        XAttribute AttrID = new XAttribute("id", "S" + count.ToString("D8"));
                        InsList.Add(new XElement(Xns + "span", new XText(St.Text), AttrID));
                        count++;
                    }
                    DelList.Add(TextNode);
                }
                else if (ChildNode is XElement)
                    this.GetBlocks(ChildNode as XElement);
            }

            foreach (XText Text in DelList)
                Text.Remove();

            CurNode.Add(InsList);
        }
        */
    }
}