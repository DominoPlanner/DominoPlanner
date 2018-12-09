using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using DominoPlanner.Core;

namespace DominoPlanner.Usage
{
    public class DominoInCanvas : Shape
    {
        public int idx;
        public System.Windows.Point[] canvasPoints = new System.Windows.Point[4];
        
        private Color _StoneColor;
        public Color StoneColor
        {
            get { return _StoneColor; }
            set
            {
                if (_StoneColor != value)
                {
                    _StoneColor = value;
                    this.Fill = new SolidColorBrush(StoneColor);
                }
            }
        }

        private bool _isSelected;
        public bool isSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    if (value)
                    {
                        Stroke = Brushes.Red;
                        StrokeThickness = 5;
                    }
                    else
                    {
                        Stroke = Brushes.Blue;
                        StrokeThickness = 2;
                    }
                }
            }
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                StreamGeometry geometry = new StreamGeometry();
                geometry.FillRule = FillRule.EvenOdd;

                using (StreamGeometryContext context = geometry.Open())
                {
                    DrawGeometry(context);
                }

                geometry.Freeze();
                return geometry;
            }
        }

        public DominoInCanvas(int idx, DominoPath rectangle, Color color)
        {
            this.idx = idx;
            this.ToolTip = idx.ToString();
            StoneColor = color;
            Stroke = Brushes.Blue;
            StrokeThickness = 2;

            canvasPoints[0] = rectangle.points[0];
            canvasPoints[1] = rectangle.points[1];
            canvasPoints[2] = rectangle.points[2];
            canvasPoints[3] = rectangle.points[3];
        }

        public DominoInCanvas(int stoneWidth, int stoneHeight, int marginLeft, int marginTop, Color color)
        {
            this.Fill = new SolidColorBrush(color);
            this.Width = stoneWidth;
            this.Height = stoneHeight;
            Stroke = Brushes.Blue;
            StrokeThickness = 2;
            this.Margin = new Thickness(marginLeft, marginTop, 0, 0);

            canvasPoints[0] = new System.Windows.Point(0, 0);
            canvasPoints[1] = new System.Windows.Point(stoneWidth, 0);
            canvasPoints[2] = new System.Windows.Point(stoneWidth, stoneHeight);
            canvasPoints[3] = new System.Windows.Point(0, stoneHeight);
        }

        private void DrawGeometry(StreamGeometryContext context)
        {
            context.BeginFigure(canvasPoints[0], true, true); //Top Left
            IList<System.Windows.Point> points = new List<System.Windows.Point>();
            points.Add(canvasPoints[1]); //Top Right
            points.Add(canvasPoints[2]); // Bottom Right
            points.Add(canvasPoints[3]); // Bottom Left
            context.PolyLineTo(points, true, true);
        }
    }
}
