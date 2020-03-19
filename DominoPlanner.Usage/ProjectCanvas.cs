using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;
using DominoPlanner.Core;

namespace DominoPlanner.Usage
{
    public class ProjectCanvas : Canvas
    {
        public List<DominoInCanvas> Stones = new List<DominoInCanvas>();

        public Color UnselectedBorderColor { get; set; }

        public Color SelectedBorderColor { get; set; }
        public double BorderSize;

        public Image<Emgu.CV.Structure.Bgra, byte> OriginalImage;

        public double[] gridlines_x = new double[] { }, gridlines_y = new double[] { };

        public bool above;
        private double opacity_value;

        public double OpacityValue
        {
            get { return opacity_value; }
            set { opacity_value = value; }
        }
        

        protected override void OnRender(DrawingContext dc)
        {
            BitmapSource bit = null;
            if (opacity_value != 0)
            {
                var reduced = OriginalImage.Clone();
                Core.ImageExtensions.OpacityReduction(reduced, opacity_value);
                bit = BitmapSourceConvert.ToBitmapSource(reduced);
            }
            base.OnRender(dc);


            if (!above && bit != null)
                dc.DrawImage(bit, new Rect(0, 0, Width, Height));
            foreach (DominoInCanvas dic in Stones)
            {

                StreamGeometry streamGeometry = new StreamGeometry();
                using (StreamGeometryContext geometryContext = streamGeometry.Open())
                {
                    geometryContext.BeginFigure(dic.canvasPoints[0], true, true);
                    var points = new System.Windows.Media.PointCollection(dic.canvasPoints.Skip(1));
                    geometryContext.PolyLineTo(points, true, true);
                }

                Pen pen = new Pen();
                if (dic.isSelected)
                {
                    pen.Brush = new SolidColorBrush(dic.StoneColor.IntelligentBW(Colors.Black, Colors.LightGray)); 
                    pen.Thickness = BorderSize;
                    pen.DashStyle = DashStyles.Dash;
                }
                else if (dic.PossibleToPaste)
                {
                    pen.Brush = Brushes.Plum;
                    pen.Thickness = BorderSize;
                }
                else
                {
                    pen.Brush = new SolidColorBrush(UnselectedBorderColor);
                    pen.Thickness = BorderSize / 2;
                }

                dc.DrawGeometry(new SolidColorBrush(dic.StoneColor), pen, streamGeometry);
                // Point in the center
                /* if (dic.isSelected)
                {
                    var center_x = dic.canvasPoints.Sum(x => x.X) / dic.canvasPoints.Length;
                    var center_y = dic.canvasPoints.Sum(x => x.Y) / dic.canvasPoints.Length;
                    dc.DrawEllipse(new SolidColorBrush(dic.StoneColor.IntelligentBW()), null,
                        new System.Windows.Point(center_x, center_y), BorderSize / 2, BorderSize/2);
                } */
            }
            if (above && bit != null)
                dc.DrawImage(bit, new Rect(0, 0, Width, Height));
            var LinePen = new Pen(Brushes.Black, 1);
            var LineContrastPen = new Pen(Brushes.White, 1);
            foreach (int x in gridlines_x)
            {
                dc.DrawLine(LineContrastPen, new System.Windows.Point(x-1, 0), new System.Windows.Point(x-1, Height));
                dc.DrawLine(LinePen, new System.Windows.Point(x, 0), new System.Windows.Point(x, Height));
                dc.DrawLine(LineContrastPen, new System.Windows.Point(x + 1, 0), new System.Windows.Point(x + 1, Height));
            }
            foreach (int y in gridlines_y)
            {
                dc.DrawLine(LineContrastPen, new System.Windows.Point(0, y-1), new System.Windows.Point(Width, y-1));
                dc.DrawLine(LinePen, new System.Windows.Point(0, y), new System.Windows.Point(Width, y));
                dc.DrawLine(LineContrastPen, new System.Windows.Point(0, y + 1), new System.Windows.Point(Width, y + 1));
            }
        }
        public DominoInCanvas FindDominoAtPosition(System.Windows.Point pos)
        {
            double min_dist = int.MaxValue;
            DominoInCanvas result = null;
            foreach (var shape in Stones)
            {
                if (shape.domino.IsInside(new Core.Point(pos.X, pos.Y))) return shape;
                var rect = shape.domino.getBoundingRectangle();
                double dist = Math.Pow((rect.x + rect.width / 2) - pos.X, 2) + Math.Pow(rect.y + rect.height / 2 - pos.Y, 2);
                if (min_dist > dist)
                {
                    min_dist = dist;
                    result = shape;

                }
            }
            return result;
        }
    }
    public static class BitmapSourceConvert
    {
        [DllImport("gdi32")]
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
        }
    }
}
