using Emgu.CV;
using Emgu.CV.Util;
using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DominoPlanner.Core
{
    [ProtoContract(SkipConstructor =true)]
    public class DominoTransfer : ICloneable
    {
        //[ProtoMember(2, IsPacked = true)]
        //public int[] dominoes { get; set; }
        [ProtoMember(1)]
        public IDominoShape[] shapes;
        [ProtoMember(3, AsReference = true)]
        ColorRepository colors;
        [ProtoMember(4, AsReference = true)]
        public IterationInformation iterationInfo {get; set;}
        public int length
        {
            get { return shapes.Length; }
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
        public IDominoShape this[int index]
        {
            get
            {
                return shapes[index];
            }
        }
        public DominoTransfer(IDominoShape[] shapes, ColorRepository colors)
        {
            this.shapes = shapes;
            this.colors = colors;
        }
        public Mat GenerateImage(int targetWidth = 0, bool borders = false)
        {
            return GenerateImage(Colors.White, targetWidth, borders);
        }
        public Mat GenerateImage(Color background, int targetWidth = 0, bool borders = false)
        {

            double scalingFactor = 1;
            int width = shapes.Max(s => s.GetContainer().x2);
            int heigth = shapes.Max(s => s.GetContainer().y2);
            if (targetWidth != 0)
            {
                // get dimensions of the structure
                
                int x_min = shapes.Min(s => s.GetContainer().x1);
                int y_min = shapes.Min(s => s.GetContainer().y1);
                scalingFactor = (double)targetWidth / width;
            }
            Image<Emgu.CV.Structure.Bgra, byte> bitmap
                = new Image<Emgu.CV.Structure.Bgra, byte>((int)(width * scalingFactor), (int)(heigth * scalingFactor),
                new Emgu.CV.Structure.Bgra() { Alpha = background.A, Blue = background.B, Green = background.G, Red = background.R });
            

            Parallel.For(0, shapes.Length, (i) =>
           {
               Color c = colors[shapes[i].color].mediaColor;

               if (shapes[i] is RectangleDomino)
               {
                   DominoRectangle rect = shapes[i].GetContainer(scalingFactor);
                   CvInvoke.Rectangle(bitmap, new System.Drawing.Rectangle() { X = (int)rect.x, Y = (int)rect.y,
                       Width = (int)rect.width, Height = (int)rect.height }, new Emgu.CV.Structure.MCvScalar(c.B, c.G, c.R, c.A), -1, Emgu.CV.CvEnum.LineType.AntiAlias);
                   if (borders)
                   {
                       CvInvoke.Rectangle(bitmap, new System.Drawing.Rectangle()
                       {
                           X = (int)rect.x,
                           Y = (int)rect.y,
                           Width = (int)rect.width,
                           Height = (int)rect.height
                       }, new Emgu.CV.Structure.MCvScalar(0, 0, 0, 255), 1, Emgu.CV.CvEnum.LineType.AntiAlias);

                   }
               }
               else
               {
                   DominoPath shape = shapes[i].GetPath(scalingFactor);
                   bitmap.FillConvexPoly(shape.getSDPath(),
                       new Emgu.CV.Structure.Bgra(c.B, c.G, c.R, c.A), Emgu.CV.CvEnum.LineType.AntiAlias);
                   if (borders)
                   {
                       bitmap.DrawPolyline(shape.getSDPath(), true, 
                           new Emgu.CV.Structure.Bgra(0, 0, 0, 255), 1, Emgu.CV.CvEnum.LineType.AntiAlias);
                   }
               }
           });
            return bitmap.Mat;
        }

        public object Clone()
        {
            return Serializer.DeepClone<DominoTransfer>(this);
        }
    }

    
}
