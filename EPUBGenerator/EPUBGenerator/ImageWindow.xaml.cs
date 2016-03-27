using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EPUBGenerator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ImageWindow : Window
    {
        public ImageWindow(string imgPath)
        {
            InitializeComponent();
            img.Source = new BitmapImage(new Uri(imgPath));
            GetImageSize();
        }

        private void GetImageSize()
        {
            double imgW = img.Source.Width;
            double imgH = img.Source.Height;
            if(imgH <= 600 && imgW <= 800)
            {
                img.Height = imgH;
                img.Width = imgW;
            }
            else if(imgH > 600)
            {
                img.Height = 600;
                img.Width = imgW * 600 / imgH;
            }
            else if(imgW > 800)
            {
                img.Width = 800;
                img.Height = imgH * 800 / imgH;
            }
        }
    }
}
