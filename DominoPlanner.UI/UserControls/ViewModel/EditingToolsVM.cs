using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Avalonia;
using System.Linq;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using Emgu.CV;
using DominoPlanner.Core;
using Avalonia.Media;
using Avalonia.Input;
using Avalonia.Controls.Shapes;
using Avalonia.Controls;
using Avalonia.Layout;
using MsgBox;
using System.Threading.Tasks;

namespace DominoPlanner.UI.UserControls.ViewModel
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

        public virtual void KeyPressed(Key key) { }
    }
    public enum SelectionMode
    {
        Add,
        Neutral,
        Remove
    }
    public class SelectionOperation : PostFilter
    {
        SelectionToolVM reference;
        IList<int> ToSelect;
        bool positiveselect;
        bool[] oldState;
        public SelectionOperation(SelectionToolVM reference, IList<int> ToSelect, bool select )
        {
            this.reference = reference;
            this.ToSelect = ToSelect.ToArray();
            this.positiveselect = select;
            oldState = new bool[ToSelect.Count];
        }
        public override void Apply()
        {
            for (int i = 0; i < ToSelect.Count; i++)
            {
                oldState[i] = reference.parent.IsSelected(ToSelect[i]);
                if (positiveselect)
                    reference.parent.AddToSelectedDominoes(ToSelect[i]);
                else
                    reference.parent.RemoveFromSelectedDominoes(ToSelect[i]);
            }
        }

        public override void Undo()
        {
            for (int i = 0; i < ToSelect.Count; i++)
            {
                if (oldState[i])
                {
                    reference.parent.AddToSelectedDominoes(ToSelect[i]);
                }
                else
                {
                    reference.parent.RemoveFromSelectedDominoes(ToSelect[i]);
                }
            }
        }
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
            UndoSelectionOperation = new RelayCommand((o) => {
                parent.UndoInternal(true);
            });
            RedoSelectionOperation = new RelayCommand((o) => {
                parent.RedoInternal(true);
            });
            InvertSelection = new RelayCommand((o) => InvertSelectionOperation());
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
            CurrentSelectionDomain?.MouseDown(sender, e, sender as ProjectCanvas);   
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            CurrentSelectionDomain?.MouseMove(sender, e, sender as ProjectCanvas);
        }
        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            var result = CurrentSelectionDomain?.MouseUp(sender, e, sender as ProjectCanvas);
            if (result.Count == 0) return;
            if (CurrentSelectionDomain?.CurrentSelectionMode == SelectionMode.Add)
            {
                Select(result, true);
            }
            else if (CurrentSelectionDomain?.CurrentSelectionMode == SelectionMode.Remove)
            {
                Select(result, false);
            }
            parent.UpdateUIElements();
        }
        public override void KeyPressed(Key key)
        {
            if (key == Key.Escape)
            {
                parent.ClearFullSelection(true);
            }
        }
        public void Select(IList<int> toSelect, bool select)
        {
            parent.ExecuteOperation(new SelectionOperation(this, toSelect, select));
        }
        public void InvertSelectionOperation()
        {
            var current = parent.selectedDominoes.ToList();
            var n = Enumerable.Range(0, parent.dominoTransfer.length).Except(current).ToList();
            Select(current, false);
            Select(n, true);
            parent.UpdateUIElements();
        }
        private ICommand _UndoSelectionOperation;
        public ICommand UndoSelectionOperation { get { return _UndoSelectionOperation; } set { if (value != _UndoSelectionOperation) { _UndoSelectionOperation= value; } } }

        private ICommand _RedoSelectionOperation;
        public ICommand RedoSelectionOperation { get { return _RedoSelectionOperation; } set { if (value != _RedoSelectionOperation) { _RedoSelectionOperation = value; } } }

        private ICommand _InvertSelection;
        public ICommand InvertSelection { get { return _InvertSelection; } set { if (value != _InvertSelection) { _InvertSelection = value; } } }

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
        private SelectionMode selectionMode = SelectionMode.Neutral;

        public SelectionMode SelectionMode
        {
            get { return selectionMode; }
            set
            {
                selectionMode = value;
                RaisePropertyChanged();
            }
        }

        public ISolidColorBrush SelectionColor
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

        ISolidColorBrush AddColor = Brushes.LightBlue;
        ISolidColorBrush RemoveColor = Brushes.IndianRed;

        protected bool ResetFlag = false;

        public SelectionMode CurrentSelectionMode;
        public Shape s;
        
        public void RemoveSelectionDomain(ProjectCanvas pc)
        {
            s.IsVisible = false;
            (VisualTreeHelper.GetParent(s) as Canvas)?.Children.Remove(s);
        }
        public void AddSelectionDomain(ProjectCanvas pc)
        {
            pc.Children.Remove(s);
            s.IsVisible = true;
            pc.Children.Add(s);
        }
        public abstract void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc);

        public abstract void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc);

        public virtual List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            return new List<int>();
        }
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
        public static bool IsPointInPolygon(IList<Avalonia.Point> polygon, Avalonia.Point testPoint)
        {
            bool result = false;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
        public bool CheckBoundary(DominoInCanvas dic, int pointsInsideSelection)
        {
            if (IncludeBoundary)
            {
                if (pointsInsideSelection > 0)
                    return true;
            }
            else
            {
                if (pointsInsideSelection == dic.canvasPoints.Length)
                    return true;
            }
            return false;
        }
    }
    public abstract class TwoClickSelection : SelectionDomain
    {
        public TwoClickSelection()
        {
            MouseDownPoint = new Avalonia.Point(-1, -1);
        }
        protected Avalonia.Point MouseDownPoint;
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
        public abstract void UpdateShapeProperties(Avalonia.Point pos);
        public abstract void Initialize();

        public override void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            UpdateSelectionMode(e);

            MouseDownPoint = e.GetPosition((Canvas)sender);

            s.Stroke = SelectionColor;
            s.StrokeThickness = 8;

            Initialize();
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
            
            UpdateShapeProperties(e.GetPosition((Canvas)sender));

            
        }
        public override List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            
            List<int> result = new List<int>();
            if (!(e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released) 
                || (MouseDownPoint.X == -1 && MouseDownPoint.Y == -1))
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
            MouseDownPoint = new Avalonia.Point(-1, -1);
            return result;
        }
        
        
    }
    public abstract class WidthHeightSelection: TwoClickSelection
    {
        public override void Initialize()
        {
            Canvas.SetLeft(s, MouseDownPoint.X);
            Canvas.SetTop(s, MouseDownPoint.Y);
            s.Width = 0;
            s.Height = 0;
        }
        public abstract Rect GetCurrentDimensions(Avalonia.Point pos);

        public override void UpdateShapeProperties(Avalonia.Point pos)
        {
            var dims = GetCurrentDimensions(pos);
            s.Width = dims.Width;
            s.Height = dims.Height;

            if (dims.Width > 10 || dims.Height > 10)
                s.IsVisible = true;
            else
                s.IsVisible = false;

            Canvas.SetLeft(s, dims.X);
            Canvas.SetTop(s, dims.Y);
        }
        public override Rect GetBoundingBox()
        {
            double left = Canvas.GetLeft(s);
            double top = Canvas.GetTop(s);
            return new Rect(left, top, s.Width, s.Height);
        }
    }
    public class RectangleSelection : WidthHeightSelection
    {
        public RectangleSelection()
        {
            s = new Rectangle();
            Image = "rect_selectDrawingImage";
            Name = "Rectangle";
        }
        public override Rect GetCurrentDimensions(Avalonia.Point pos)
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
    public class CircleSelectionDomain : WidthHeightSelection
    {
        public CircleSelectionDomain()
        {
            s = new Ellipse();
            Image = "round_selectDrawingImage";
            Name = "Circle";
        }
        public override Rect GetCurrentDimensions(Avalonia.Point pos)
        {
            var radius = Math.Sqrt(Math.Pow(pos.X - MouseDownPoint.X, 2) + Math.Pow(pos.Y - MouseDownPoint.Y, 2));
            return new Rect(MouseDownPoint.X - radius, MouseDownPoint.Y - radius, 2 * radius, 2*radius);
        }

        public override bool IsInside(DominoInCanvas dic, Rect boundingBox, bool includeBoundary)
        {
            var radius = boundingBox.Width / 2;
            var center = new Avalonia.Point(boundingBox.X + radius, boundingBox.Y + radius);

            if (IsInsideBoundingBox(boundingBox, dic, includeBoundary))
            {
                var insideCircle = dic.canvasPoints.Count(x =>
                Math.Sqrt((x.X - center.X) * (x.X - center.X) + (x.Y - center.Y) * (x.Y - center.Y)) < radius);
                return CheckBoundary(dic, insideCircle);
            }
            return false;
        }
        
    }
    public class PolygonSelectionDomain : SelectionDomain
    {
        public List<Avalonia.Point> points;
        public MouseButton? firstButton;
        public PolygonSelectionDomain()
        {
            Image = "poly_selectDrawingImage";
            Name = "Polygon";
            s = new Polyline();
            points = new List<Avalonia.Point>();
            var poly = s as Polyline;
            poly.Fill = new SolidColorBrush(Color.FromArgb(50, 100, 100, 100));
            Canvas.SetLeft(poly, 0);
            Canvas.SetTop(poly, 0);
            
        }

        public override Rect GetBoundingBox()
        {
            double left = points.Min(x => x.X);
            double top = points.Min(x => x.Y);
            double bottom = points.Max(x => x.Y);
            double right = points.Max(x => x.X);
            return new Rect(left, top, right - left, bottom - top);
        }

        public override bool IsInside(DominoInCanvas dic, Rect boundingBox, bool includeBoundary)
        {
            if (IsInsideBoundingBox(boundingBox, dic, includeBoundary))
            {
                var insidePoly = dic.canvasPoints.Count(x => IsPointInPolygon(points, new Avalonia.Point(x.X, x.Y)));
                return CheckBoundary(dic, insidePoly);
            }
            return false;
        }
        bool DoubleClickFlag;
        public override void MouseDown(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            
            if (!((e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right) &&
                (e.LeftButton == MouseButtonState.Pressed ^ e.RightButton == MouseButtonState.Pressed)))
                return;
            
            if (points.Count == 0)
            {
                firstButton = e.ChangedButton;
                if (SelectionMode == SelectionMode.Neutral)
                {
                    CurrentSelectionMode = e.ChangedButton == MouseButton.Left ? SelectionMode.Add : SelectionMode.Remove;
                }
                else
                {
                    CurrentSelectionMode = SelectionMode;
                }
                AddSelectionDomain(pc);
            }
            else if (e.ClickCount == 2)
            {
                DoubleClickFlag = true;
            }
            if (e.ChangedButton != firstButton)
            {
                // selection canceled, clear polygon
                points.Clear();
                RemoveSelectionDomain(pc);
                return;
            }
            s.Stroke = SelectionColor;
            s.StrokeThickness = 8;
            points.Add(e.GetPosition(pc));
            var poly = s as Polyline;
            poly.Points = points;
            
        }

        public override void MouseMove(object sender, MouseEventArgs e, ProjectCanvas pc)
        {
            if (s == null) return;

            var poly = s as Polyline;
            if (poly.Points.Count == points.Count)
                poly.Points.Add(e.GetPosition(pc));
            else
                poly.Points[poly.Points.Count - 1] = e.GetPosition(pc);
        }

        public override List<int> MouseUp(object sender, MouseButtonEventArgs e, ProjectCanvas pc)
        {
            var result = new List<int>();
            if (!DoubleClickFlag)
                return result;
            DoubleClickFlag = false;
            var boundingBox = GetBoundingBox();
            for (int i = 0; i < pc.Stones.Count; i++)
            {
                if (pc.Stones[i] is DominoInCanvas dic && IsInside(dic, boundingBox, IncludeBoundary))
                {
                    result.Add(i);
                }
            }
            RemoveSelectionDomain(pc);
            points = new List<Avalonia.Point>();
            firstButton = null;
            return result;
        }
    }
    public class FreehandSelectionDomain : TwoClickSelection
    {
        public FreehandSelectionDomain()
        {
            Image = "freehand_selectDrawingImage";
            Name = "Freehand";
            s = new Polyline();
            MouseDownPoint = new Avalonia.Point(-1, -1);
            var poly = s as Polyline;
            poly.Fill = new SolidColorBrush(Color.FromArgb(50, 100, 100, 100));
            Canvas.SetLeft(poly, 0);
            Canvas.SetTop(poly, 0);

        }
        public override void Initialize()
        {
            var poly = s as Polyline;
            poly.Points = new List<Avalonia.Point>();
            poly.Points.Add(MouseDownPoint);
        }

        public override bool IsInside(DominoInCanvas dic, Rect boundingBox, bool includeBoundary)
        {
            var poly = s as Polyline;
            var points = poly.Points.ToList();
            if (IsInsideBoundingBox(boundingBox, dic, includeBoundary))
            {
                var insidePoly = dic.canvasPoints.Count(x => IsPointInPolygon(points, new Avalonia.Point(x.X, x.Y)));
                return CheckBoundary(dic, insidePoly);
            }
            return false;
        }

        public override void UpdateShapeProperties(Avalonia.Point pos)
        {
            var poly = s as Polyline;
            var last = poly.Points.Last();
            Debug.WriteLine("Hit, Length: " + poly.Points.Count);
            if ((last.X - pos.X) * (last.X - pos.X) + (last.Y - pos.Y) * (last.Y - pos.Y) > 3)
            {
                
                poly.Points.Add(pos);
            }
        }
        public override Rect GetBoundingBox()
        {
            var poly = s as Polyline;
            var points = poly.Points;
            double left = points.Min(x => x.X);
            double top = points.Min(x => x.Y);
            double bottom = points.Max(x => x.Y);
            double right = points.Max(x => x.X);
            return new Rect(left, top, right - left, bottom - top);
        }
    }
    public class DisplaySettingsToolVM : EditingToolVM
    {
        private double visibleWidth = 0;
        private double visibleHeight = 0;
        private double largestX = 0;
        private double largestY = 0;
        private Image<Emgu.CV.Structure.Bgra, byte> FilteredMat;

        private ProjectCanvas _DominoProject;
        public ProjectCanvas DominoProject
        {
            get { return _DominoProject; }
            set
            {
                if (_DominoProject != value)
                {
                    if (_DominoProject != null)
                    {
                        _DominoProject.SizeChanged -= _DominoProject_SizeChanged;
                    }
                    _DominoProject = value;
                    RaisePropertyChanged();
                    _DominoProject.SizeChanged += _DominoProject_SizeChanged;

                    _DominoProject.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _DominoProject.VerticalAlignment = VerticalAlignment.Stretch;
                }
            }
        }
        public DisplaySettingsToolVM(EditProjectVM parent)
        {
            Image = "display_settingsDrawingImage";
            Name = "View Properties";
            this.parent = parent;
            PossiblePastePositions = new List<DominoInCanvas>();
            ShowImageClick = new RelayCommand(o => { ShowImage(); });

            if (parent.CurrentProject != null && parent.CurrentProject.PrimaryImageTreatment != null)
            {
                if (parent.CurrentProject.PrimaryImageTreatment.FilteredImage != null)
                {
                    FilteredImage = parent.CurrentProject.PrimaryImageTreatment.FilteredImage;
                    FilteredMat = parent.CurrentProject.PrimaryImageTreatment.imageFiltered.ToImage<Emgu.CV.Structure.Bgra, byte>();
                }
                else
                {
                    Core.BlendFileFilter bff = parent.CurrentProject.PrimaryImageTreatment.ImageFilters.OfType<Core.BlendFileFilter>().FirstOrDefault();
                    if (bff != null)
                    {
                        string relativePath = bff.FilePath;
                        string absolutePath = Core.Workspace.AbsolutePathFromReference(ref relativePath, parent.CurrentProject);
                        if (File.Exists(absolutePath))
                        {
                            FilteredImage = (System.Drawing.Bitmap)System.Drawing.Image.FromFile(absolutePath);
                            FilteredMat = new Image<Emgu.CV.Structure.Bgra, byte>(absolutePath);
                        }
                    }
                }
            }
            Expandable = parent.CurrentProject is FieldParameters;
        }
        private System.Drawing.Bitmap _FilteredImage;

        public System.Drawing.Bitmap FilteredImage
        {
            get { return _FilteredImage; }
            set
            {
                if (_FilteredImage != value)
                {
                    _FilteredImage = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _Expanded;
        public bool Expanded
        {
            get => _Expanded;
            set
            {
                if (_Expanded != value)
                {
                    _Expanded = value;
                    RaisePropertyChanged();
                    ResetCanvas();
                }
            }
        }
        private bool _Expandable;
        public bool Expandable
        {
            get { return _Expandable; }
            set
            {
                if (_Expandable != value)
                {
                    _Expandable = value;
                    RaisePropertyChanged();
                }
            }
        }
        private Color backgroundColor = Colors.Transparent;
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                DominoProject.Background = new SolidColorBrush(value);
                RaisePropertyChanged();
                Redraw();
            }
        }
        private Color borderColor = Color.FromArgb(50, 0, 0, 255);
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                DominoProject.UnselectedBorderColor = BorderColor;
                DominoProject.SelectedBorderColor = Colors.Blue;
                RaisePropertyChanged();
                Redraw();
            }
        }

        private double borderSize = 2;

        public double BorderSize
        {
            get { return borderSize; }
            set
            {
                borderSize = value;
                DominoProject.BorderSize = BorderSize;
                RaisePropertyChanged();
                Redraw();
            }
        }

        private double opacity = 0;
        public double ImageOpacity
        {
            get { return opacity; }
            set
            {
                opacity = value;
                DominoProject.OpacityValue = opacity;
                RaisePropertyChanged();
                Redraw();
            }
        }

        private bool above = false;
        public bool Above
        {
            get { return above; }
            set
            {
                above = value;
                DominoProject.above = above;
                RaisePropertyChanged();
                Redraw();
            }
        }
        private double _ZoomValue = 1;
        public double ZoomValue
        {
            get { return _ZoomValue; }
            set
            {
                if (_ZoomValue != value)
                {
                    if (value < 1) value = 1;
                    double scale = _DominoProject.LayoutTransform.Value.M11 / _ZoomValue * value;
                    _ZoomValue = value;
                    _DominoProject.LayoutTransform = new ScaleTransform(scale, scale);
                    RaisePropertyChanged();
                }
            }
        }
        private string PreviewPath
        {
            get
            {
                string imagepath = Application.LocalUserAppDataPath;
                imagepath += "\\" + parent.name + "_prev.png";
                return imagepath;
            }
        }
        private void _DominoProject_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshTransformation();
        }
        internal void RefreshTransformation()
        {
            if (_DominoProject == null)
                return;
            double ScaleX, ScaleY;

            ScaleX = visibleWidth / largestX * ZoomValue;
            ScaleY = visibleHeight / largestY * ZoomValue;

            if (ScaleX < ScaleY)
                _DominoProject.LayoutTransform = new ScaleTransform(ScaleX, ScaleX);
            else
                _DominoProject.LayoutTransform = new ScaleTransform(ScaleY, ScaleY);

            _DominoProject.UpdateLayout();
        }
        internal void ResetCanvas()
        {
            var selectedIndices = parent.selectedDominoes.ToList();
            parent.selectedDominoes.Clear();
            if (DominoProject != null)
            {
                RemoveStones();
                DominoProject.MouseDown -= parent.Canvas_MouseDown;
                DominoProject.MouseMove -= parent.Canvas_MouseMove;
                DominoProject.MouseUp -= parent.Canvas_MouseUp;
            }
            
            DominoProject = new ProjectCanvas();
            parent.dominoTransfer = parent.CurrentProject.Generate(new System.Threading.CancellationToken());
            largestX = parent.dominoTransfer.shapes.Max(x => x.GetContainer(expanded: Expanded).x2);
            largestY = parent.dominoTransfer.shapes.Max(x => x.GetContainer(expanded: Expanded).y2);
            DominoProject.Width = largestX;
            DominoProject.Height = largestY;
            DominoProject.MouseDown += parent.Canvas_MouseDown;
            DominoProject.MouseMove += parent.Canvas_MouseMove;
            DominoProject.MouseUp += parent.Canvas_MouseUp;
            DominoProject.Background = new SolidColorBrush(BackgroundColor);
            DominoProject.UnselectedBorderColor = BorderColor;
            DominoProject.SelectedBorderColor = Colors.Blue;
            DominoProject.BorderSize = BorderSize;
            DominoProject.OriginalImage = FilteredMat;
            DominoProject.OpacityValue = ImageOpacity;
            DominoProject.above = above;
            

            for (int i = 0; i < parent.dominoTransfer.shapes.Count(); i++)
            {
                DominoInCanvas dic = new DominoInCanvas(i, parent.dominoTransfer[i], parent.CurrentProject.colors, !Expanded);
                DominoProject.Stones.Add(dic);
            }
            
            selectedIndices.ForEach(x => parent.AddToSelectedDominoes(x));
            

            parent.UpdateUIElements();
        }
        internal void SizeChanged(double width, double height)
        {
            visibleWidth = width;
            visibleHeight = height;
            RefreshTransformation();
        }
        internal void RemoveStones()
        {
            while (DominoProject.Stones.Count > 0)
            {
                if (DominoProject.Stones[0] is DominoInCanvas dic)
                    dic.DisposeStone();
                DominoProject.Stones.RemoveAt(0);
            }
        }
        public void cleanEvents()
        {
            foreach (DominoInCanvas dic in DominoProject.Stones)
            {
                dic.DisposeStone();
            }
        }
        public bool SelectDominoVisual(int position)
        {
            var dic = DominoProject.Stones[position];
            if (dic.isSelected == false)
            {
                dic.isSelected = true;
                return true;
            }
            return false;
        }
        public bool DeSelectDominoVisual(int position)
        {
            var dic = DominoProject.Stones[position];
            if (dic.isSelected == true)
            {
                dic.isSelected = false;
                return true;
            }
            return false;
        }
        private List<DominoInCanvas> PossiblePastePositions;
        public void HighlightPastePositions(int[] validPositions)
        {
            PossiblePastePositions = new List<DominoInCanvas>();
            foreach (int i in validPositions)
            {
                var dic = DominoProject.Stones[i];
                dic.PossibleToPaste = true;
                PossiblePastePositions.Add(dic);
            }
            Redraw();
        }
        public void ClearPastePositions()
        {
            foreach (DominoInCanvas dic in PossiblePastePositions)
            {
                dic.PossibleToPaste = false;
            }
            PossiblePastePositions.Clear();
            Redraw();
        }
        public void Redraw()
        {
            DominoProject?.InvalidateVisual();
            bool discrepancy = false;
            if (DominoProject?.Stones == null) return;
            for (int i = 0; i < DominoProject.Stones.Count; i++)
            {
                if (DominoProject.Stones[i].isSelected != parent.selectedDominoes.Contains(i))
                {
                    discrepancy = true;
                }
            }
            if (discrepancy)
            {
                var res = MessageBox.Show("Discrepancy detected!", "Error", MessageBox.MessageBoxButtons.Ok).Result;
            }
        }
        private void ShowImage()
        {
            try
            {
                if (!File.Exists(PreviewPath))
                {
                    FilteredImage.Save(PreviewPath);
                }
                Process.Start(PreviewPath);
            }
            catch (Exception)
            {

            }
        }
        internal void DeleteImage()
        {
            try
            {
                if (File.Exists(PreviewPath)) File.Delete(PreviewPath);
            }
            catch (Exception) { }
        }
        public bool IsSelected(int i)
        {
            return DominoProject.Stones[i].isSelected;
        }
        private ICommand _ShowImageClick;
        public ICommand ShowImageClick { get { return _ShowImageClick; } set { if (value != _ShowImageClick) { _ShowImageClick = value; } } }

    }
    public class RulerTool : EditingToolVM
    {
        public override void KeyPressed(Key key)
        {
            base.KeyPressed(key);
        }
        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            base.MouseUp(sender, e);
        }
        public override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.MouseDown(sender, e);
        }
    }
    public class ZoomToolVM : EditingToolVM
    {
        public ZoomToolVM(EditProjectVM parent) : base()
        {
            this.parent = parent;
            Image = "zoomDrawingImage";
            Name = "Zoom";
            ZoomIn = new RelayCommand((o) => parent.DisplaySettingsTool.ZoomValue += 1);
            ZoomOut = new RelayCommand((o) => parent.DisplaySettingsTool.ZoomValue -= 1);
        }
        private ICommand _ZoomIn;
        public ICommand ZoomIn { get { return _ZoomIn; } set { if (value != _ZoomIn) { _ZoomIn = value; } } }

        private ICommand _ZoomOut;
        public ICommand ZoomOut { get { return _ZoomOut; } set { if (value != _ZoomOut) { _ZoomOut = value; } } }

    }

}
