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

        public ColorRepository colorRepository;
        public IDominoShape domino;

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

        private bool _PossibleToPaste;

        public bool PossibleToPaste
        {
            get { return _PossibleToPaste; }
            set
            {
                if(_PossibleToPaste != value)
                {
                    _PossibleToPaste = value;
                    if (value)
                    {
                        Stroke = Brushes.Plum;
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

        public DominoInCanvas(int idx, IDominoShape domino, ColorRepository colorlist)
        {
            colorRepository = colorlist;
            this.idx = idx;
            this.ToolTip = idx.ToString();
            StoneColor = colorlist[domino.color].mediaColor;
            this.domino = domino;
            domino.ColorChanged += Domino_ColorChanged;
            Stroke = Brushes.Blue;
            StrokeThickness = 2;
            DominoPath rectangle = domino.GetPath();
            canvasPoints[0] = new System.Windows.Point(rectangle.points[0].X, rectangle.points[0].Y);
            canvasPoints[1] = new System.Windows.Point(rectangle.points[1].X, rectangle.points[1].Y);
            canvasPoints[2] = new System.Windows.Point(rectangle.points[2].X, rectangle.points[2].Y);
            canvasPoints[3] = new System.Windows.Point(rectangle.points[3].X, rectangle.points[3].Y);
        }

        private void Domino_ColorChanged(object sender, System.EventArgs e)
        {
            StoneColor = colorRepository[(sender as IDominoShape).color].mediaColor;
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

        public void DisposeStone()
        {
            domino.ColorChanged -= Domino_ColorChanged; //jojoasdf - nurnoch auch wieder aufrufen eim abbauen :D
        }
    }
}
