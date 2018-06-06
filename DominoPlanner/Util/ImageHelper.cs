using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Util
{
    public static class ImageHelper
    {
        public static BitmapImage BitmapToBitmapImage(Bitmap b)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                b.Save(memory, System.Drawing.Imaging.ImageFormat.Tiff);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }
        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource BitmapToBitmapSource(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }
        public static Bitmap BitmapImageToBitmap(BitmapImage b)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(b));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
        public static BitmapImage WriteableBitmapToBitmapImage(WriteableBitmap wbm)
        {
            BitmapImage bmImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(wbm));
                encoder.Save(stream);
                bmImage.BeginInit();
                bmImage.CacheOption = BitmapCacheOption.OnLoad;
                bmImage.StreamSource = stream;
                bmImage.EndInit();
                bmImage.Freeze();
            }
            return bmImage;
        }
    }
}
