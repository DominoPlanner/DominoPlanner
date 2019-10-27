using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;

namespace DominoPlanner.Usage.UserControls.ViewModel
{

    public class EditingToolVM : ModelBase
    {
        public EditProjectVM parent;
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
            set { includeBoundary = value; RaisePropertyChanged(); SelectionDomain.IncludeBoundary = value; }
        }

        private SelectionDomain selectionDomain;

        public SelectionDomain SelectionDomain
        {
            get { return selectionDomain; }
            set { selectionDomain = value; RaisePropertyChanged();
                selectionDomain.SelectionMode = SelectionMode;
                selectionDomain.IncludeBoundary = includeBoundary;
            }
        }
        private SelectionMode selectionMode;

        public SelectionMode SelectionMode
        {
            get { return selectionMode; }
            set
            {
                selectionMode = value; RaisePropertyChanged();
                selectionDomain.SelectionMode = value;
                selectionDomain.IncludeBoundary = includeBoundary;
            }
        }

        public SelectionToolVM(EditProjectVM parent)
        {
            Image = "rect_selectDrawingImage";
            Name = "Select";
            SelectionDomain = new RectangleSelection();
            this.parent = parent;
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            SelectionDomain.MouseDown(sender, e, parent.DominoProject);   
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            SelectionDomain.MouseMove(sender, e, parent.DominoProject);
        }
        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            var result = SelectionDomain.MouseUp(sender, e, parent.DominoProject);
            if (SelectionDomain.CurrentSelectionMode == SelectionMode.Add)
            {
                result.ForEach(x => parent.AddToSelectedDominoes(parent.DominoProject.Stones[x]));
            }
            else if (SelectionDomain.CurrentSelectionMode == SelectionMode.Remove)
            {
                result.ForEach(x => parent.RemoveFromSelectedDominoes(parent.DominoProject.Stones[x]));
            }
            parent.UpdateUIElements();
        }
    }
    public abstract class SelectionDomain
    {
        SolidColorBrush AddColor = Brushes.LightBlue;
        SolidColorBrush RemoveColor = Brushes.IndianRed;


        public SolidColorBrush SelectionColor()
        {
            if (CurrentSelectionMode == SelectionMode.Add)
            {
                return AddColor;
            }
            else return RemoveColor;
        }
        public bool IncludeBoundary;
        public SelectionMode CurrentSelectionMode;
        public SelectionMode SelectionMode;
        public System.Windows.Shapes.Shape s;
        
        public void RemoveSelectionDomain(ProjectCanvas pc)
        {
            s.Visibility = Visibility.Hidden;
            pc.Children.Remove(s);
        }
        public void AddSelectionDomain(ProjectCanvas pc)
        {
            if (s.Visibility == Visibility.Visible)
            {
                RemoveSelectionDomain(pc);
            }
            s.Visibility = Visibility.Visible;
            pc.Children.Add(s);
        }
        public abstract void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc);

        public abstract void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc);

        public abstract List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc);

        public abstract Rect GetBoundingBox();

        public bool IsInsideBoundingBox(Rect BoundingBox, DominoInCanvas dic, bool includeBoundary)
        {
            if (includeBoundary)
            {
                return dic.canvasPoints.Max(x => x.X) > BoundingBox.Left &&
                    dic.canvasPoints.Min(x => x.X) < BoundingBox.Right &&
                    dic.canvasPoints.Min(x => x.Y) < BoundingBox.Bottom &&
                    dic.canvasPoints.Max(x => x.Y) > BoundingBox.Top;
            }
            else
            {
                return dic.canvasPoints.Min(x => x.X) > BoundingBox.Left &&
                    dic.canvasPoints.Max(x => x.X) < BoundingBox.Right &&
                    dic.canvasPoints.Max(x => x.Y) < BoundingBox.Bottom &&
                    dic.canvasPoints.Min(x => x.Y) > BoundingBox.Top;
                
            }
        }

        public abstract bool? IsInside();

        public abstract void UpdateShape(Point position);
    }
    public class RectangleSelection : SelectionDomain
    {
        public RectangleSelection()
        {
            s = new System.Windows.Shapes.Rectangle();
        }
        Point MouseDownPoint;

        public void UpdateSelectionMode(MouseButtonEventArgs e) // called on Mouse down
        {
            if ((e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right))
            {
                if (SelectionMode == SelectionMode.Add || (SelectionMode == SelectionMode.Neutral && e.ChangedButton == MouseButton.Left))
                {
                    CurrentSelectionMode = SelectionMode.Add;
                }
                else if (SelectionMode == SelectionMode.Remove || (SelectionMode == SelectionMode.Neutral && e.ChangedButton == MouseButton.Right))
                {
                    CurrentSelectionMode = SelectionMode.Remove;
                }
                else
                {
                    CurrentSelectionMode = SelectionMode.Neutral;
                }
            }
            
        }

        public override void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc)
        {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                RemoveSelectionDomain(pc);
                return;
            }
            var pos = e.GetPosition((Canvas)sender);

            System.Windows.Shapes.Rectangle rect = s as System.Windows.Shapes.Rectangle;
            if (rect == null) return;
            

            var x = Math.Min(pos.X, MouseDownPoint.X);
            var y = Math.Min(pos.Y, MouseDownPoint.Y);

            var w = Math.Max(pos.X, MouseDownPoint.X) - x;
            var h = Math.Max(pos.Y, MouseDownPoint.Y) - y;

            rect.Width = w;
            rect.Height = h;

            if (w > 10 || h > 10)
                rect.Visibility = Visibility.Visible;
            else
                rect.Visibility = Visibility.Hidden;

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
        }
        public override Rect GetBoundingBox()
        {
            System.Windows.Shapes.Rectangle rect = s as System.Windows.Shapes.Rectangle;
            double left = Canvas.GetLeft(rect);
            double top = Canvas.GetTop(rect);
            return new Rect(left, top, rect.Width, rect.Height);
        }
        public override List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            List<int> result = new List<int>();
            if (!(e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released))
            {
                return result;
            }
            Rect boundingBox = GetBoundingBox();
            bool singleClickFlag = false;
            if (s.Visibility == Visibility.Hidden || s == null)
            {
                // single click 
                boundingBox = new Rect(e.GetPosition(pc).X, e.GetPosition(pc).Y, 0, 0);
                singleClickFlag = true;
            }
            for (int i = 0; i < pc.Stones.Count; i++)
            {
                if (pc.Stones[i] is DominoInCanvas dic && IsInsideBoundingBox(boundingBox, dic, singleClickFlag ? false: IncludeBoundary))
                {
                    result.Add(i);
                }
            }
            RemoveSelectionDomain(pc);
            return result;

        }
        public override void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            UpdateSelectionMode(e);

            MouseDownPoint = e.GetPosition((Canvas) sender);

            SolidColorBrush color = SelectionColor();

            s = new System.Windows.Shapes.Rectangle
            {
                Stroke = color,
                StrokeThickness = 8
            };
         
            Canvas.SetLeft(s, MouseDownPoint.X);
            Canvas.SetTop(s, MouseDownPoint.Y);
            AddSelectionDomain(pc);
        }

        public override bool? IsInside()
        {
            throw new NotImplementedException();
        }

        public override void UpdateShape(Point position)
        {
            throw new NotImplementedException();
        }
    }
}
