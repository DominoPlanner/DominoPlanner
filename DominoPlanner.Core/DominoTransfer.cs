using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core
{
    
    public class DominoTransfer
    {
        public int[] dominoes { get; set; }
        IDominoShape[] shapes;
        List<DominoColor> colors;
        public int length
        {
            get { return dominoes.Length; }
        }
        public int dominoLength
        {
            get
            {
                return shapes.Max(x => (x.position != null) ? x.position.x : 0) + 1;
            }
        }
        public int dominoHeight
        {
            get
            {
                return shapes.Max(y => (y.position != null) ? y.position.y : 0) + 1;
            }
        }
        public Tuple<IDominoShape, Color, string> this[int index]
        {
            get
            {
                return new Tuple<IDominoShape, Color, string>(shapes[index], colors[dominoes[index]].mediaColor, colors[dominoes[index]].name);
            }
        }
        public DominoTransfer(int[] dominoColors, IDominoShape[] shapes, List<DominoColor> colors)
        {
            if (dominoColors.Length != shapes.Length) throw new InvalidOperationException("Colors and shapes must have the same length");
            this.dominoes = dominoColors;
            this.shapes = shapes;
            this.colors = colors;
        }
        public WriteableBitmap GenerateImage(int targetWidth, bool borders = false)
        {

            // get dimensions of the structure
            int width = shapes.Max(s => s.GetContainer().x2);
            int heigth = shapes.Max(s => s.GetContainer().y2);
            double scalingFactor = (double)targetWidth / width;
            WriteableBitmap bitmap = BitmapFactory.New((int)(width * scalingFactor), (int)(heigth * scalingFactor));
            using (bitmap.GetBitmapContext())
            {
                bitmap.Clear(Colors.White);
                for (int i = 0; i < dominoes.Length; i++)
                {
                    DominoPath shape = this[i].Item1.GetPath(scalingFactor);
                    Color c = this[i].Item2;
                    if (!borders)
                    {
                        bitmap.FillPolygon(shape.getWBXPath(), c);
                    }
                    else
                    {
                        
                        //bitmap.FillPolygon(shape.GetPath(scalingFactor).getWBXPath(), c);
                        //for (int k = 0; k < shape.points.Length - 1; k++)
                        //{
                         //   bitmap.AaWidthLine(shape.points[k].X, shape.points[k].Y, shape.points[k + 1].Y, shape.points[k + 1].Y, Colors.Black, scalingFactor)
                        //}

                        //bitmap.DrawPolyline(shape.GetPath(scalingFactor).getWBXPath(), Colors.Black);
                        //bitmap.FillPolygon(shape.GetPath(scalingFactor).getOffsetRectangle((int)Math.Ceiling(scalingFactor)).getWBXPath(), Colors.Black); // outline
                        //bitmap.FillPolygon(shape.GetPath(scalingFactor).getOffsetRectangle(-(int)Math.Ceiling(scalingFactor)).getWBXPath(), c); // fill
                    }
                }
            }
            return bitmap;
        }
    }
}
