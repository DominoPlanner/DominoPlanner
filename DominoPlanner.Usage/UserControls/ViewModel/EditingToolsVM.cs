using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace DominoPlanner.Usage.UserControls.ViewModel
{

    public class EditingToolVM : ModelBase
    {
        public string Name { get; internal set; }
        private string image;

        public string Image
        {
            get { return image; }
            set { image = value; img = (DrawingImage)System.Windows.Application.Current.Resources[value]; }
        }

        public DrawingImage img { get; private set; }

        public virtual void MouseMove(object sender, MouseEventArgs e) { }

        public virtual void MouseDown(object sender, MouseButtonEventArgs e) { }

        public virtual void MouseUp(object sender, MouseButtonEventArgs e) { }
    }
    public enum SelectionMode
    {
        Add,
        Neutral,
        Remove
    }
    public class SelectionToolVM : EditingToolVM
    {
        private bool includeBoundary;

        public bool IncludeBoundary
        {
            get { return includeBoundary; }
            set { includeBoundary = value; RaisePropertyChanged(); }
        }

        private SelectionDomain selectionDomain;

        public SelectionDomain SelectionDomain
        {
            get { return selectionDomain; }
            set { selectionDomain = value; RaisePropertyChanged(); }
        }
        private SelectionMode selectionMode;

        public SelectionMode SelectionMode
        {
            get { return selectionMode; }
            set { selectionMode = value; RaisePropertyChanged(); }
        }

        public SelectionToolVM()
        {
            Image = "rect_selectDrawingImage";
            Name = "Select";
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
    public abstract class SelectionDomain
    {
        public abstract void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc);

        public abstract void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc);

        public abstract List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc);
    }
    public class RectangleSelection : SelectionDomain
    {
        Point SelectionStartPoint;
        System.Windows.Shapes.Rectangle rect;

        public override void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc)
        {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                if (rect != null)
                {
                    pc.Children.Remove(rect);
                    rect = null;
                }
                return;
            }

            if (rect == null) return;

            var pos = e.GetPosition((Canvas)sender);

            var x = Math.Min(pos.X, SelectionStartPoint.X);
            var y = Math.Min(pos.Y, SelectionStartPoint.Y);

            var w = Math.Max(pos.X, SelectionStartPoint.X) - x;
            var h = Math.Max(pos.Y, SelectionStartPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;

            if (w > 10 || h > 10)
                rect.Visibility = Visibility.Visible;
            else
                rect.Visibility = Visibility.Hidden;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }

        public override List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            if (rect == null || rect.Visibility != Visibility.Visible)
            {
                for (int i = 0; i < pc.Stones.Count; i++)
                {
                    if (DominoProject.Stones[i] is DominoInCanvas dic)
                    {
                        double _top = double.MaxValue;
                        double _bottom = 0;
                        double _left = double.MaxValue;
                        double _right = 0;

                        foreach (System.Windows.Point point in dic.canvasPoints)
                        {
                            if (point.Y < _top) _top = point.Y;
                            if (point.Y > _bottom) _bottom = point.Y;
                            if (point.X < _left) _left = point.X;
                            if (point.X > _right) _right = point.X;
                        }
                        if (_left < e.GetPosition(pc).X && _right > e.GetPosition(pc).X
                            && _top < e.GetPosition(pc).Y && _bottom > e.GetPosition(pc).Y)
                        {
                            if (e.ChangedButton == MouseButton.Left)
                            {
                                if (!((DominoInCanvas)pc.Stones[i]).isSelected)
                                {
                                    AddToSelectedDominoes(pc.Stones[i]);
                                }
                            }
                            else if (e.ChangedButton == MouseButton.Right)
                            {
                                if (((DominoInCanvas)DominoProject.Stones[i]).isSelected)
                                {
                                    RemoveFromSelectedDominoes(pc.Stones[i]);
                                }
                            }
                        }
                    }
                }

                UpdateUIElements();
                return;
            }
            double top = Canvas.GetTop(rect);
            double right = Canvas.GetLeft(rect) + rect.ActualWidth;
            double bottom = Canvas.GetTop(rect) + rect.ActualHeight;
            double left = Canvas.GetLeft(rect);

            for (int i = 0; i < pc.Stones.Count; i++)
            {
                if (pc.Stones[i] is DominoInCanvas dic)
                {
                    double _top = double.MaxValue;
                    double _bottom = 0;
                    double _left = double.MaxValue;
                    double _right = 0;

                    foreach (System.Windows.Point point in dic.canvasPoints)
                    {
                        if (point.Y < _top) _top = point.Y;
                        if (point.Y > _bottom) _bottom = point.Y;
                        if (point.X < _left) _left = point.X;
                        if (point.X > _right) _right = point.X;
                    }

                    /*if ((dic.RenderedGeometry.Bounds.Left > left && dic.RenderedGeometry.Bounds.Left < right
                          || dic.RenderedGeometry.Bounds.Right > left && dic.RenderedGeometry.Bounds.Right < right
                          || dic.RenderedGeometry.Bounds.Left < left && dic.RenderedGeometry.Bounds.Right > left 
                          && dic.RenderedGeometry.Bounds.Left < right && dic.RenderedGeometry.Bounds.Right > right)
                          && (dic.RenderedGeometry.Bounds.Top > top && dic.RenderedGeometry.Bounds.Top < bottom
                          || dic.RenderedGeometry.Bounds.Bottom > top && dic.RenderedGeometry.Bounds.Bottom < bottom
                          || (dic.RenderedGeometry.Bounds.Top < top && dic.RenderedGeometry.Bounds.Bottom > top
                          && dic.RenderedGeometry.Bounds.Top < bottom && dic.RenderedGeometry.Bounds.Bottom > bottom)))*/
                    if ((_left > left && _left < right
                      || _right > left && _right < right
                      || _left < left && _right > left
                      && _left < right && _right > right)
                      && (_top > top && _top < bottom
                      || _bottom > top && _bottom < bottom
                      || (_top < top && _bottom > top
                      && _top < bottom && _bottom > bottom)))
                    {
                        if (e.ChangedButton == MouseButton.Left)
                        {
                            if (!((DominoInCanvas)pc.Stones[i]).isSelected)
                            {
                                AddToSelectedDominoes(pc.Stones[i]);
                            }
                        }
                        else if (e.ChangedButton == MouseButton.Right)
                        {
                            if (((DominoInCanvas)pc.Stones[i]).isSelected)
                            {
                                RemoveFromSelectedDominoes(pc.Stones[i]);
                            }
                        }
                    }
                }
            }

            rect.Visibility = Visibility.Hidden;
            pc.Children.Remove(rect);

            UpdateUIElements();
        }
        public override void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            if (e.MiddleButton == MouseButtonState.Pressed) return;

            SelectionStartPoint = e.GetPosition(pc);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                rect = new System.Windows.Shapes.Rectangle
                {
                    Stroke = Brushes.LightBlue,
                    StrokeThickness = 8
                };
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                rect = new System.Windows.Shapes.Rectangle
                {
                    Stroke = Brushes.IndianRed,
                    StrokeThickness = 8
                };
            }
            Canvas.SetLeft(rect, SelectionStartPoint.X);
            Canvas.SetTop(rect, SelectionStartPoint.Y);
            rect.Visibility = System.Windows.Visibility.Hidden;
            pc.Children.Add(rect);
        }
    }
}
