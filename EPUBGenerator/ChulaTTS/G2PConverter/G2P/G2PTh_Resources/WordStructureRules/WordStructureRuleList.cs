using System;

using System.Collections.Generic;
using System.Text;

namespace ThaiSpeechSynthesizer
{
    class WordStructureRuleList : List<IWordStructureRule>
    {
        public WordStructureRuleList(params object[] objects)
            : base()
        {
            foreach (object obj in objects)
            {
                if (obj is IWordStructureRule)
                    this.Add((IWordStructureRule)obj);
                else if (obj is char)
                    this.Add(WordStructureChar.FromChar((char)obj));
                else if (obj is string)
                {
                    string s = (string)obj;
                    if (s.Length > 0)
                        this.Add(new WordStructureString(s));
                } else
                    throw new ArgumentException("Mismatched type of arguments.");
            }
        }
    }
}
