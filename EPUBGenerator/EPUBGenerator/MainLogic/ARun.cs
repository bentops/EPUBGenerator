using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace EPUBGenerator.MainLogic
{
    abstract class ARun : Run
    {
        public Paragraph Paragraph { get { return Parent as Paragraph; } }
        public Paragraph NextParagraph { get { return Paragraph == null ? null : Paragraph.NextBlock as Paragraph; } }
        public Paragraph PreviousParagraph { get { return Paragraph == null ? null : Paragraph.PreviousBlock as Paragraph; } }

        abstract public bool IsImage { get; }

        public ARun() : base() { }
        public ARun(String text) : base(text) { }
        public ARun(String text, TextPointer pointer) : base(text, pointer) { }

        abstract public void PlayCachedSound();

        abstract public bool IsSelected { get; }
        abstract public void Select();

        public bool IsHovered { get; private set; }
        public void Hover()
        {
            IsHovered = true;
            UpdateBackground();
        }
        public void Unhover()
        {
            IsHovered = false;
            UpdateBackground();
        }

        abstract public bool IsEdited { get; }
        abstract public void UpdateSegmentedBackground();
        abstract public void UpdateBackground();

        abstract public ARun LogicalPrevious();
        abstract public ARun LogicalNext();
    }
}
