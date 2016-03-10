using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EPUBGenerator.MainLogic
{
    static class ProjectProperties
    {
        private static BrushConverter Converter;
        private static Brush LightPink = Brush("#FFC0CB");
        private static Brush DarkPink = Brush("#FF99A7");
        private static Brush LightYellow = Brush("#FFFFCB");
        private static Brush DarkYellow = Brush("#FDFF7F");
        private static Brush LightBlue = Brush("#CDEAFD");
        private static Brush DarkBlue = Brush("#84CCFA");
        private static Brush LightOrange = Brush("#FFDC7F");
        private static Brush DarkOrange = Brush("#FFC833");
        private static Brush LightPurple = Brush("#F1B1FF");
        private static Brush DarkPurple = Brush("#E87FFF");

        public static Brush Transparent = Brush("#00000000");

        public static Brush EditedWord = DarkPink;

        public static Brush PlayedSentence = LightYellow;
        public static Brush PlayedWord = DarkYellow;

        public static Brush EditingWord = LightPink;
        public static Brush HoveredWord = LightYellow;
        public static Brush SelectedWord = DarkYellow;

        public static Brush[] CutWords = new Brush[] { LightBlue, DarkBlue };
        public static Brush[] SplittedWords = new Brush[] { LightOrange, DarkOrange };
        public static Brush[] MergedWords = new Brush[] { LightPurple, DarkPurple };

        private static Brush Brush(String hexValue)
        {
            if (Converter == null)
                Converter = new BrushConverter();
            return Converter.ConvertFrom(hexValue) as SolidColorBrush;
        }

        public static int MinRandomValue = 900000;
        public static int MaxRandomValue = 1000000;
        public static int Digits = 6;
        public static String NonRandomPattern = "S0*.wav";
        public static String RandomPattern = "S9*.wav";
    }
}
