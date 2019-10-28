using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Collections.ObjectModel;

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
        public SelectionToolVM(EditProjectVM parent)
        {
            Image = "rect_selectDrawingImage";
            Name = "Select";
            SelectionTools = new ObservableCollection<SelectionDomain>() {
                new RectangleSelection(), new CircleSelectionDomain(),
                new PolygonSelectionDomain(), new FreehandSelectionDomain() };
            CurrentSelectionDomain = SelectionTools[0];
            this.parent = parent;
        }

        private SelectionDomain currentSelectionDomain;

        public SelectionDomain CurrentSelectionDomain
        {
            get { return currentSelectionDomain; }
            set {
                if (value != null)
                {
                    if (currentSelectionDomain != null)
                    {
                        value.IncludeBoundary = currentSelectionDomain.IncludeBoundary;
                        value.SelectionMode = currentSelectionDomain.SelectionMode;
                    }
                    currentSelectionDomain = value; 
                }
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<SelectionDomain> selectionTools;

        public ObservableCollection<SelectionDomain> SelectionTools
        {
            get { return selectionTools; }
            set { selectionTools = value; RaisePropertyChanged();  }
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentSelectionDomain?.MouseDown(sender, e, parent.DominoProject);   
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            CurrentSelectionDomain?.MouseMove(sender, e, parent.DominoProject);
        }
        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            var result = CurrentSelectionDomain?.MouseUp(sender, e, parent.DominoProject);
            if (CurrentSelectionDomain?.CurrentSelectionMode == SelectionMode.Add)
            {
                result.ForEach(x => parent.AddToSelectedDominoes(parent.DominoProject.Stones[x]));
            }
            else if (CurrentSelectionDomain?.CurrentSelectionMode == SelectionMode.Remove)
            {
                result.ForEach(x => parent.RemoveFromSelectedDominoes(parent.DominoProject.Stones[x]));
            }
            parent.UpdateUIElements();
        }
    }
    public abstract class SelectionDomain : ModelBase
    {
        public string Name { get; internal set; }
        public DrawingImage img { get; private set; }
        private string image;

        public string Image
        {
            get { return image; }
            set { image = value; img = (DrawingImage)System.Windows.Application.Current.Resources[value]; }
        }
       

        private bool includeBoundary = true;

        public bool IncludeBoundary
        {
            get { return includeBoundary; }
            set { includeBoundary = value; RaisePropertyChanged(); }
        }
        private SelectionMode selectionMode;

        public SelectionMode SelectionMode
        {
            get { return selectionMode; }
            set
            {
                selectionMode = value;
                RaisePropertyChanged();
            }
        }

        public SolidColorBrush SelectionColor
        {
            get
            {
                if (CurrentSelectionMode == SelectionMode.Add)
                {
                    return AddColor;
                }
                else return RemoveColor;
            }
        }

        SolidColorBrush AddColor = Brushes.LightBlue;
        SolidColorBrush RemoveColor = Brushes.IndianRed;

        protected bool ResetFlag = false;

        public SelectionMode CurrentSelectionMode;
        public System.Windows.Shapes.Shape s;
        
        public void RemoveSelectionDomain(ProjectCanvas pc)
        {
            s.Visibility = Visibility.Hidden;
            (VisualTreeHelper.GetParent(s) as Canvas)?.Children.Remove(s);
        }
        public void AddSelectionDomain(ProjectCanvas pc)
        {
            pc.Children.Remove(s);
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

        public abstract bool IsInside(DominoInCanvas dic, Rect boundingBox, bool includeBoundary);

        public void ResetSelectionArea()
        {
            ResetFlag = true;
        }
    }
    public abstract class TwoClickSelection : SelectionDomain
    {
        protected Point MouseDownPoint;
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
        public abstract Rect GetCurrentDimensions(Point pos);

        public override void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            UpdateSelectionMode(e);

            MouseDownPoint = e.GetPosition((Canvas)sender);

            SolidColorBrush color = SelectionColor;

            s.Stroke = color;
            s.StrokeThickness = 8;

            Canvas.SetLeft(s, MouseDownPoint.X);
            Canvas.SetTop(s, MouseDownPoint.Y);
            s.Width = 0;
            s.Height = 0;
            ResetFlag = false;
            AddSelectionDomain(pc);
        }
        public override void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc)
        {
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                RemoveSelectionDomain(pc);
                return;
            }
            if (s == null) return;

            var dims = GetCurrentDimensions(e.GetPosition((Canvas)sender));
            s.Width = dims.Width;
            s.Height = dims.Height;

            if (dims.Width > 10 || dims.Height > 10)
                s.Visibility = Visibility.Visible;
            else
                s.Visibility = Visibility.Hidden;

            Canvas.SetLeft(s, dims.X);
            Canvas.SetTop(s, dims.Y);
        }
        public override List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            List<int> result = new List<int>();
            if (!(e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released))
            {
                return result;
            }
            Rect boundingBox = GetBoundingBox();
            bool SingleClickFlag = false;
            var pos = e.GetPosition(pc);
            if ((pos.X - MouseDownPoint.X) * (pos.X - MouseDownPoint.X) + (pos.Y - MouseDownPoint.Y) * (pos.Y - MouseDownPoint.Y) < 5)
            {
                // single click 
                boundingBox = new Rect(e.GetPosition(pc).X, e.GetPosition(pc).Y, 0, 0);
                SingleClickFlag = true;
            }
            if (!ResetFlag)
            {
                for (int i = 0; i < pc.Stones.Count; i++)
                {
                    if (pc.Stones[i] is DominoInCanvas dic && IsInside(dic, boundingBox, SingleClickFlag ? true : IncludeBoundary))
                    {
                        result.Add(i);
                    }
                }
            }
            ResetFlag = false;
            RemoveSelectionDomain(pc);
            return result;
        }

        public override Rect GetBoundingBox()
        {
            double left = Canvas.GetLeft(s);
            double top = Canvas.GetTop(s);
            return new Rect(left, top, s.Width, s.Height);
        }
    }
    public class RectangleSelection : TwoClickSelection
    {
        public RectangleSelection()
        {
            s = new System.Windows.Shapes.Rectangle();
            Image = "rect_selectDrawingImage";
            Name = "Rectangle";
        }
        public override Rect GetCurrentDimensions(Point pos)
        {
            var x = Math.Min(pos.X, MouseDownPoint.X);
            var y = Math.Min(pos.Y, MouseDownPoint.Y);

            var w = Math.Max(pos.X, MouseDownPoint.X) - x;
            var h = Math.Max(pos.Y, MouseDownPoint.Y) - y;
            return new Rect(x, y, w, h);

        }
        public override bool IsInside(DominoInCanvas dic, Rect boundingBox, bool includeBoundary)
        {
            return IsInsideBoundingBox(boundingBox, dic, includeBoundary);
        }
    }
    public class CircleSelectionDomain : TwoClickSelection
    {
        public CircleSelectionDomain()
        {
            s = new System.Windows.Shapes.Ellipse();
            Image = "round_selectDrawingImage";
            Name = "Circle";
        }
        public override Rect GetCurrentDimensions(Point pos)
        {
            var radius = Math.Sqrt(Math.Pow(pos.X - MouseDownPoint.X, 2) + Math.Pow(pos.Y - MouseDownPoint.Y, 2));
            return new Rect(MouseDownPoint.X - radius, MouseDownPoint.Y - radius, 2 * radius, 2*radius);
        }

        public override bool IsInside(DominoInCanvas dic, Rect boundingBox, bool includeBoundary)
        {
            var radius = boundingBox.Width / 2;
            var center = new Point(boundingBox.X + radius, boundingBox.Y + radius);

            if (IsInsideBoundingBox(boundingBox, dic, includeBoundary))
            {
                var insideCircle = dic.canvasPoints.Count(x =>
                Math.Sqrt((x.X - center.X) * (x.X - center.X) + (x.Y - center.Y) * (x.Y - center.Y)) < radius);
                if (IncludeBoundary)
                {
                    if (insideCircle > 0)
                        return true;
                }
                else
                {
                    if (insideCircle == dic.canvasPoints.Length)
                        return true;
                }
            }
            return false;
        }
        
    }
    public class PolygonSelectionDomain : SelectionDomain
    {
        public PolygonSelectionDomain()
        {
            Image = "poly_selectDrawingImage";
            Name = "Polygon";
        }
        public override Rect GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public override bool IsInside(DominoInCanvas dic, Rect boundingBox, bool includeBoundary)
        {
            throw new NotImplementedException();
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            throw new NotImplementedException();
        }

        public override void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc)
        {
        }

        public override List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            throw new NotImplementedException();
        }
    }
    public class FreehandSelectionDomain : SelectionDomain
    {
        public FreehandSelectionDomain()
        {
            Image = "freehand_selectDrawingImage";
            Name = "Freehand";
        }
        public override Rect GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public override bool IsInside(DominoInCanvas dic, Rect boundingBox, bool includeBoundary)
        {
            throw new NotImplementedException();
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            throw new NotImplementedException();
        }

        public override void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc)
        {
        }

        public override List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            throw new NotImplementedException();
        }
    }
    public class DisplaySettingsToolVM : EditingToolVM
    {
        public DisplaySettingsToolVM(EditProjectVM parent)
        {
            Image = "display_settingsDrawingImage";
            Name = "View Properties";
            this.parent = parent;
        }
    }
}
