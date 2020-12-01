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
using DominoPlanner.Core.RTree;
using Avalonia.Collections;

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
                Img = (DrawingImage)temp;
            }
        }

        public DrawingImage Img { get; private set; }

        public virtual void MouseMove(Avalonia.Point dominoPoint, PointerEventArgs e) { }

        public virtual void MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e) { }

        public virtual void MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e) { }

        public virtual void KeyPressed(KeyEventArgs key) { }

        public virtual void MouseWheel(Avalonia.Point dominoPoint, PointerWheelEventArgs e) { }


        public virtual void LeaveTool() { }
        public virtual void EnterTool() { }
    }
    public enum SelectionMode
    {
        Add,
        Neutral,
        Remove
    }
    public class SelectionOperation : PostFilter
    {
        readonly SelectionToolVM reference;
        readonly IList<int> ToSelect;
        readonly bool positiveselect;
        readonly bool[] oldState;
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
                new PolygonSelectionDomain(parent), new FreehandSelectionDomain(parent), new FillBucketDomain(parent)};
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
            var n = Enumerable.Range(0, parent.dominoTransfer.Length).Except(current).ToList();
            Select(current, false);
            Select(n, true);
            parent.UpdateUIElements();
        }
        public override void LeaveTool()
        {
            CurrentSelectionDomain.RemoveSelectionDomain();
        }
        public override void EnterTool()
        {
            CurrentSelectionDomain.ResetSelectionArea();
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
        static readonly Color AddColor = Colors.LightBlue;
        static readonly Color RemoveColor = Colors.IndianRed;
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
        public DrawingImage Img { get; private set; }
        private string image;

        public string Image
        {
            get { return image; }
            set { image = value; Application.Current.TryFindResource(value, out object temp);
                Img = (DrawingImage)temp;
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
            SelectionShape.GetBounds(out SKRect rect);
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
                var r = parent.FindDominoAtPosition(pos);
                if (r != null) result.Add(r.idx);
                ResetFlag = true;
            }
            if (!ResetFlag)
            {
                for (int i = 0; i < parent.Dominoes.Count; i++)
                {
                    if (parent.Dominoes[i] is EditingDominoVM dic && IsInside(dic, boundingBox, IncludeBoundary))
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
                var insideCircle = dic.CanvasPoints.Count(x =>
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
        readonly List<Avalonia.Point> points;
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
                var insidePoly = dic.CanvasPoints.Count(x => SelectionShape.Contains((float)x.X, (float)x.Y));
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
    public class FillBucketDomain : SelectionDomain
    {

        public bool IncludeDiagonals
        {
            get { return nl.EightNeighbor; }
            set { nl.EightNeighbor = value; RaisePropertyChanged(); }
        }

        private readonly NeighborLocator nl;
        private AvaloniaList<EditingDominoVM> dominoes;
        public FillBucketDomain(EditProjectVM parent) : base(parent)
        {
            if (parent.CurrentProject is FieldParameters)
            {
                nl = new FieldNeighborLocator();
            }
            else
            {
                nl = new GeneralNeighborLocator();
            }
            this.parent = parent;
            Image = "fill_bucketDrawingImage";
            Name = "Fill area";
        }

        public override void MouseDown(Avalonia.Point dominoPoint, PointerPressedEventArgs e)
        {
            var props = e.GetCurrentPoint(null).Properties;
            if (props.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed || props.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
            {
                if (SelectionMode == SelectionMode.Add || (SelectionMode == SelectionMode.Neutral && props.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed))
                {
                    CurrentSelectionMode = SelectionMode.Add;
                }
                else if (SelectionMode == SelectionMode.Remove || (SelectionMode == SelectionMode.Neutral && props.PointerUpdateKind == PointerUpdateKind.RightButtonPressed))
                {
                    CurrentSelectionMode = SelectionMode.Remove;
                }
                else
                {
                    CurrentSelectionMode = SelectionMode.Neutral;
                }
            }
        }
        public override List<int> MouseUp(Avalonia.Point dominoPoint, PointerReleasedEventArgs e)
        {
            // If the canvas has been reset (i.e. after inserting / removing a line), reset the locator (= recalculate R tree)
            if (dominoes != this.parent.Dominoes)
            {
                dominoes = this.parent.Dominoes;
                nl.ResetLocator(parent, dominoes);
            }
            var props = e.GetCurrentPoint(null).Properties;
            if (!(!props.IsLeftButtonPressed && !props.IsRightButtonPressed))
                return new List<int>();

            List<int> neighbors = new List<int>();

            var start = parent.FindDominoAtPosition(dominoPoint);
            if (start == null) return neighbors; // no domino was clicked

            RecursiveSearch(start.idx, neighbors);
            return neighbors;
        }

        private void RecursiveSearch(int dc, List<int> list)
        {
            var neighbors = nl.FindNeighbors(dc);
            foreach (var n in neighbors)
            {
                if (list.Contains(n)) continue;
                if (parent.dominoTransfer[n].Color == parent.dominoTransfer[dc].Color)
                {
                    list.Add(n);
                    RecursiveSearch(n, list);
                }

            }
        }
        public override bool IsInside(EditingDominoVM dic, Rect boundingBox, bool includeBoundary)
        {
            throw new NotImplementedException();
        }
        public override void MouseMove(Avalonia.Point dominoPoint, PointerEventArgs e)
        {

        }
    }
    public abstract class NeighborLocator
    {
        public bool EightNeighbor = false;
        public NeighborLocator()
        {
        }
        public virtual void ResetLocator(EditProjectVM e, AvaloniaList<EditingDominoVM> pc) { }
        public abstract List<int> FindNeighbors(int dc);
    }
    public class FieldNeighborLocator : NeighborLocator
    {
        private AvaloniaList<EditingDominoVM> dominoes;
        private FieldParameters fp;
        private readonly int[] positions = new int[] { -1, 0, 1 };
        public FieldNeighborLocator()
        {

        }
        public override void ResetLocator(EditProjectVM e, AvaloniaList<EditingDominoVM> dominoes)
        {
            this.dominoes = dominoes;
            this.fp = (e.CurrentProject as FieldParameters);
        }

        public override List<int> FindNeighbors(int dc)
        {
            var result = new List<int>();
            var pos = fp.getPositionFromIndex(dc);

            foreach (int i in positions)
            {
                foreach (int j in positions)
                {
                    if (Math.Abs(i) + Math.Abs(j) <= 1 || EightNeighbor)
                    {
                        CheckCandidateAndAddToList(pos.X + i, pos.Y + j, result);
                    }
                }
            }
            return result;
        }
        private void CheckCandidateAndAddToList(int x, int y, List<int> list)
        {
            if (x >= 0 && x < fp.current_width && y >= 0 && y < fp.current_height)
            {
                list.Add(fp.getIndexFromPosition(y, x, 0));
                //var w = fp.getIndexFromPosition(y, x, 0);
            }
        }
    }

    public class GeneralNeighborLocator : NeighborLocator
    {
        private RTree<EditingDominoVM> tree;
        private EditProjectVM parent;
        private AvaloniaList<EditingDominoVM> dominoes;
        public GeneralNeighborLocator() : base() { }

        public override List<int> FindNeighbors(int dc)
        {
            double cl = parent.CurrentProject.charLength;
            var rect = parent.dominoTransfer[dc].GetBoundingRectangle();
            var roi = new DominoRectangle()
            {
                height = cl * 2,
                width = cl * 2,
                x = rect.xc - cl,
                y = rect.yc - cl
            };
            var results = tree.Search(roi);

            // Todo: replace with real distance between polygons
            var ordered_distances = results.Select(r => {
                var current = r.domino.GetBoundingRectangle();
                var dx = Math.Abs(current.xc - rect.xc) - (current.width + rect.width) / 2;
                var dy = Math.Abs(current.yc - rect.yc) - (current.height + rect.height) / 2;
                return new Tuple<EditingDominoVM, double>(r, EightNeighbor ? Math.Min(dx, dy) : Math.Max(dx, dy));
            }).OrderByDescending(x => x.Item2);

            return ordered_distances.Where(x => x.Item2 < cl / 10).Select(x => x.Item1.idx).ToList();
        }
        public override void ResetLocator(EditProjectVM e, AvaloniaList<EditingDominoVM> dominoes)
        {
            if(dominoes != this.dominoes)
            {
                parent = e;
                this.dominoes = dominoes;
                tree = new RTree<EditingDominoVM>(9, new GuttmannQuadraticSplit<EditingDominoVM>());
                var list = parent.Dominoes.OrderByDescending(x =>
                {
                    var container = x.domino.GetContainer();
                    return container.y + container.height / 2;
                }).ThenBy(x =>
                {
                    var container = x.domino.GetContainer();
                    return container.x + container.width / 2;
                }).ToList();

                for (int i = 0; i < parent.Dominoes.Count; i++)
                {
                    // todo: list not used?
                    tree.Insert(parent.Dominoes[i]);
                }
            }
        }
    }
    public class DisplaySettingsToolVM : EditingToolVM
    {
        private double visibleWidth = 0;
        private double visibleHeight = 0;
        private readonly double largestX = 0;
        private readonly double largestY = 0;

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
                    parent.RecreateCanvasViewModel();
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
        private Color backgroundColor = Color.FromArgb(0, 255, 255, 255);
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                RaisePropertyChanged();
            }
        }
        private Color borderColor = Color.FromArgb(127, 0, 0, 255);
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                RaisePropertyChanged();
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
            }
        }

        private double borderSize = 2;

        public double BorderSize
        {
            get { return borderSize; }
            set
            {
                borderSize = value;
                RaisePropertyChanged();
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
        private bool forceRedraw;

        public bool ForceRedraw
        {
            get { return forceRedraw; }
            set { forceRedraw = value; RaisePropertyChanged(); }
        }

        internal void SizeChanged(double width, double height)
        {
            visibleWidth = width;
            visibleHeight = height;
        }
       
        private void ShowImage()
        {
            try
            {
                if (!File.Exists(PreviewPath))
                {
                    using var image = SKImage.FromBitmap(FilteredImage);
                    using var data = image.Encode(SKEncodedImageFormat.Png, 80);
                    // save the data to a stream
                    using var stream = File.OpenWrite(PreviewPath);
                    data.SaveTo(stream);
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
    public class RulerToolVM : EditingToolVM
    {
        private double _length;

        public double Length
        {
            get { return _length; }
            set { _length = value; RaisePropertyChanged(); }
        }
        private bool _snapping;

        public bool Snapping
        {
            get { return _snapping; }
            set { _snapping = value; RaisePropertyChanged(); }
        }

        private Avalonia.Point start;
        private Avalonia.Point end;
        private double linewidth = 8;
        private int dragging;
        public RulerToolVM(EditProjectVM parent)
        {
            this.parent = parent;
            Image = "ruler2DrawingImage";
            Name = "Measure distance";
            MakeInvisible();
        }
        public override void KeyPressed(KeyEventArgs keyArgs)
        {
            var key = keyArgs.Key;
            if (key == Key.LeftCtrl || key == Key.RightCtrl)
                Snapping = !Snapping;
        }
        public override void MouseUp(Avalonia.Point pos, PointerReleasedEventArgs e)
        {
            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

            dragging = 0;
        }
        public override void MouseDown(Avalonia.Point pos, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) return;

            if (parent.AdditionalDrawables.Count == 4)
            {
                if (Distance(pos, start) < (4 * linewidth))
                    dragging = 1;
                else if (Distance(pos, end) < (4 * linewidth))
                    dragging = 2;
                else
                    dragging = 0;
            }
            else // no ruler there yet
            {
                dragging = 0;
                MakeVisible();
            }
            if (dragging == 0)
            {
                start = pos;
                end = pos;
                UpdateShapes();
                dragging = 2;
            }


        }
        public double Distance(Avalonia.Point a, Avalonia.Point b)
        {
            return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }
        public override void MouseMove(Avalonia.Point pos, PointerEventArgs e)
        {
            if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                dragging = 0;
                return;
            }
            var X = pos.X;
            var Y = pos.Y;
            if (pos.X < 0) X = 0;
            if (pos.Y < 0) Y = 0;
            if (pos.X > parent.PhysicalLength) X = parent.PhysicalLength;
            if (pos.Y > parent.PhysicalHeight) Y = parent.PhysicalHeight;

            if (Snapping)
            {
                // get angle between stationary and dragged point
                var stationary = dragging == 1 ? end : start;
                var angle = Math.Atan2(pos.X - stationary.X, pos.Y - stationary.Y);
                // round to multiples of Pi/8 (22.5 deg)
                var steps = 360 / 5;
                angle = Math.Round(angle * steps / Math.PI) * Math.PI / steps;
                var distance = Distance(pos, stationary);
                X = Math.Sin(angle) * distance + stationary.X;
                Y = Math.Cos(angle) * distance + stationary.Y;
            }
            if (dragging == 1)
                start = new Avalonia.Point(X, Y);
            else if (dragging == 2)
                end = new Avalonia.Point(X, Y);
            Length = Distance(start, end);
            UpdateShapes();
        }
        public override void EnterTool()
        {
            Length = 0;
        }
        public override void LeaveTool()
        {
            MakeInvisible();
        }
        public void MakeVisible()
        {
            parent.AdditionalDrawables = new AvaloniaList<CanvasDrawable>();
        }
        public void MakeInvisible()
        {
            parent.AdditionalDrawables = new AvaloniaList<CanvasDrawable>();
        }
        public void UpdateShapes()
        {
            linewidth = 10 / parent.DisplaySettingsTool.ZoomValue;
            var p1 = new SKPath();
            p1.MoveTo((float)start.X,(float) start.Y);
            p1.Close();
            var p2 = new SKPath();
            p2.MoveTo((float)end.X,(float) end.Y);
            p2.Close();

            var circlepaint = new SKPaint() { Color = SKColors.Blue, IsAntialias = true, StrokeCap = SKStrokeCap.Round, StrokeWidth = 10, IsStroke = true};

            var line = new SKPath();
            line.MoveTo((float)start.X,(float) start.Y);
            line.LineTo((float)end.X,(float) end.Y);

            parent.AdditionalDrawables = new AvaloniaList<CanvasDrawable>()
            {
                
                new CanvasDrawable { Path = line, Paint = new SKPaint() {Color = SKColors.White, StrokeWidth = (float) 3, IsStroke = true, IsAntialias=true}},
                new CanvasDrawable { Path = line, Paint = new SKPaint() {Color = SKColors.Black, StrokeWidth = (float) 1, IsStroke = true, IsAntialias=true}},
                new CanvasDrawable { Path = p1, Paint = circlepaint},
                new CanvasDrawable { Path = p2, Paint = circlepaint},
            };
            
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
