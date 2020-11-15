using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using Avalonia.Controls;
using Avalonia;
using System.Linq;
using System.IO;
using Avalonia.Media;
using DominoPlanner.Usage.UserControls.View;
using Avalonia.Collections;
using DominoPlanner.Usage.UserControls.ViewModel;
using Avalonia.Input;
using SkiaSharp;
using Avalonia.Skia;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using System.Diagnostics;
using DominoPlanner.Core;
using Avalonia.Data.Converters;
using System.Globalization;
using Avalonia.Controls.Shapes;
using Avalonia.VisualTree;

namespace DominoPlanner.Usage
{
    public class ProjectCanvas : Control
    {


        public double ShiftX
        {
            get { return GetValue(ShiftXProperty); }
            set { SetValue(ShiftXProperty, value); }
        }

        public static readonly StyledProperty<double> ShiftXProperty =  AvaloniaProperty.Register<ProjectCanvas, double>(nameof(ShiftX));

        public double ShiftY
        {
            get { return GetValue(ShiftYProperty); }
            set { SetValue(ShiftYProperty, value); }
        }

        public static readonly StyledProperty<double> ShiftYProperty = AvaloniaProperty.Register<ProjectCanvas, double>(nameof(ShiftY));

        public double Zoom
        {
            get { return GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public static readonly StyledProperty<double> ZoomProperty = AvaloniaProperty.Register<ProjectCanvas, double>(nameof(Zoom));

        public bool Expanded
        {
            get { return GetValue(ExpandedProperty); }
            set { SetValue(ExpandedProperty, value); }
        }

        public static readonly StyledProperty<bool> ExpandedProperty = AvaloniaProperty.Register<ProjectCanvas, bool>(nameof(Expanded));

        public Color UnselectedBorderColor
        {
            get { return GetValue(UnselectedBorderColorProperty); }
            set { SetValue(UnselectedBorderColorProperty, value); }
        }

        public static readonly StyledProperty<Color> UnselectedBorderColorProperty = AvaloniaProperty.Register<ProjectCanvas, Color>(nameof(UnselectedBorderColor));

        public Color SelectedBorderColor
        {
            get { return GetValue(SelectedBorderColorProperty); }
            set { SetValue(SelectedBorderColorProperty, value); }
        }

        public static readonly StyledProperty<Color> SelectedBorderColorProperty = AvaloniaProperty.Register<ProjectCanvas, Color>(nameof(SelectedBorderColor));

        public float ImageOpacity
        {
            get { return GetValue(ImageOpacityProperty); }
            set { SetValue(ImageOpacityProperty, value); }
        }

        public static readonly StyledProperty<float> ImageOpacityProperty = AvaloniaProperty.Register<ProjectCanvas, float>(nameof(ImageOpacity));

        public AvaloniaList<EditingDominoVM> Project
        {
            get { return GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public static readonly StyledProperty<AvaloniaList<EditingDominoVM>> ProjectProperty = AvaloniaProperty.Register<ProjectCanvas, AvaloniaList<EditingDominoVM>>(nameof(Project));

        public SKPath SelectionDomain
        {
            get { return GetValue(SelectionDomainProperty); }
            set { SetValue(SelectionDomainProperty, value); }
        }

        public static readonly StyledProperty<SKPath> SelectionDomainProperty = AvaloniaProperty.Register<ProjectCanvas, SKPath>(nameof(SelectionDomain));

        public Color SelectionDomainColor
        {
            get { return GetValue(SelectionDomainColorProperty); }
            set { SetValue(SelectionDomainColorProperty, value); }
        }
        public static readonly StyledProperty<Color> SelectionDomainColorProperty = AvaloniaProperty.Register<ProjectCanvas, Color>(nameof(SelectionDomainColor));

        public bool SelectionDomainVisible
        {
            get { return GetValue(SelectionDomainVisibleProperty); }
            set { SetValue(SelectionDomainVisibleProperty, value); }
        }
        public static readonly StyledProperty<bool> SelectionDomainVisibleProperty = AvaloniaProperty.Register<ProjectCanvas, bool>(nameof(SelectionDomainVisible));

        public AvaloniaList<int> SelectedDominoes
        {
            get { return (AvaloniaList<int>)GetValue(SelectedDominoesProperty); }
            set { SetValue(SelectedDominoesProperty, value); }
        }

        public static readonly StyledProperty<AvaloniaList<int>> SelectedDominoesProperty = AvaloniaProperty.Register<ProjectCanvas, AvaloniaList<int>>(nameof(SelectedDominoes));


        public double ProjectHeight { get; set; }

        public double ProjectWidth { get; set; }



        public double BorderSize;

        public Image<Emgu.CV.Structure.Bgra, byte> OriginalImage;

        public bool above;
        private double opacity_value;

        public double OpacityValue
        {
            get { return opacity_value; }
            set { opacity_value = value; }
        }
        public ProjectCanvas()
        {
            AffectsRender<ProjectCanvas>(ShiftXProperty);
            AffectsRender<ProjectCanvas>(ShiftYProperty);
            AffectsRender<ProjectCanvas>(ZoomProperty);
            //Stones = new List<DominoInCanvas>();
            //Stones.Add(new DominoInCanvas(50, 50, 50, 50, Colors.AliceBlue));
            DataContextProperty.Changed.AddClassHandler<ProjectCanvas>((o, e) => SubscribeEvents(o, e));
            AffectsRender<ProjectCanvas>(ProjectProperty);
            AffectsRender<ProjectCanvas>(ExpandedProperty);
            AffectsRender<ProjectCanvas>(SelectionDomainProperty);
            AffectsRender<ProjectCanvas>(SelectionDomainProperty);
            AffectsRender<ProjectCanvas>(SelectionDomainVisibleProperty);
            AffectsRender<ProjectCanvas>(SelectionDomainColorProperty);
            
        }



        private void SubscribeEvents(ProjectCanvas o, AvaloniaPropertyChangedEventArgs e)
        {
            var oldv = e.OldValue as EditProjectVM;
            var newv = e.NewValue as EditProjectVM;

            if (oldv != null)
            {
                this.KeyDown -= oldv.PressedKey;
            }
            if (newv != null)
            {
                this.KeyDown += newv.PressedKey;
            }
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            
            var p = e.GetCurrentPoint(this);
            if (DataContext is EditProjectVM ed)
                ed.Canvas_MouseDown(ScreenPointToDominoCoordinates(p.Position), e);
        }
        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            var p = e.GetCurrentPoint(this);
            if (DataContext is EditProjectVM ed)
                ed.Canvas_MouseUp(ScreenPointToDominoCoordinates(p.Position), e);
        }
        protected override void OnPointerMoved(PointerEventArgs e)
        {
            var p = e.GetCurrentPoint(this);
            if (DataContext is EditProjectVM ed)
                ed.Canvas_MouseMove(ScreenPointToDominoCoordinates(p.Position), e);
        }
        Avalonia.Point ScreenPointToDominoCoordinates(Avalonia.Point p)
        {
            var tempx = p.X / Zoom + ShiftX;
            var tempy = p.Y / Zoom + ShiftY;
            return new Avalonia.Point(tempx, tempy);
        }
        protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
        {
            var oldx = ShiftX;
            var oldy = ShiftY;
            double newx = ShiftX, newy = ShiftY;

            base.OnPointerWheelChanged(e);
            var delta = e.Delta.Y;
            if (e.KeyModifiers == KeyModifiers.Control)
            {
                Debug.WriteLine("Raw position: " + e.GetPosition(this));
                // get the screen coordinate of the current point.
                var p = ScreenPointToDominoCoordinates(e.GetPosition(this));
                Debug.WriteLine("Computed position in domino coordinates: " + oldx + ", " + oldy);
                if (delta > 0)
                    Zoom *= 1.1;
                else
                    Zoom *= 1 / 1.1;
                
                newx = (p.X - e.GetPosition(this).X / Zoom);
                
                newy = (p.Y - e.GetPosition(this).Y / Zoom);
            }
            else if (e.KeyModifiers == KeyModifiers.Shift)
            {
                newx = oldx - 100 * delta;
            }
            else
            {
                newy = oldy - 100 * delta;
            }
            ShiftX = newx;
            ShiftY = newy;

        }


        public override void Render(DrawingContext context)
        {
            /*IImage bit = null;
            if (opacity_value != 0)
            {
                var reduced = OriginalImage.Clone();
                Core.ImageExtensions.OpacityReduction(reduced, opacity_value);
                //bit = BitmapSourceConvert.ToBitmapSource(reduced);
            }
            base.Render(dc);

            var unselectedBrush = new SolidColorBrush(UnselectedBorderColor);
            var selectedBrush = new SolidColorBrush(SelectedBorderColor);
            if (!above && bit != null)
                dc.DrawImage(bit, new Rect(0, 0, Width, Height));
            foreach (DominoInCanvas dic in Stones)
            {
                Point point1 = dic.canvasPoints[0];
                Point point2 = dic.canvasPoints[1];
                Point point3 = dic.canvasPoints[2];
                Point point4 = dic.canvasPoints[3];

                StreamGeometry streamGeometry = new StreamGeometry();
                using (StreamGeometryContext geometryContext = streamGeometry.Open())
                {
                    geometryContext.BeginFigure(point1, true);
                    geometryContext.LineTo(point2);
                    geometryContext.LineTo(point3);
                    geometryContext.LineTo(point4);
                    geometryContext.EndFigure(true);
                }

                Pen pen = new Pen();
                if (dic.isSelected)
                {
                    pen.Brush = selectedBrush;
                    pen.Thickness = BorderSize;
                }
                else if (dic.PossibleToPaste)
                {
                    pen.Brush = Brushes.Plum;
                    pen.Thickness = BorderSize;
                }
                else
                {
                    pen.Brush = unselectedBrush;
                    pen.Thickness = BorderSize / 2;
                }

                dc.DrawGeometry(new SolidColorBrush(dic.StoneColor), pen, streamGeometry);
            }
            if (above && bit != null)
                dc.DrawImage(bit, new Rect(0, 0, Width, Height));
            */
            
            context.Custom(new DominoRenderer(this));
            //Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }
    }
    public static class BitmapSourceConvert
    {
        /*[DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }*/
    }
    public class DominoRenderer : ICustomDrawOperation
    {
        private readonly FormattedText _noSkia;
        private float shift_x;
        private float shift_y;
        private float zoom;
        private SKColor unselectedBorderColor;
        private SKColor selectedBorderColor;
        private SKPath selectionPath;
        private bool selectionVisible;
        private SKColor selectionColor;
        private AvaloniaList<EditingDominoVM> project;
        private AvaloniaList<int> selected;

        public Rect Bounds { get; set; }
        public TransformedBounds? TightBounds { get; set; }


        public DominoRenderer(ProjectCanvas pc)
        {
            _noSkia = new FormattedText()
            {
                Text = "Current rendering API is not Skia"
            };
            Bounds = new Rect(0, 0, pc.Bounds.Width, pc.Bounds.Height);
            TightBounds = pc.TransformedBounds;
            shift_x = (float)pc.ShiftX;
            shift_y = (float)pc.ShiftY;
            zoom = (float)pc.Zoom;
            unselectedBorderColor = new SKColor(pc.UnselectedBorderColor.R, pc.UnselectedBorderColor.G, pc.UnselectedBorderColor.B, pc.UnselectedBorderColor.A);
            selectedBorderColor = new SKColor(pc.SelectedBorderColor.R, pc.SelectedBorderColor.G, pc.SelectedBorderColor.B, pc.SelectedBorderColor.A);
            selectionColor = new SKColor(pc.SelectionDomainColor.R, pc.SelectionDomainColor.G, pc.SelectionDomainColor.B, 255);
            selectionPath = pc.SelectionDomain.Clone();
            selected = pc.SelectedDominoes ?? new AvaloniaList<int>();
            this.project = pc.Project;
            // Transform the selection path into screen coordinates
            var transform = SKMatrix.CreateScaleTranslation(zoom, zoom, -shift_x * zoom, -shift_y * zoom);

            selectionPath?.Transform(transform);
            selectionVisible = pc.SelectionDomainVisible;
        }

        public void Dispose()
        {

        }

        public bool Equals([AllowNull] ICustomDrawOperation other) => false;
        public bool HitTest(Avalonia.Point p) => true;

        public void Render(IDrawingContextImpl context)
        {
            var canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;
            if (canvas == null)
            {
                context.DrawText(Brushes.Black, new Avalonia.Point(), _noSkia.PlatformImpl);
                return;
            }

            if (project == null)
                return;

            canvas.Save();
            for (int i = 0; i < project.Count; i++)
            {
                DrawDomino(canvas, project[i]);
            }
            if (selectionVisible && selectionPath != null)
            {
                canvas.DrawPath(selectionPath, new SKPaint() { Color = new SKColor(0, 0, 0, 255), IsStroke = true, StrokeWidth = 4, IsAntialias = true });
                canvas.DrawPath(selectionPath, new SKPaint() { Color = selectionColor, IsStroke = true, StrokeWidth = 2, IsAntialias = true });
            }
            canvas.Restore();


        }
        public SKPoint PointToDisplaySkiaPoint(Avalonia.Point p)
        {
            return new SKPoint((float)((p.X - shift_x) * zoom), (float)((p.Y - shift_y) * zoom));
        }
        public SKPoint PointToDisplaySkiaPoint(Core.Point p)
        {
            return new SKPoint((float)((p.X - shift_x) * zoom), (float)((p.Y - shift_y) * zoom));
        }
        public Avalonia.Point PointToDisplayAvaloniaPoint(Avalonia.Point p)
        {
            return new Avalonia.Point((float)((p.X - shift_x) * zoom), (float)((p.Y - shift_y) * zoom));
        }
        
        private void DrawDomino(SKCanvas canvas, EditingDominoVM vm)
        {
            var shape = vm.domino;
            var c = vm.StoneColor;
            var dp = shape.GetPath(1, false);
            // is the domino visible at all?
            var inside = dp.points.Select(x => new Avalonia.Point(x.X, x.Y)).Sum(x => Bounds.Contains(PointToDisplayAvaloniaPoint(x)) ? 1 : 0);
            if (inside == dp.points.Length)
            {
                var path = new SKPath();
                path.MoveTo(PointToDisplaySkiaPoint(dp.points[0]));
                foreach (var line in dp.points.Skip(0))
                    path.LineTo(PointToDisplaySkiaPoint(line));
                path.Close();

                canvas.DrawPath(path, new SKPaint() { Color = new SKColor(c.R, c.G, c.B, c.A), IsAntialias = true, IsStroke = false });
                if (vm.isSelected)
                {
                    canvas.DrawPath(path, new SKPaint() { Color = selectedBorderColor, IsAntialias = true, IsStroke = true, StrokeWidth = 2 * zoom, PathEffect = SKPathEffect.CreateDash(new float[] { 8 * zoom, 2 * zoom}, 10 * zoom) });
                }
                else
                {

                    canvas.DrawPath(path, new SKPaint() { Color = unselectedBorderColor, IsAntialias = true, IsStroke = true, StrokeWidth = 1 * zoom });
                }
            }
        }
    }
}
