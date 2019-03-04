using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Usage.HelperClass
{
    public static class ImageConvert
    {
        public static WriteableBitmap ToWriteableBitmap(Bitmap source)
        {
            return new WriteableBitmap(ToBitmapSource(source));
        }

        public static BitmapImage ToBitmapSource(Bitmap source)
        {
            if (source == null) return new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                source.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
    }
}
