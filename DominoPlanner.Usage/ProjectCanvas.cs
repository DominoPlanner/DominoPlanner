using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DominoPlanner.Usage
{
    public class ProjectCanvas : Canvas
    {
        public List<DominoInCanvas> Stones = new List<DominoInCanvas>();

        public PointCollection selectionArea;

        public Color UnselectedBorderColor { get; set; }

        public Color SelectedBorderColor { get; set; }
        public double BorderSize;

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            foreach (DominoInCanvas dic in Stones)
            {
                Point point1 = dic.canvasPoints[0];
                Point point2 = dic.canvasPoints[1];
                Point point3 = dic.canvasPoints[2];
                Point point4 = dic.canvasPoints[3];

                StreamGeometry streamGeometry = new StreamGeometry();
                using (StreamGeometryContext geometryContext = streamGeometry.Open())
                {
                    geometryContext.BeginFigure(point1, true, true);
                    PointCollection points = new PointCollection
                                             {
                                                 point2, point3, point4
                                             };
                    geometryContext.PolyLineTo(points, true, true);
                }

                Pen pen = new Pen();
                if (dic.isSelected)
                {
                    pen.Brush = new SolidColorBrush(SelectedBorderColor);
                    pen.Thickness = BorderSize;
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
            }

        }
    }
}
