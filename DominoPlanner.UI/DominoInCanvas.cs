using System.Collections.Generic;
using System.Windows;
using DominoPlanner.Core;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia;

namespace DominoPlanner.UI
{
    public class DominoInCanvas : Shape
    {
        public int idx;
        public Avalonia.Point[] canvasPoints = new Avalonia.Point[4];
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
                    refreshStroke();
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
                    refreshStroke();
                }
            }
        }
        protected override Geometry CreateDefiningGeometry()
        {
            StreamGeometry geometry = new StreamGeometry();
            //geometry.FillRule = FillRule.EvenOdd;

            using (StreamGeometryContext context = geometry.Open())
            {
                DrawGeometry(context);
            }

            //geometry.Freeze();
            return geometry;
        }

        public DominoInCanvas(int idx, IDominoShape domino, ColorRepository colorlist, bool showSpaces)
        {
            colorRepository = colorlist;
            this.idx = idx;
            StoneColor = colorlist[domino.color].mediaColor;
            this.domino = domino;
            domino.ColorChanged += Domino_ColorChanged;
            Stroke = Brushes.Blue;
            StrokeThickness = 1;
            DominoPath rectangle = domino.GetPath();
            if (domino is RectangleDomino rectangleDomino)
            {
                double stoneWidth = 0;
                double stoneHeight = 0;
                if (showSpaces)
                {
                    stoneHeight = rectangleDomino.height;
                    stoneWidth = rectangleDomino.width;
                }
                else
                {
                    stoneHeight = rectangleDomino.expanded_height;
                    stoneWidth = rectangleDomino.expanded_width;
                }
                canvasPoints[0] = new Avalonia.Point(rectangleDomino.x, rectangleDomino.y);
                canvasPoints[1] = new Avalonia.Point(rectangleDomino.x + stoneWidth, rectangleDomino.y);
                canvasPoints[2] = new Avalonia.Point(rectangleDomino.x + stoneWidth, rectangleDomino.y + stoneHeight);
                canvasPoints[3] = new Avalonia.Point(rectangleDomino.x, rectangleDomino.y + stoneHeight);
            }
            else
            {
                canvasPoints[0] = new Avalonia.Point(rectangle.points[0].X, rectangle.points[0].Y);
                canvasPoints[1] = new Avalonia.Point(rectangle.points[1].X, rectangle.points[1].Y);
                canvasPoints[2] = new Avalonia.Point(rectangle.points[2].X, rectangle.points[2].Y);
                canvasPoints[3] = new Avalonia.Point(rectangle.points[3].X, rectangle.points[3].Y);
            }
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

            canvasPoints[0] = new Avalonia.Point(0, 0);
            canvasPoints[1] = new Avalonia.Point(stoneWidth, 0);
            canvasPoints[2] = new Avalonia.Point(stoneWidth, stoneHeight);
            canvasPoints[3] = new Avalonia.Point(0, stoneHeight);
        }

        private void DrawGeometry(StreamGeometryContext context)
        {
            context.BeginFigure(canvasPoints[0], true); //Top Left
            IList< Avalonia.Point> points = new List<Avalonia.Point>();
            points.Add(canvasPoints[1]); //Top Right
            points.Add(canvasPoints[2]); // Bottom Right
            points.Add(canvasPoints[3]); // Bottom Left
            context.PolyLineTo(points, true, true);
        }

        public void DisposeStone()
        {
            domino.ColorChanged -= Domino_ColorChanged;
        }
        
        private void refreshStroke()
        {
            if (isSelected)
            {
                Stroke = Brushes.Red;
                StrokeThickness = 5;
            }
            else if (PossibleToPaste)
            {
                Stroke = Brushes.Plum;
                StrokeThickness = 5;
            }
            else
            {
                Stroke = Brushes.Blue;
                StrokeThickness = 1;
            }
        }
    }
}
