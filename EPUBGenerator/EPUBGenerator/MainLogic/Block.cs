﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EPUBGenerator.MainLogic
{
    class Block
    {
        public int ID { get; set; }
        public String BID { get { return "B" + ID; } }
        public List<Sentence> Sentences { get; set; }

        public String Text
        {
            get
            {
                if (Sentences == null)
                    return null;
                String text = "";
                foreach (Sentence s in Sentences)
                    text += s.Text + " ";
                return Text;
            }
        }

        public Block(int id, String text)
        {
            ID = id;
            // DO SOME SPLIT HERE //
            int count = 0;
            Sentences = new List<Sentence>();
            foreach (KeyValuePair<String, Int32> sentence in Tools.SentenceSplitter.Split(text))
            {
                if (String.IsNullOrWhiteSpace(sentence.Key)) continue;
                Sentences.Add(new Sentence(count++, sentence.Value, sentence.Key, this));
            }
        }

        public Block(XElement xBlock)
        {
            Sentences = new List<Sentence>();
            foreach (XElement element in xBlock.Descendants())
            {
                switch(element.Name.ToString())
                {
                    case "Sentence": Sentences.Add(new Sentence(element, this)); break;
                    default: break;
                }
            }
        }

        public XElement ToXml()
        {
            XElement xBlock = new XElement("Block", new XAttribute("id", BID));
            foreach (Sentence sentence in Sentences)
                xBlock.Add(sentence.ToXml());
            return xBlock;
        }
    }
}
