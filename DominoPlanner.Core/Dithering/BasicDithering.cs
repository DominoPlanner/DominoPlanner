using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DominoPlanner.Core.Dithering
{
    public class Dithering
    {
        protected DitherMode mode = DitherMode.NoDithering;
        public DitherMode Mode { get { return mode; } }

        // Die Dithering-Verfahren müssen sequenziell ausgeführt werden.
        // Die Basisimplementierung kann aber multicore laufen
        public int maxDegreeOfParallelism = -1;

        public virtual void DiffuseError(int x, int y, int v1, int v2, int v3, Image<Emgu.CV.Structure.Bgra, byte> bitmap)
        {
            // do nothing in default implementation
        }

        protected void SetPixel(int x, int y, byte r, byte g, byte b, Image<Emgu.CV.Structure.Bgra, byte> bitmap)
        {
            if (x >= bitmap.Width) x = bitmap.Width - 1;
            else if (x < 0) x = 0;
            if (y >= bitmap.Height) y = bitmap.Height - 1;
            else if (y < 0) y = 0;
            bitmap.Data[y, x, 1] = g;
            bitmap.Data[y, x, 0] = b;
            bitmap.Data[y, x, 2] = r;

        }
        protected System.Windows.Media.Color GetPixel(int x, int y, Image<Emgu.CV.Structure.Bgra, byte> bitmap)
        {
            if (x >= bitmap.Width) x = bitmap.Width - 1;
            else if (x < 0) x = 0;
            if (y >= bitmap.Height) y = bitmap.Height - 1;
            else if (y < 0) y = 0;
            return new System.Windows.Media.Color() {
                R = bitmap.Data[y, x, 2],
                G = bitmap.Data[y, x, 1],
                B = bitmap.Data[y, x, 0]
            };
        }
        protected byte Scale(int value)
        {
            return (byte)((value > 255) ? 255 : ((value < 0) ? 0 : value));
        }
    }
}
