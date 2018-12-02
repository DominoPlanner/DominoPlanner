using System;
using System.Windows.Media;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using ProtoBuf;
using System.IO;
using System.Reflection;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Stellt die Methoden und Eigenschaften zum Erstellen und Bearbeiten eines Feldes zur Verfügung.
    /// </summary>
    [ProtoContract]
    public class FieldParameters : IDominoProvider
    {
        #region public properties
        public override int targetCount
        {
            set
            {
                double tempwidth = Math.Sqrt(((double)source.Height * (a + b) * value * (c + d) * source.Width)) / (source.Height * (a + b));
                double tempheight = Math.Sqrt(((double)source.Height * (a + b) * value * (c + d) * source.Width)) / (source.Width * (c + d));
                if (tempwidth < tempheight)
                {
                    length = (int)Math.Round(tempwidth);
                    height = (int)(value / (double)length);
                }
                else
                {
                    height = (int)Math.Round(tempheight);
                    length = (int)(value / (double)height);
                }
            }
        }
        
        private int _a;
        /// <summary>
        /// Der horizontale Abstand zwischen zwei Reihen/Steinen.
        /// </summary>
        [ProtoMember(2)]
        public int a
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
                resizedValid = false;
                shapesValid = false;
                lastValid = false;
            }
        }

        private int _b;
        /// <summary>
        /// Die horizontale Breite der Steine.
        /// </summary>
        [ProtoMember(3)]
        public int b
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
                resizedValid = false;
                shapesValid = false;
                lastValid = false;
            }
        }

        private int _c;
        /// <summary>
        /// Die vertikale Breite der Steine.
        /// </summary>
        [ProtoMember(4)]
        public int c
        {
            get
            {
                return _c;
            }
            set
            {
                _c = value;
                resizedValid = false;
                shapesValid = false;
                lastValid = false;
            }
        }

        private int _d;
        /// <summary>
        /// Der vertikale Abstand zwischen zwei Steinen/Reihen.
        /// </summary>
        [ProtoMember(5)]
        public int d
        {
            get
            {
                return _d;
            }
            set
            {
                _d = value;
                resizedValid = false;
                shapesValid = false;
                lastValid = false;
            }
        }
    
        private Inter _resizeMode;
        /// <summary>
        /// Gibt an, mit welcher Genauigkeit das Bild verkleinert werden soll.
        /// Bicubic eignet sich für Fotos, NearestNeighbor für Logos
        /// </summary>
        [ProtoMember(6)]
        public Inter resizeMode
        {
            get
            {
                return _resizeMode;
            }
            set
            {
                _resizeMode = value;
                shapesValid = false;
                resizedValid = false;
                lastValid = false;
            }
        }
        private Dithering.Dithering _ditherMode;
        /// <summary>
        /// Gibt an, ob ein Fehlerkorrekturalgorithmus verwendet werden soll.
        /// </summary>
        public Dithering.Dithering ditherMode
        {
            get
            {
                return _ditherMode;
            }
            set
            {
                _ditherMode = value;
                lastValid = false;
            }
        }
        [ProtoMember(7)]
        private string DitheringSurrogate
        {
            get
            {
                return (_ditherMode.GetType().Name);
            }
            set
            {
                _ditherMode = (Dithering.Dithering) Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.Dithering.{value}"));
            }
        }
        private int _length;
        /// <summary>
        /// Die horizontale Steineanzahl.
        /// </summary>
        [ProtoMember(8)]
        public int length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
                shapesValid = false;
                resizedValid = false;
                lastValid = false;
            }
        }
        private int _height;
        /// <summary>
        /// Die vertikale Steineanzahl.
        /// </summary>
        [ProtoMember(9)]
        public int height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                shapesValid = false;
                resizedValid = false;
                lastValid = false;
            }
        }
        public HistoryTree<FieldParameters> history { get; set; }
        public HistoryTree<FieldParameters> current;
        #endregion
        #region private properties
        private Mat resizedImage;
        private IDominoShape[] shapes;
        [ProtoMember(1000)]
        private bool resizedValid = false;
        #endregion
        #region public constructors
        /// <summary>
        /// Erstellt ein FieldParameters-Objekt mit der angegebenen Länge und Breite.
        /// </summary>
        /// <param name="bitmap">Das Bitmap, welchem dem Feld zugrunde liegen soll.</param>
        /// <param name="colors">Die Farben, die für dieses Objekt verwendet werden sollen.</param>
        /// <param name="a">Der horizontale Abstand zwischen zwei Spalten/Steinen.</param>
        /// <param name="b">Die horizonale Breite der Steine.</param>
        /// <param name="c">Die vertikale Breite der Steine.</param>
        /// <param name="d">Der vertikale Abstand zwischen zwei Reihen/Steinen.</param>
        /// <param name="width">Die Anzahl der Steine in horizonaler Richtung.</param>
        /// <param name="height">Die Anzahl der Steine in vertikaler Richtung.</param>
        /// <param name="scalingMode">Gibt an, mit welcher Genauigkeit das Bild verkleinert werden soll.
        /// Eine niedrige Genauigkeit eignet sich v.a. bei Logos.</param>
        /// <param name="ditherMode">Gibt an, ob ein Fehlerkorrekturalgorithmus verwendet werden soll.</param>
        /// <param name="interpolationMode">Der Interpolationsmodus, der zur Farberkennung berechnet wird.</param>
        /// <param name="useOnlyMyColors">Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.
        /// Hat keine Wirkung, wenn ein Fehlerkorrekturalgorithmus verwendet werden soll.</param>
        public FieldParameters(string imagePath, string colors, int a, int b, int c, int d, int width, int height, 
            Inter scalingMode, Dithering.Dithering ditherMode, IColorComparison colormode, IterationInformation iterationInformation) 
            : base(imagePath, colormode, colors, iterationInformation)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.length = width;
            this.height = height;
            this.resizeMode = scalingMode;
            this.ditherMode = ditherMode;
            hasProcotolDefinition = true;
            //this.history = new EmptyOperation<FieldParameters>(this);
            //current = history;
        }
        /// <summary>
        /// Erzeugt ein Feld, dessen Steineanzahl möglichst nahe an einem bestimmten Wert liegt.
        /// Es wird versucht, das Seitenverhältnis des Quellbildes möglichst zu wahren.
        /// </summary>
        /// <param name="bitmap">Das Bitmap, welchem dem Feld zugrunde liegen soll.</param>
        /// <param name="colors">Die Farben, die für dieses Objekt verwendet werden sollen.</param>
        /// <param name="a">Der horizontale Abstand zwischen zwei Spalten/Steinen.</param>
        /// <param name="b">Die horizonale Breite der Steine.</param>
        /// <param name="c">Die vertikale Breite der Steine.</param>
        /// <param name="d">Der vertikale Abstand zwischen zwei Reihen/Steinen.</param>
        /// <param name="scalingMode">Gibt an, mit welcher Genauigkeit das Bild verkleinert werden soll.
        /// Eine niedrige Genauigkeit eignet sich v.a. bei Logos.</param>
        /// <param name="ditherMode">Gibt an, ob ein Fehlerkorrekturalgorithmus verwendet werden soll.</param>
        /// <param name="interpolationMode">Der Interpolationsmodus, der zur Farberkennung verwendet wird.</param>
        /// <param name="useOnlyMyColors">Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.
        /// Hat keine Wirkung, wenn ein Fehlerkorrekturalgorithmus verwendet werden soll.</param>
        /// <param name="targetSize">Gibt die Zielgröße des Feldes an.
        /// Dabei wird versucht, das Seitenverhältnis des Quellbildes möglichst zu wahren.</param>
        public FieldParameters(String imagePath, string colors, int a, int b, int c, int d, int targetSize, 
            Inter scalingMode, Dithering.Dithering ditherMode, IColorComparison interpolationMode, IterationInformation iterationInformation) 
            : this(imagePath, colors, a, b, c, d, 1, 1, scalingMode, ditherMode, interpolationMode, iterationInformation)
        {
            targetCount = targetSize;
        }
        private FieldParameters() : base() { }
        #endregion
        #region override methods
        /// <summary>
        /// Generiert das Feld.
        /// Die Methode erkennt automatisch, welche Teile des DominoTransfers regeneriert werden müssen.
        /// </summary>
        /// <param name="progressIndicator">Kann für Threading verwendet werden.</param>
        /// <returns>Einen DominoTransfer, der alle Informationen über das fertige Feld erhält.</returns>
        public override DominoTransfer Generate(IProgress<string> progressIndicator = null)
        {
            if (!sourceValid)
            {
                if (progressIndicator != null) progressIndicator.Report("Updating source image");
                UpdateSource();
            }
            if (!colorsValid)
            {
                if (progressIndicator != null) progressIndicator.Report("Updating Color filters");
                ApplyColorFilters();
            }
            if (!imageValid)
            {
                if (progressIndicator != null) progressIndicator.Report("Applying image filters");
                ApplyImageFilters();
                image_filtered.Save("tests/field_filtered.png");
                resizedValid = false;
            }
            if (!resizedValid)
            {
                image_filtered.Save("tests/field_filtered_inside_resize.png");
                if (progressIndicator != null) progressIndicator.Report("Resizing Image");
                ResizeImage();
            }
            if (!lastValid)
            {
                if (progressIndicator != null) progressIndicator.Report("Calculating ideal domino colors");
                GetDominoes();
            }
            return last;
        }
        #endregion
        #region private helper methods
        /// <summary>
        /// Verkleinert das Bild mit der spezifizierten Genauigkeit und auf die spezifizierte Größe.
        /// </summary>
        /// <param name="image">Das zu verkleinernde Bild.</param>
        [ProtoAfterDeserialization]
        private void ResizeImage()
        {
            if (length < 2) length = 2;
            if (height < 2) height = 2;
            resizedImage = new Mat();
            image_filtered.Save("tests/image_filtered.png");
            CvInvoke.Resize(image_filtered, resizedImage, 
                new System.Drawing.Size() { Height = height, Width=length}, interpolation: resizeMode);
            image_filtered.Save("tests/resized.png");
            resizedValid = true;
            if (!shapesValid) GenerateShapes();
            if (shapes == null) restoreShapes();
        }
        /// <summary>
        /// Berechnet die Shapes mit den angegebenen Parametern.
        /// </summary>
        public void GenerateShapes()
        {
            IDominoShape[] array = new IDominoShape[length*height];
            Parallel.For(0, length, new ParallelOptions { MaxDegreeOfParallelism= -1}, (xi) =>
            {
                for (int yi = 0; yi < height; yi++)
                {
                    RectangleDomino shape = new RectangleDomino()
                    {
                        x = (b + a) * xi,
                        y = (c + d) * yi,
                        width = b,
                        height = c,
                        position = new ProtocolDefinition() { x = xi, y = yi }
                    };
                    array[height * xi + yi] = shape;
                }
            });
            shapes = array;
            shapesValid = true;
        }
        public void restoreShapes()
        {
            //bool last_valid_temp = lastValid;
            shapes = last.shapes;
            ResizeImage();
            //lastValid = last_valid_temp;
        }
        /// <summary>
        /// Berechnet aus dem Shape-Array die Farben.
        /// </summary>
        private void GetDominoes()
        {
            // apply filters to color list
            //foreach (PreFilter f in PreFilters) f.Apply(colors);
            // remove transparency
            Console.WriteLine("Debug Flag");
            var colors = this.color_filtered.RepresentionForCalculation;
            IterationInformation.weights = Enumerable.Repeat(1.0, colors.Length).ToArray();
            /*if (IterationInformation is IterativeColorRestriction)
            {
                if (colors.Sum(color => ) < resizedImage.Width * resizedImage.Height)
                    throw new InvalidOperationException("Gesamtsteineanzahl ist größer als vorhandene Anzahl, kann nicht konvergieren");
            }*/
            int[] field = new int[resizedImage.Width * resizedImage.Height];
            using (Image<Emgu.CV.Structure.Bgra, Byte> bitmap = resizedImage.ToImage<Emgu.CV.Structure.Bgra, Byte>())
            {
                bitmap.Save("tests/field_filtered_bitmap.png");
                // tatsächlich genutzte Farben auslesen
                for (int iter = 0; iter < IterationInformation.maxNumberOfIterations; iter++)
                {
                    IterationInformation.numberofiterations = iter;
                    Console.WriteLine($"Iteration {iter}");
                    Parallel.For(0, resizedImage.Width, new ParallelOptions() { MaxDegreeOfParallelism = ditherMode.maxDegreeOfParallelism }, (x) =>
                    {
                        for (int y = resizedImage.Height - 1; y >= 0; y--)
                        {
                            Emgu.CV.Structure.Bgra bgra = bitmap[y, x];
                            
                            int Minimum = 0;
                            double min = Int32.MaxValue;
                            double temp = Int32.MaxValue;
                            for (int z = colors.Length - 1; z >= 0; z--)
                            {
                                temp = colors[z].distance(bgra, colorMode, TransparencySetting) * IterationInformation.weights[z];
                                if (min > temp)
                                {
                                    min = temp;
                                    Minimum = z;
                                }
                            }
                            Color newpixel = colors[Minimum].mediaColor;
                            field[resizedImage.Height * x + y] = Minimum;
                            ditherMode.DiffuseError(
                                x, y, (int)(bgra.Red) - newpixel.R, (int)(bgra.Green) - newpixel.G, (int)(bgra.Blue) - newpixel.B, bitmap);
                        }
                    });
                    IterationInformation.EvaluateSolution(colors, field);
                    if (IterationInformation.colorRestrictionsFulfilled != false) break;
                }
            }
            last = new DominoTransfer(field, shapes, this.colors);
            lastValid = true;
        }
        /// <summary>
        /// Berechnet das Basisfeld für ein Feldprotokoll
        /// </summary>
        /// <param name="o">Gibt an, ob das Feld gedreht sein soll.</param>
        /// <returns>Das Basisfeld als int[,]-Array.</returns>
        public override int[,] GetBaseField(Orientation o = Orientation.Horizontal)
        {
            if (!shapesValid || !resizedValid) throw new InvalidOperationException("There are unreflected changes in this field.");
            int[,] result = new int[length, height];
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        result[i, j] = last.dominoes[i*height + j];
                    }
                }
            if (o == Orientation.Vertical) result = TransposeArray(result);
            return result;
        }

        public override object Clone()
        {
            FieldParameters res = (FieldParameters)this.MemberwiseClone();
            //res.source = source.Clone();
            res.resizedImage = resizedImage?.Clone();
            // History-Objekt soll immer gleich bleiben. Keinesfalls klonen. 
            res.last = (DominoTransfer) last?.Clone();
            return res;
        }
        private FieldParameters(String imagePath, String colors, IColorComparison colorMode, IterationInformation iterationInformation)
            : base(imagePath, colorMode, colors, iterationInformation)
        {

        }
        
        #endregion
    }
}
