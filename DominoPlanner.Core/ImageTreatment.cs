using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using SkiaSharp;

namespace DominoPlanner.Core
{
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(100, typeof(NormalReadout))]
    [ProtoInclude(101, typeof(FieldReadout))]
    public abstract class ImageTreatment
    {
        private SKBitmap source;
        public SKBitmap imageFiltered;
        [ProtoMember(1)]
        private int _width;
        public int Width
        {
            get { return _width; }
            set
            {
                _width = value; sourceValid = false;
            }
        }
        [ProtoMember(2)]
        private int _heigth;
        public int Height
        {
            get => _heigth;
            set { _heigth = value; sourceValid = false; }
        }
        private Color _background;
        [ProtoMember(3)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "Used by Protobuf to serialize _background")]
        private string BackgroundSurrogate
        {
            get
            {
                return _background.ToString();
            }
            set { Background = Color.Parse(value); }
        }
        public Color Background
        {
            get { return _background; }
            set { _background = value; sourceValid = false; }
        }
        private ObservableCollection<ImageFilter> _imageFilters;
        [ProtoMember(4, OverwriteList = true)]
        public ObservableCollection<ImageFilter> ImageFilters
        {
            get => _imageFilters;
            set
            {
                _imageFilters = value;
            }
        }

        public StateReference StateReference { get; set; }

        internal bool sourceValid;

        internal bool imageValid;

        internal bool colorsValid;

        internal IDominoProvider parent;

        public void FillDominos(DominoTransfer shapes)
        {
            if (!sourceValid)
            {
                UpdateSource();
                sourceValid = true;
            }
            if (!imageValid)
            {
                ApplyImageFilters();
                imageValid = true;
            }
            if (!colorsValid)
            {
                ReadoutColors(shapes);
                colorsValid = true;
            }
        }
        public abstract void ReadoutColors(DominoTransfer shapes);
        private void ImageFiltersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                var newitems = e.NewItems;
                foreach (var item in newitems)
                {
                    ((ImageFilter)item).PropertyChanged += new PropertyChangedEventHandler((s, param) => imageValid = false);
                }
            }
            imageValid = false;
        }
        protected void UpdateSource()
        {
            source = new SKBitmap(Width, Height);
            using (SKCanvas canvas = new SKCanvas(source))
            {
                var bg = new SKColor(Background.R, Background.G, Background.B, Background.A);
                canvas.Clear(bg);
            }
            imageValid = false;
        }
        protected void ApplyImageFilters()
        {
            var imageFiltered = source;
            foreach (ImageFilter filter in ImageFilters)
            {
                filter.parent = parent;
                filter.Apply(imageFiltered);
            }
            this.imageFiltered = imageFiltered;
            imageValid = true;
            colorsValid = false;
        }
        protected ImageTreatment()
        {
            ImageFilters = new ObservableCollection<ImageFilter>();
            ImageFilters.CollectionChanged +=
               new NotifyCollectionChangedEventHandler((sender, e) => ImageFiltersChanged(sender, e));
            Background = Colors.Transparent;
        }
        public ImageTreatment(string relativeImagePath, IDominoProvider parent) : this()
        {
            this.parent = parent;
            var BlendFileFilter = new BlendFileFilter() { FilePath = relativeImagePath, parent = parent };
            //BlendFileFilter.UpdateMat();
            Width = BlendFileFilter.GetSizeOfMat().Width;
            Height = BlendFileFilter.GetSizeOfMat().Height;
            BlendFileFilter.CenterX = Width / 2;
            BlendFileFilter.CenterY = Height / 2;
            UpdateSource();
            ImageFilters.Add(BlendFileFilter);
        }
        public ImageTreatment(int width, int height, IDominoProvider parent) : this()
        {
            this.parent = parent;
            Width = width;
            Height = height;
            UpdateSource();
        }

        public SKBitmap FilteredImage
        {
            get
            {
                return imageFiltered;
            }
        }
    }
    [ProtoContract(SkipConstructor =true)]
    public class NormalReadout : ImageTreatment
    {
        #region properties
        private AverageMode _average;
        /// <summary>
        /// Gibt an, ob nur ein Punkt des Dominos (linke obere Ecke) oder ein Durchschnittswert aller Pixel unter dem Pfad verwendet werden soll, um die Farbe auszuwählen.
        /// </summary>
        [ProtoMember(1)]
        public AverageMode Average
        {
            get
            {
                return _average;
            }

            set
            {
                _average = value;
                colorsValid = false;
            }
        }
        private bool _allowStretch;
        /// <summary>
        /// Gibt an, ob beim Berechnen die Struktur an das Bild angepasst werden darf.
        /// </summary>
        [ProtoMember(2)]
        public bool AllowStretch
        {
            get
            {
                return _allowStretch;
            }

            set
            {
                _allowStretch = value;
                colorsValid = false;
            }
        }
        #endregion
        #region constructors
        public NormalReadout(IDominoProvider parent, string relativeImagePath, AverageMode average, bool allowStretch) : base(relativeImagePath, parent)
        {
            Average = average;
            AllowStretch = allowStretch;
        }
        public NormalReadout(IDominoProvider parent, int width, int height, AverageMode average, bool allowStretch) : base(width, height, parent)
        {
            Average = average;
            AllowStretch = allowStretch;
        }
        #endregion
        #region overrides
        public override void ReadoutColors(DominoTransfer shapes)
        {
            if (shapes.Length == 0)
                return;
            var img = FilteredImage;
            double scalingX = (double)(Width - 1) / shapes.PhysicalLength;
            double scalingY = (double)(Height - 1) / shapes.PhysicalHeight;
            if (!AllowStretch)
            {
                if (scalingX > scalingY) scalingX = scalingY;
                else scalingY = scalingX;
            }
            // tatsächlich genutzte Farben auslesen
            Parallel.For(0, shapes.Length, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, (i) =>
            {
                SKColor result = new SKColor();
                if (Average == AverageMode.Corner)
                {
                    DominoRectangle container = shapes[i].GetContainer(scalingX, scalingY);

                    result = img.GetPixel(container.x1, container.y1);
                }
                else if (Average == AverageMode.Average)
                {
                    DominoRectangle container = shapes[i].GetContainer(scalingX, scalingY);
                    double R = 0, G = 0, B = 0, A = 0;
                    int counter = 0;

                    // for each container
                    for (int x_iterator = container.x1; x_iterator <= container.x2; x_iterator++)
                    {
                        for (int y_iterator = container.y1; y_iterator <= container.y2; y_iterator++)
                        {
                            if (shapes[i].IsInside(new Point(x_iterator, y_iterator), scalingX, scalingY))
                            {
                                var r = img.GetPixel(x_iterator, y_iterator);
                                R += r.Red;
                                G += r.Green;
                                B += r.Blue;
                                A += r.Alpha;
                                counter++;
                            }
                        }
                    }
                    if (counter != 0)
                    {
                        result = new SKColor((byte)(R / counter), (byte)(G / counter), (byte)(B / counter),   (byte)(A / counter));
                    }
                    else // rectangle too small
                    {
                        result = img.GetPixel(container.y1, container.x1);
                    }
                }
                if (StateReference == StateReference.Before)
                {
                    shapes[i].PrimaryOriginalColor = result;
                }
                else
                {
                    throw new NotImplementedException();
                }
            });
            
            colorsValid = true;
        }
        #endregion
        

    }
    public enum Inter
    {
        Nearest = 0,
        Linear = 1,
        LinearExact = 1,
        Cubic = 2,
        Area = 3,
        Lanczos4 = 3,
        Unset=5
    }
    [ProtoContract(SkipConstructor =true)]
    public class FieldReadout : ImageTreatment
    {
        #region properties
        private Inter _resizeMode;
        /// <summary>
        /// Gibt an, mit welcher Genauigkeit das Bild verkleinert werden soll.
        /// Bicubic eignet sich für Fotos, NearestNeighbor für Logos
        /// </summary>
        [ProtoMember(1)]
        private Inter ResizeMode
        {
            get => _resizeMode;
            set
            {
                _resizeMode = value;
                if (value == Inter.Nearest )
                    ResizeQuality = SKFilterQuality.Low;
                else if (value == Inter.Linear || value == Inter.LinearExact || value == Inter.Cubic)
                    ResizeQuality = SKFilterQuality.Medium;
                else
                    ResizeQuality = SKFilterQuality.High;
                _resizeMode = Inter.Unset;
            }
        }
        private SKFilterQuality _resizeQuality;
        [ProtoMember(2)]
        public SKFilterQuality ResizeQuality
        {
            get => _resizeQuality;
            set
            {
                if (_resizeQuality != value)
                {
                    _resizeQuality = value;
                    colorsValid = false;
                }
            }
        }
        #endregion
        private SKBitmap resizedImage;

        #region constructors
        public FieldReadout(FieldParameters parent, string relativeImagePath, SKFilterQuality resizeQuality) : base(relativeImagePath, parent)
        {
            ResizeQuality = resizeQuality;
        }
        public FieldReadout(FieldParameters parent, int imageWidth, int imageHeight, SKFilterQuality resizeQuality) : base(imageWidth, imageHeight, parent)
        {
            ResizeQuality = resizeQuality;
        }
        #endregion
        #region overrides
        public override void ReadoutColors(DominoTransfer shapes)
        {
            int length = shapes.FieldPlanLength;
            int height = shapes.FieldPlanHeight;
            resizedImage = imageFiltered.Resize(new SKImageInfo(length, height), ResizeQuality);
            using (var image = resizedImage)
            {
                Parallel.For(0, length, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (xi) =>
                {
                    for (int yi = 0; yi < height; yi++)
                    {
                        if (StateReference == StateReference.Before)
                        {
                            shapes[length * yi + xi].PrimaryOriginalColor = image.GetPixel(xi, yi);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                });
            }
            colorsValid = true;
        }
        #endregion

    }
}

