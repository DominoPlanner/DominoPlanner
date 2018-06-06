using DominoPlanner.Document_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DominoPlanner
{
    public class DominoCanvas : Canvas
    {
        public ProjectDocument ProjectDoc;
        //public StructureDocument strucDocument = new StructureDocument();
        public System.Windows.Point SelectionStartPoint;
        public System.Windows.Shapes.Rectangle rect;
        public double projectWidth = 0;
        public double projectHeight = 0;

        public DominoCanvas()
        {
            this.MouseDown += Canvas_MouseDown;
            this.MouseMove += Canvas_MouseMove;
            this.Unloaded += DominoCanvas_Unloaded;
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Top;
        }

        private void DominoCanvas_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ProjectDoc = null;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) return;
            if (((DominoCanvas)sender).ProjectDoc.GetType() == typeof(FieldDocument))
            {
                int stoneHeight = 0, stoneWidth = 0, spaceWidth = 0, spaceHeight = 0;
                if (((FieldDocument)((DominoCanvas)sender).ProjectDoc).ShowSpaces)
                {
                    stoneWidth = ((FieldDocument)((DominoCanvas)sender).ProjectDoc).a;
                    stoneHeight = ((FieldDocument)((DominoCanvas)sender).ProjectDoc).c;
                    spaceWidth = ((FieldDocument)((DominoCanvas)sender).ProjectDoc).b;
                    spaceHeight = ((FieldDocument)((DominoCanvas)sender).ProjectDoc).d;
                }
                else
                {
                    stoneWidth = ((FieldDocument)((DominoCanvas)sender).ProjectDoc).a + ((FieldDocument)((DominoCanvas)sender).ProjectDoc).b / 2;
                    stoneHeight = ((FieldDocument)((DominoCanvas)sender).ProjectDoc).c;
                    spaceWidth = 0;
                    spaceHeight = 0;
                }
            ((DominoCanvas)sender).SelectionStartPoint = e.GetPosition((DominoCanvas)sender);

                ((DominoCanvas)sender).rect = new System.Windows.Shapes.Rectangle
                {
                    Stroke = System.Windows.Media.Brushes.LightBlue,
                    StrokeThickness = 8
                };
                Canvas.SetLeft(((DominoCanvas)sender).rect, ((DominoCanvas)sender).SelectionStartPoint.X);
                Canvas.SetTop(((DominoCanvas)sender).rect, ((DominoCanvas)sender).SelectionStartPoint.Y);
                ((DominoCanvas)sender).rect.Visibility = System.Windows.Visibility.Hidden;
                ((Canvas)sender).Children.Add(((DominoCanvas)sender).rect);
            }
            else if (((DominoCanvas)sender).ProjectDoc.GetType() == typeof(StructureDocument))
            {
                ((DominoCanvas)sender).SelectionStartPoint = e.GetPosition((DominoCanvas)sender);

                ((DominoCanvas)sender).rect = new System.Windows.Shapes.Rectangle
                {
                    Stroke = System.Windows.Media.Brushes.LightBlue,
                    StrokeThickness = 8
                };
                Canvas.SetLeft(((DominoCanvas)sender).rect, ((DominoCanvas)sender).SelectionStartPoint.X);
                Canvas.SetTop(((DominoCanvas)sender).rect, ((DominoCanvas)sender).SelectionStartPoint.Y);
                ((DominoCanvas)sender).rect.Visibility = System.Windows.Visibility.Hidden;
                ((Canvas)sender).Children.Add(((DominoCanvas)sender).rect);
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released || ((DominoCanvas)sender).rect == null)
                return;

            var pos = e.GetPosition((Canvas)sender);

            var x = Math.Min(pos.X, ((DominoCanvas)sender).SelectionStartPoint.X);
            var y = Math.Min(pos.Y, ((DominoCanvas)sender).SelectionStartPoint.Y);

            var w = Math.Max(pos.X, ((DominoCanvas)sender).SelectionStartPoint.X) - x;
            var h = Math.Max(pos.Y, ((DominoCanvas)sender).SelectionStartPoint.Y) - y;

            ((DominoCanvas)sender).rect.Width = w;
            ((DominoCanvas)sender).rect.Height = h;

            if (w > 6 || h > 6)
                ((DominoCanvas)sender).rect.Visibility = System.Windows.Visibility.Visible;

            Canvas.SetLeft(((DominoCanvas)sender).rect, x);
            Canvas.SetTop(((DominoCanvas)sender).rect, y);
        }
    }
}
