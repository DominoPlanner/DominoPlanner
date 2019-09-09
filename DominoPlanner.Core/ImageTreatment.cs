using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DominoPlanner.Core
{
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(100, typeof(NormalReadout))]
    [ProtoInclude(101, typeof(FieldReadout))]
    public abstract class ImageTreatment
    {
        private Mat source;
        protected Mat imageFiltered;
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
        private string BackgroundSurrogate
        {
            get
            {
                return _background.ToString();
            }
            set { Background = (Color)ColorConverter.ConvertFromString(value); }
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
            this.source = new Image<Emgu.CV.Structure.Bgra, byte>(Width, Height,
                new Emgu.CV.Structure.Bgra(Background.B, Background.G, Background.R, Background.A)).Mat;
            imageValid = false;
        }
        protected void ApplyImageFilters()
        {
            var imageFiltered = source.ToImage<Emgu.CV.Structure.Bgra, byte>();
            foreach (ImageFilter filter in ImageFilters)
            {
                filter.parent = parent;
                filter.Apply(imageFiltered);
            }
            this.imageFiltered = imageFiltered.Mat;
            imageValid = true;
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

        public System.Drawing.Bitmap FilteredImage
        {
            get
            {
                if(imageFiltered != null)
                {
                    return imageFiltered.Bitmap;
                }
                return null;
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
            using (Image<Bgra, byte> img = imageFiltered.ToImage<Bgra, byte>())
            {
                double scalingX = (double)(Width - 1) / shapes.physicalLength;
                double scalingY = (double)(Height - 1) / shapes.physicalHeight;
                if (!AllowStretch)
                {
                    if (scalingX > scalingY) scalingX = scalingY;
                    else scalingY = scalingX;
                }
                // tatsächlich genutzte Farben auslesen
                Parallel.For(0, shapes.length, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, (i) =>
                {
                    Bgra result = new Bgra();
                    if (Average == AverageMode.Corner)
                    {
                        DominoRectangle container = shapes[i].GetContainer(scalingX, scalingY);

                        result = new Bgra(img.Data[container.y1, container.x1, 0], img.Data[container.y1, container.x1, 1],
                            img.Data[container.y1, container.x1, 2], img.Data[container.y1, container.x1, 3]);
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
                                    R += img.Data[y_iterator, x_iterator, 2];
                                    G += img.Data[y_iterator, x_iterator, 1];
                                    B += img.Data[y_iterator, x_iterator, 0];
                                    A += img.Data[y_iterator, x_iterator, 3];
                                    counter++;
                                }
                            }
                        }
                        if (counter != 0)
                        {
                            result = new Bgra((byte)(B / counter), (byte)(G / counter), (byte)(R / counter), (byte)(A / counter));
                        }
                        else // rectangle too small
                        {
                            result = new Bgra(img.Data[container.y1, container.x1, 0], img.Data[container.y1, container.x1, 1],
                            img.Data[container.y1, container.x1, 2], img.Data[container.y1, container.x1, 3]);
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
            }
            colorsValid = true;
        }
        #endregion
        

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
        public Inter ResizeMode
        {
            get => _resizeMode;
            set
            {
                if (_resizeMode != value)
                {
                    _resizeMode = value;
                    colorsValid = false;
                }
            }
        }
        #endregion
        private Mat resizedImage;

        #region constructors
        public FieldReadout(FieldParameters parent, string relativeImagePath, Inter resizeMode) : base(relativeImagePath, parent)
        {
            ResizeMode = resizeMode;
        }
        public FieldReadout(FieldParameters parent, int imageWidth, int imageHeight, Inter resizeMode) : base(imageWidth, imageHeight, parent)
        {
            ResizeMode = resizeMode;
        }
        #endregion
        #region overrides
        public override void ReadoutColors(DominoTransfer shapes)
        {
            int length = shapes.FieldPlanLength;
            int height = shapes.FieldPlanHeight;
            resizedImage = new Mat();
            CvInvoke.Resize(imageFiltered, resizedImage,
                new System.Drawing.Size() { Height = height, Width = length}, interpolation: ResizeMode);
            using (var image = resizedImage.ToImage<Bgra, byte>())
            {
                Parallel.For(0, length, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (xi) =>
                {
                    for (int yi = 0; yi < height; yi++)
                    {
                        if (StateReference == StateReference.Before)
                        {
                            shapes[length * yi + xi].PrimaryOriginalColor = image[yi, xi];
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

