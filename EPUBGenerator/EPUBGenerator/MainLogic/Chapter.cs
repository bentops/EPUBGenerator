using ChulaTTS.G2PConverter;
using eBdb.EpubReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Chapter
    {
        public String ID { get; private set; }
        public XElement Root { get; private set; }
        public XNamespace Xns { get; private set; }
        public String Source { get; private set; }
        public String Title { get; private set; }
        public int Order { get; private set; }
        public List<Sentence> Sentences { get; private set; }

        private int count;

        public Chapter(NavPoint Nav) 
        {
            ID = Nav.ID;
            Root = XElement.Parse(Nav.ContentData.Content);
            Xns = Root.Attribute("xmlns") != null ? Root.Attribute("xmlns").Value : XNamespace.None;
            Source = Nav.Source;
            Title = Nav.Title;
            Order = Nav.Order;
            Sentences = new List<Sentence>();

            count = 0;
        }

        public void GetContent()
        {
            GetContent(Root.Element(Xns + "body"));

        }

        public void GetContent(XElement CurNode)
        {
            List<XText> DelList = new List<XText>();
            List<XElement> InsList = new List<XElement>();
            foreach (XNode ChildNode in CurNode.Nodes())
            {
                if (ChildNode is XText)
                {
                    // DO SOME SPLITTER HERE //
                    XText TextNode = ChildNode as XText;
                    List<KeyValuePair<String, Int32>> SList = SentenceSplitter.Split(TextNode.Value);
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
                    this.GetContent(ChildNode as XElement);
            }
            foreach (XText Text in DelList)
                Text.Remove();
            CurNode.Add(InsList);
        }
    }
}
