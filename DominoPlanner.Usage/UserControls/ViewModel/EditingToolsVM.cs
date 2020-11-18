using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
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
using Avalonia.VisualTree;
using System.Threading.Tasks;
using SkiaSharp;
using Avalonia.Data.Converters;

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
            set { image = value;
                Application.Current.TryFindResource(value, out object temp);
                img = (DrawingImage)temp;
            }
        }

        public DrawingImage img { get; private set; }

        public virtual void MouseMove(Avalonia.Point dominoPoint, PointerEventArgs e) { }

        public virtual void MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e) { }

        public virtual void MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e) { }

        public virtual void KeyPressed(KeyEventArgs key) { }

        public virtual void MouseWheel(Avalonia.Point dominoPoint, PointerWheelEventArgs e) { }
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
                new RectangleSelection(parent), new CircleSelectionDomain(parent),
                new PolygonSelectionDomain(parent), new FreehandSelectionDomain(parent) };
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

        public override void MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e)
        {
            CurrentSelectionDomain?.MouseDown(dominoPoint, e);   
        }
        public override void MouseMove(Avalonia.Point dominoPoint, PointerEventArgs e)
        {
            CurrentSelectionDomain?.MouseMove(dominoPoint, e);
        }
        public override void MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e)
        {
            var result = CurrentSelectionDomain?.MouseUp(dominoPoint, e);
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
        public override void KeyPressed(KeyEventArgs key)
        {
            if (key.Key == Key.Escape)
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
    public class SelectionModeColorConverter : IValueConverter
    {
        static Color AddColor = Colors.LightBlue;
        static Color RemoveColor = Colors.IndianRed;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SelectionMode sm)
            {
                if (sm == SelectionMode.Add)
                {
                    return AddColor;
                }
                else return RemoveColor;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public abstract class SelectionDomain : ModelBase
    {
        internal EditProjectVM parent;
        internal SelectionDomain(EditProjectVM parent)
        {
            this.parent = parent;
        }
        public string Name { get; internal set; }
        public DrawingImage img { get; private set; }
        private string image;

        public string Image
        {
            get { return image; }
            set { image = value; Application.Current.TryFindResource(value, out object temp);
                img = (DrawingImage)temp;
            }
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
        protected bool ResetFlag = false;

        private SelectionMode _currentSelectionMode;
        public SelectionMode CurrentSelectionMode
        {
         get { return _currentSelectionMode; }
            set { _currentSelectionMode = value; RaisePropertyChanged(); }
        }
        private SKPath selectionshape;
        public SKPath SelectionShape
        {
            get { return selectionshape; }
            set
            {
                selectionshape = value;
                RaisePropertyChanged();
            }
        }
        private bool _selectionVisible;

        public bool SelectionPathVisible
        {
            get { return _selectionVisible; }
            set { _selectionVisible = value; RaisePropertyChanged(); }
        }
        private Color selectionFill;

        public Color  SelectionFillColor
        {
            get { return selectionFill; }
            set { selectionFill = value; RaisePropertyChanged(); }
        }


        public void RemoveSelectionDomain()
        {
            SelectionPathVisible = false;
        }
        public void AddSelectionDomain()
        {
            SelectionPathVisible = true;
        }
        public abstract void MouseMove(Avalonia.Point dominoPoint, PointerEventArgs e);

        public abstract void MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e);

        public virtual List<int> MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e)
        {
            return new List<int>();
        }
        public Rect GetBoundingBox()
        {
            SKRect rect;
            SelectionShape.GetBounds(out rect);
            return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        public bool IsInsideBoundingBox(Rect BoundingBox, EditingDominoVM dic, bool includeBoundary)
        {
            var points = dic.domino.GetPath(expanded: dic.expanded).points;
            if (includeBoundary)
            {
                return points.Max(x => x.X) > BoundingBox.Left &&
                    points.Min(x => x.X) < BoundingBox.Right &&
                    points.Min(x => x.Y) < BoundingBox.Bottom &&
                    points.Max(x => x.Y) > BoundingBox.Top;
            }
            else
            {
                return points.Min(x => x.X) > BoundingBox.Left &&
                    points.Max(x => x.X) < BoundingBox.Right &&
                    points.Max(x => x.Y) < BoundingBox.Bottom &&
                    points.Min(x => x.Y) > BoundingBox.Top;
                
            }
        }

        public abstract bool IsInside(EditingDominoVM dic, Rect boundingBox, bool includeBoundary);

        public void ResetSelectionArea()
        {
            ResetFlag = true;
        }
        public bool CheckBoundary(EditingDominoVM dic, int pointsInsideSelection)
        {
            if (IncludeBoundary)
            {
                if (pointsInsideSelection > 0)
                    return true;
            }
            else
            {
                if (pointsInsideSelection == dic.domino.GetPath().points.Length)
                    return true;
            }
            return false;
        }
    }
    public abstract class TwoClickSelection : SelectionDomain
    {
        public TwoClickSelection(EditProjectVM parent) : base(parent)
        {
            MouseDownPoint = new Avalonia.Point(-1, -1);
        }
        protected Avalonia.Point MouseDownPoint;
        protected Avalonia.Point currentPoint;
        public void UpdateSelectionMode(PointerPressedEventArgs e) // called on Mouse down
        {
            var update = e.GetCurrentPoint(null).Properties.PointerUpdateKind;
            if (update == PointerUpdateKind.LeftButtonPressed || update == PointerUpdateKind.RightButtonPressed)
            {
                if (SelectionMode == SelectionMode.Add || (SelectionMode == SelectionMode.Neutral && update == PointerUpdateKind.LeftButtonPressed))
                {
                    CurrentSelectionMode = SelectionMode.Add;
                }
                else if (SelectionMode == SelectionMode.Remove || (SelectionMode == SelectionMode.Neutral && update == PointerUpdateKind.RightButtonPressed))
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

        public override void MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e)
        {
            UpdateSelectionMode(e);

            MouseDownPoint = dominoPoint;
            Initialize();
            ResetFlag = false;
            
            AddSelectionDomain();
        }
        public override void MouseMove(Avalonia.Point dominoPoint, PointerEventArgs e)
        {
            var props = e.GetCurrentPoint(null).Properties;
            if (!props.IsLeftButtonPressed && !props.IsRightButtonPressed)
            {
                RemoveSelectionDomain();
                return;
            }
            if (SelectionShape == null) return;
            
            UpdateShapeProperties(dominoPoint);

            
        }
        public override List<int> MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e)
        {
            List<int> result = new List<int>();
            var props = e.GetCurrentPoint(null).Properties;
            if ((props.IsLeftButtonPressed || props.IsRightButtonPressed)
                || (MouseDownPoint.X == -1 && MouseDownPoint.Y == -1))
            {
                return result;
            }
            Rect boundingBox = GetBoundingBox();
            bool SingleClickFlag = false;
            var pos = dominoPoint;
            if ((pos.X - MouseDownPoint.X) * (pos.X - MouseDownPoint.X) + (pos.Y - MouseDownPoint.Y) * (pos.Y - MouseDownPoint.Y) < 5)
            {
                // single click 
                boundingBox = new Rect(dominoPoint.X, dominoPoint.Y, 0, 0);
                SingleClickFlag = true;
            }
            if (!ResetFlag)
            {
                for (int i = 0; i < parent.Dominoes.Count; i++)
                {
                    if (parent.Dominoes[i] is EditingDominoVM dic && IsInside(dic, boundingBox, SingleClickFlag ? true : IncludeBoundary))
                    {
                        result.Add(i);
                    }
                }
            }
            ResetFlag = false;
            RemoveSelectionDomain();
            MouseDownPoint = new Avalonia.Point(-1, -1);
            return result;
        }
        
        
    }
    public abstract class WidthHeightSelection : TwoClickSelection
    {
        internal WidthHeightSelection(EditProjectVM parent) : base(parent) { }
        
        public override void Initialize()
        {
            SelectionShape = new SKPath();
        }
        public abstract Rect GetCurrentDimensions(Avalonia.Point pos);

        public override void UpdateShapeProperties(Avalonia.Point pos)
        {
            currentPoint = pos;
            var dims = GetCurrentDimensions(pos);
            if (dims.Width > 10 || dims.Height > 10)
                SelectionPathVisible = true;
            else
                SelectionPathVisible = false;

        }
    }
    public class RectangleSelection : WidthHeightSelection
    {
        
        public RectangleSelection(EditProjectVM parent) : base(parent)
        {
            Image = "rect_selectDrawingImage";
            Name = "Rectangle";
        }
        public override void UpdateShapeProperties(Avalonia.Point pos)
        {
            var p = new SKPath();
            var dims = GetCurrentDimensions(pos);
            p.AddRect(new SKRect() { Left = (float)dims.X, Top = (float)dims.Y, Size = new SKSize() { Width = (float)dims.Width, Height = (float)dims.Height } });
            SelectionShape = p;
            base.UpdateShapeProperties(pos);
        }
        public override Rect GetCurrentDimensions(Avalonia.Point pos)
        {
            var x = Math.Min(pos.X, MouseDownPoint.X);
            var y = Math.Min(pos.Y, MouseDownPoint.Y);

            var w = Math.Max(pos.X, MouseDownPoint.X) - x;
            var h = Math.Max(pos.Y, MouseDownPoint.Y) - y;
            return new Rect(x, y, w, h);

        }
        public override bool IsInside(EditingDominoVM dic, Rect boundingBox, bool includeBoundary)
        {
            return IsInsideBoundingBox(boundingBox, dic, includeBoundary);
        }
    }
    public class CircleSelectionDomain : WidthHeightSelection
    {
        public CircleSelectionDomain(EditProjectVM parent) : base(parent)
        {
            Image = "round_selectDrawingImage";
            Name = "Circle";
        }
        public override void UpdateShapeProperties(Avalonia.Point pos)
        {
            base.UpdateShapeProperties(pos);
            var p = new SKPath();
            var dims = GetCurrentDimensions(pos);
            p.AddCircle((float)MouseDownPoint.X, (float)MouseDownPoint.Y, (float)dims.Width/2);
            SelectionShape = p;
        }
        public override Rect GetCurrentDimensions(Avalonia.Point pos)
        {
            var radius = Math.Sqrt(Math.Pow(pos.X - MouseDownPoint.X, 2) + Math.Pow(pos.Y - MouseDownPoint.Y, 2));
            return new Rect(MouseDownPoint.X - radius, MouseDownPoint.Y - radius, 2 * radius, 2*radius);
        }

        public override bool IsInside(EditingDominoVM dic, Rect boundingBox, bool includeBoundary)
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
        public PointerUpdateKind? firstButton;
        public PolygonSelectionDomain(EditProjectVM parent) : base(parent)
        {
            Image = "poly_selectDrawingImage";
            Name = "Polygon";
            points = new List<Avalonia.Point>();
            SelectionFillColor = Color.FromArgb(50, 100, 100, 100);
            
        }

        public override bool IsInside(EditingDominoVM dic, Rect boundingBox, bool includeBoundary)
        {
            if (IsInsideBoundingBox(boundingBox, dic, includeBoundary))
            {
                var pts = dic.domino.GetPath(expanded: dic.expanded).points;
                var insidePoly = pts.Count(x => SelectionShape.Contains((float)x.X, (float)x.Y));
                return CheckBoundary(dic, insidePoly);
            }
            return false;
        }
        bool DoubleClickFlag;
        public override void MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(null).Properties;
            if (!((props.PointerUpdateKind ==  PointerUpdateKind.LeftButtonPressed || props.PointerUpdateKind == PointerUpdateKind.RightButtonPressed) &&
                (props.IsLeftButtonPressed ^ props.IsRightButtonPressed)))
                return;
            Debug.WriteLine(points.Count);
            if (points.Count == 0)
            {
                firstButton = props.PointerUpdateKind;
                if (SelectionMode == SelectionMode.Neutral)
                {
                    CurrentSelectionMode = props.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed ? SelectionMode.Add : SelectionMode.Remove;
                }
                else
                {
                    CurrentSelectionMode = SelectionMode;
                }
                AddSelectionDomain();
                points.Add(dominoPoint);
            }
            else if (e.ClickCount == 2)
            {
                DoubleClickFlag = true;
            }
            if (props.PointerUpdateKind != firstButton)
            {
                // selection canceled, clear polygon
                points.Clear();
                RemoveSelectionDomain();
                return;
            }
            points.Add(dominoPoint);
            UpdatePath();
            
        }
        private void UpdatePath()
        {
            var p = new SKPath();
            if (points != null && points.Count > 0)
            {
                p.MoveTo((float)points[0].X, (float)points[0].Y);
                foreach (Avalonia.Point point in points.Skip(1))
                    p.LineTo((float)point.X, (float)point.Y);
            }
            SelectionShape = p;
        }
        public override void MouseMove(Avalonia.Point dominoPoint, PointerEventArgs e)
        {
            if (SelectionShape == null) return;
            if (points.Count >= SelectionShape.PointCount - 1 && points.Count > 0)
                points[SelectionShape.PointCount - 1] = dominoPoint;
            UpdatePath();
        }

        public override List<int> MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e)
        {
            var result = new List<int>();
            if (!DoubleClickFlag)
                return result;
            DoubleClickFlag = false;
            var boundingBox = GetBoundingBox();
            for (int i = 0; i < parent.Dominoes.Count; i++)
            {
                if (parent.Dominoes[i] is EditingDominoVM dic && IsInside(dic, boundingBox, IncludeBoundary))
                {
                    result.Add(i);
                }
            }
            RemoveSelectionDomain();
            points = new List<Avalonia.Point>();
            firstButton = null;
            return result;
        }
    }
    public class FreehandSelectionDomain : TwoClickSelection
    {
        List<Avalonia.Point> points;
        public FreehandSelectionDomain(EditProjectVM parent) : base(parent)
        {
            Image = "freehand_selectDrawingImage";
            Name = "Freehand";
            SelectionShape = new SKPath();
            MouseDownPoint = new Avalonia.Point(-1, -1);
            SelectionFillColor = Color.FromArgb(50, 100, 100, 100);
            points = new List<Avalonia.Point>();

        }
        public override void Initialize()
        {
            points.Clear();
            points.Add(MouseDownPoint);
            UpdatePath();
        }
        private void UpdatePath()
        {
            var p = new SKPath();
            if (points != null && points.Count > 1)
            {
                p.MoveTo((float)points[0].X, (float)points[0].Y);
                foreach (Avalonia.Point point in points.Skip(1))
                    p.LineTo((float)point.X, (float)point.Y);
            }
            SelectionShape = p;
        }

        public override bool IsInside(EditingDominoVM dic, Rect boundingBox, bool includeBoundary)
        {
            if (IsInsideBoundingBox(boundingBox, dic, includeBoundary))
            {
                var insidePoly = dic.canvasPoints.Count(x => SelectionShape.Contains((float)x.X, (float)x.Y));
                return CheckBoundary(dic, insidePoly);
            }
            return false;
        }

        public override void UpdateShapeProperties(Avalonia.Point pos)
        {
            var last = points.Last();
            Debug.WriteLine("Hit, Length: " + points.Count);
            if ((last.X - pos.X) * (last.X - pos.X) + (last.Y - pos.Y) * (last.Y - pos.Y) > 3)
            {
                points.Add(pos);
                UpdatePath();
            }
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
        /*public ProjectCanvas DominoProject
        {
            get { return _DominoProject; }
            set
            {
                if (_DominoProject != value)
                {
                    if (_DominoProject != null)
                    {
                        //_DominoProject.SizeChanged -= _DominoProject_SizeChanged;
                    }
                    _DominoProject = value;
                    RaisePropertyChanged();
                    //_DominoProject.SizeChanged += _DominoProject_SizeChanged;

                    //_DominoProject.HorizontalAlignment = HorizontalAlignment.Stretch;
                    //_DominoProject.VerticalAlignment = VerticalAlignment.Stretch;
                }
            }
        }*/
        public DisplaySettingsToolVM(EditProjectVM parent)
        {
            Image = "display_settingsDrawingImage";
            Name = "View Properties";
            this.parent = parent;
            ShowImageClick = new RelayCommand(o => { ShowImage(); });

            if (parent.CurrentProject != null && parent.CurrentProject.PrimaryImageTreatment != null)
            {
                if (parent.CurrentProject.PrimaryImageTreatment.FilteredImage != null)
                {
                    FilteredImage = parent.CurrentProject.PrimaryImageTreatment.FilteredImage;
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
                            FilteredImage = SKBitmap.Decode(absolutePath);
                            bff.FilePath = relativePath;
                        }
                    }
                }
            }
            Expandable = parent.CurrentProject is FieldParameters;
        }
        private SKBitmap _FilteredImage;

        public SKBitmap FilteredImage
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
                    parent.ResetCanvas();
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
        private Color backgroundColor = Color.FromArgb(255, 255, 255, 0);
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                RaisePropertyChanged();
            }
        }
        private Color borderColor = Color.FromArgb(50, 0, 0, 255);
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                RaisePropertyChanged();
                Redraw();
            }
        }
        private Color selectedColor = Colors.Blue;
        public Color SelectedColor
        {
            get => selectedColor;
            set
            {
                selectedColor = value;
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
                //DominoProject.BorderSize = BorderSize;
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
                    _ZoomValue = value;
                    RaisePropertyChanged();
                }
            }
        }
        private double horizontalOffset;

        public double HorizontalOffset
        {
            get { return horizontalOffset; }
            set { horizontalOffset = value; RaisePropertyChanged(); }
        }

        private double verticalOffset;

        public double VerticalOffset
        {
            get { return verticalOffset; }
            set { verticalOffset = value; RaisePropertyChanged(); }
        }

        private string PreviewPath
        {
            get
            {
                string imagepath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                imagepath = System.IO.Path.Combine(imagepath, parent.name + "_prev.png");
                return imagepath;
            }
        }
        /*private void _DominoProject_SizeChanged(object sender,  e)
        {
            RefreshTransformation();
        }*/
        internal void RefreshTransformation()
        {
            if (_DominoProject == null)
                return;
            double ScaleX, ScaleY;

            ScaleX = visibleWidth / largestX * ZoomValue;
            ScaleY = visibleHeight / largestY * ZoomValue;

            if (ScaleX < ScaleY)
                _DominoProject.RenderTransform = new ScaleTransform(ScaleX, ScaleX);
            else
                _DominoProject.RenderTransform = new ScaleTransform(ScaleY, ScaleY);
            _DominoProject.InvalidateVisual();
        }
        
        internal void SizeChanged(double width, double height)
        {
            visibleWidth = width;
            visibleHeight = height;
            RefreshTransformation();
        }
        
        public void Redraw()
        {
            /*DominoProject?.InvalidateVisual();
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
                Errorhandler.RaiseMessage("Discrepancy detected!", "Error", Errorhandler.MessageType.Error);
            }*/
        }
        private void ShowImage()
        {
            try
            {
                if (!File.Exists(PreviewPath))
                {
                    using (var image = SKImage.FromBitmap(FilteredImage))
                    using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
                    {
                        // save the data to a stream
                        using (var stream = File.OpenWrite(PreviewPath))
                        {
                            data.SaveTo(stream);
                        }
                    }
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
            //return DominoProject.Stones[i].isSelected;
            return false;
        }
        private ICommand _ShowImageClick;
        public ICommand ShowImageClick { get { return _ShowImageClick; } set { if (value != _ShowImageClick) { _ShowImageClick = value; } } }
    }
    public class RulerTool : EditingToolVM
    {
        public override void KeyPressed(KeyEventArgs key)
        {
            base.KeyPressed(key);
        }
        public override void MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e)
        {
            base.MouseUp(dominoPoint, e);
        }
        public override void MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e)
        {
            base.MouseDown(dominoPoint, e);
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
