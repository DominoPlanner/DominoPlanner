using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using Avalonia.Controls;
using Avalonia;
using System.Linq;
using System.IO;
using Avalonia.Media;

namespace DominoPlanner.Usage
{
    public class ProjectCanvas : Canvas
    {
        public List<DominoInCanvas> Stones = new List<DominoInCanvas>();

        public Color UnselectedBorderColor { get; set; }

        public Color SelectedBorderColor { get; set; }
        public double BorderSize;

        public Image<Emgu.CV.Structure.Bgra, byte> OriginalImage;

        public bool above;
        private double opacity_value;

        public double OpacityValue
        {
            get { return opacity_value; }
            set { opacity_value = value; }
        }
        

        public override void Render(DrawingContext dc)
        {
            IImage bit = null;
            if (opacity_value != 0)
            {
                var reduced = OriginalImage.Clone();
                Core.ImageExtensions.OpacityReduction(reduced, opacity_value);
                //bit = BitmapSourceConvert.ToBitmapSource(reduced);
            }
            base.Render(dc);

            var unselectedBrush = new SolidColorBrush(UnselectedBorderColor);
            var selectedBrush = new SolidColorBrush(SelectedBorderColor);
            if (!above && bit != null)
                dc.DrawImage(bit, new Rect(0, 0, Width, Height));
            foreach (DominoInCanvas dic in Stones)
            {
                Point point1 = dic.canvasPoints[0];
                Point point2 = dic.canvasPoints[1];
                Point point3 = dic.canvasPoints[2];
                Point point4 = dic.canvasPoints[3];

                StreamGeometry streamGeometry = new StreamGeometry();
                using (StreamGeometryContext geometryContext = streamGeometry.Open())
                {
                    geometryContext.BeginFigure(point1, true);
                    geometryContext.LineTo(point2);
                    geometryContext.LineTo(point3);
                    geometryContext.LineTo(point4);
                    geometryContext.EndFigure(true);
                }

                Pen pen = new Pen();
                if (dic.isSelected)
                {
                    pen.Brush = selectedBrush;
                    pen.Thickness = BorderSize;
                }
                else if (dic.PossibleToPaste)
                {
                    pen.Brush = Brushes.Plum;
                    pen.Thickness = BorderSize;
                }
                else
                {
                    pen.Brush = unselectedBrush;
                    pen.Thickness = BorderSize / 2;
                }

                dc.DrawGeometry(new SolidColorBrush(dic.StoneColor), pen, streamGeometry);
            }
            if (above && bit != null)
                dc.DrawImage(bit, new Rect(0, 0, Width, Height));

        }
    }
    public static class BitmapSourceConvert
    {
        /*[DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }*/
    }
}
