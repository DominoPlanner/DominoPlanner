using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using DominoPlanner.Document_Classes;
using DominoPlanner.Util;

namespace DominoPlanner
{
    public class DominoUI : Shape
    {
        public string path;
        public int stoneHeight;
        public int stoneWidth;
        public int arrayCounter;
        private List<DominoColor> dominocolor;
        private int _colorValue = -1;
        public int ColorValue
        {
            get { return _colorValue; }
            set
            {
                if (_colorValue != value)
                {
                    _colorValue = value;
                    if(dominocolor != null)
                        Fill = new SolidColorBrush(Color.FromRgb(dominocolor[value].rgb.R, dominocolor[value].rgb.G, dominocolor[value].rgb.B));
                }
            }
        }

        private int _selectionColorValue = -1;
        public int SelectionColorValue
        {
            get { return _selectionColorValue; }
            set
            {
                if (_selectionColorValue != value)
                {
                    _selectionColorValue = value;
                    if (dominocolor != null)
                        Stroke = new SolidColorBrush(Color.FromRgb(dominocolor[value].rgb.R, dominocolor[value].rgb.G, dominocolor[value].rgb.B));
                }
            }
        }
        private bool _isSelected;
        private List<DominoColor> colors;
        private DominoDefinition dominoDefinition;
        private int v;

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

        public DominoUI() { }

        public DominoUI(List<DominoColor> dominocolor, string path, int stoneWidth, int stoneHeight, int colorValue, int width, int height)
        {
            this.dominocolor = dominocolor;
            this.path = path;
            this.stoneWidth = stoneWidth;
            this.stoneHeight = stoneHeight;
            this.Width = width;
            this.Height = height;
            topLeft = new Point(0, 0);
            topRight = new Point(Width, 0);
            bottomRight = new Point(Width, Height);
            bottomLeft = new Point(0, Height);
            this.ColorValue = colorValue;
            Stroke = Brushes.Blue;
            StrokeThickness = 2;
        }

        public DominoUI(List<DominoColor> colors, string path, DominoDefinition dominoDefinition, int colorValue)
        {
            this.dominocolor = colors;
            this.path = path;
            this.dominoDefinition = dominoDefinition;
            this.ColorValue = colorValue;
            Stroke = Brushes.Blue;
            StrokeThickness = 2;
            topLeft = new Point(((PathDomino)dominoDefinition).xCoordinates[0], ((PathDomino)dominoDefinition).yCoordinates[0]);
            topRight = new Point(((PathDomino)dominoDefinition).xCoordinates[1], ((PathDomino)dominoDefinition).yCoordinates[1]);
            bottomRight = new Point(((PathDomino)dominoDefinition).xCoordinates[2], ((PathDomino)dominoDefinition).yCoordinates[2]);
            bottomLeft = new Point(((PathDomino)dominoDefinition).xCoordinates[3], ((PathDomino)dominoDefinition).yCoordinates[3]);
        }

        Point topLeft;
        Point topRight;
        Point bottomRight;
        Point bottomLeft;

        private void DrawGeometry(StreamGeometryContext context)
        {
            /*context.BeginFigure(new Point(0, 0), true, true); //Top Left
            IList<Point> points = new List<Point>();
            points.Add(new Point(Width, 0)); //Top Right
            points.Add(new Point(Width, Height)); // Bottom Right
            points.Add(new Point(0, Height)); // Bottom Left
            context.PolyLineTo(points, true, true);*/
            context.BeginFigure(topLeft, true, true); //Top Left
            IList<Point> points = new List<Point>();
            points.Add(topRight); //Top Right
            points.Add(bottomRight); // Bottom Right
            points.Add(bottomLeft); // Bottom Left
            context.PolyLineTo(points, true, true);
        }
    }
}
