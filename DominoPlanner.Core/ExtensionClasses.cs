using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using Color = Avalonia.Media.Color;

namespace DominoPlanner.Core
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Invertiert eine Farbe.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color Invert(this Color c)
        {
            return Color.FromArgb(255, (byte)(255 - c.R), (byte)(255 - c.G), (byte)(255 - c.B));
        }
        /// <summary>
        /// Berechnet, ob schwarz oder weiß auf der angegebenen Farbe besser lesbar ist und gibt diese zurück.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color IntelligentBW(this Color c)
        {
            if (c.R * .3d + c.G * .59 + c.B * 0.11 > 128)
            {
                return Colors.Black;
            }
            return Colors.White;
        }
        /// <summary>
        /// Gibt eine String-Repräsentation der Farbe im Format #rrggbb zurück.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ToHTML(this Color c)
        {
            return string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
        }
        /// <summary>
        /// Konvertiert die Farbe in eine System.Drawing.Color.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static System.Drawing.Color ToSD(this Color c, bool removeTransparency= false)
        {
            return System.Drawing.Color.FromArgb(removeTransparency ? 255 :c.A, c.R, c.G, c.B);
        }
        /// <summary>
        /// Konvertiert die Farbe in den Lab-Farbraum.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        
        public static void Save(this SkiaSharp.SKSurface surf, string filename)
        {
            using (var image = surf.Snapshot().Encode())
            {
                using (var stream = File.OpenWrite(filename))
                {
                    image.SaveTo(stream);
                }
            }
        }

        public static void Save(this SkiaSharp.SKSurface surf, string filename, int dpi)
        {
            Save(surf, filename, dpi, dpi);
        }

        public static void Save(this SkiaSharp.SKSurface surf, string filename, int dpiX, int dpiY)
        {
            using (var image = surf.Snapshot().Encode())
            {
                using (MemoryStream ms = new MemoryStream(image.ToArray()))
                {
                    using (Bitmap bitmap = new Bitmap(ms))
                    {
                        bitmap.SetResolution(dpiX, dpiY);
                        bitmap.Save(filename, ImageFormat.Png);
                    }
                }
            }
        }
    }
}
