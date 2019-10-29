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
using System.IO;
using System.Diagnostics;
using Emgu.CV;

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

        public virtual void KeyPressed(Key key) { }
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
            CurrentSelectionDomain?.MouseDown(sender, e, sender as ProjectCanvas);   
        }
        public override void MouseMove(object sender, MouseEventArgs e)
        {
            CurrentSelectionDomain?.MouseMove(sender, e, sender as ProjectCanvas);
        }
        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            var result = CurrentSelectionDomain?.MouseUp(sender, e, sender as ProjectCanvas);
            if (CurrentSelectionDomain?.CurrentSelectionMode == SelectionMode.Add)
            {
                result.ForEach(x => parent.AddToSelectedDominoes(x));
            }
            else if (CurrentSelectionDomain?.CurrentSelectionMode == SelectionMode.Remove)
            {
                result.ForEach(x => parent.RemoveFromSelectedDominoes(x));
            }
            parent.UpdateUIElements();
        }
        public override void KeyPressed(Key key)
        {
            if (key == Key.Escape)
            {
                parent.ClearFullSelection();
            }
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
        public TwoClickSelection()
        {
            MouseDownPoint = new Point(-1, -1);
        }
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
            MouseDownPoint = new Point(-1, -1);
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
        private int _ZoomValue = 1;
        public int ZoomValue
        {
            get { return _ZoomValue; }
            set
            {
                if (_ZoomValue != value)
                {
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
                string imagepath = System.Windows.Forms.Application.LocalUserAppDataPath;
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
            parent.ClearFullSelection();
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
            catch (Exception ex) { }
        }
        private ICommand _ShowImageClick;
        public ICommand ShowImageClick { get { return _ShowImageClick; } set { if (value != _ShowImageClick) { _ShowImageClick = value; } } }

    }

}
