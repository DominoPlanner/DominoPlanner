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
using Emgu.CV.Structure;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Stellt die Methoden und Eigenschaften zum Erstellen und Bearbeiten eines Feldes zur Verfügung.
    /// </summary>
    [ProtoContract]
    public class FieldParameters : IDominoProvider, ICountTargetable
    {
        #region public properties
        public int TargetCount
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
                shapesValid = false;
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
                shapesValid = false;
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
                shapesValid = false;
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
                shapesValid = false;
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
                usedColorsValid = false;
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
            }
        }
        public HistoryTree<FieldParameters> history { get; set; }
        public HistoryTree<FieldParameters> current;
        #endregion
        #region private properties
        private Mat resizedImage;
        private IDominoShape[] shapes;
        #endregion
        #region public constructors
        /// <summary>
        /// Erstellt ein FieldParameters-Objekt mit der angegebenen Länge und Breite.
        /// </summary>
        /// <param name="imagePath">Der (relative) Pfad zum Quellbild</param>
        /// <param name="colors">Der (relative) Pfad zur Farbendatei</param>
        /// <param name="a">Der horizontale Abstand zwischen zwei Spalten/Steinen.</param>
        /// <param name="b">Die horizonale Breite der Steine.</param>
        /// <param name="c">Die vertikale Breite der Steine.</param>
        /// <param name="d">Der vertikale Abstand zwischen zwei Reihen/Steinen.</param>
        /// <param name="width">Die Anzahl der Steine in horizonaler Richtung.</param>
        /// <param name="height">Die Anzahl der Steine in vertikaler Richtung.</param>
        /// <param name="scalingMode">Gibt an, mit welcher Genauigkeit das Bild verkleinert werden soll.
        /// Eine niedrige Genauigkeit eignet sich v.a. bei Logos.</param>
        /// <param name="ditherMode">Gibt an, ob ein Fehlerkorrekturalgorithmus verwendet werden soll.</param>
        /// <param name="colorMode">Der Interpolationsmodus, der zur Farberkennung berechnet wird.</param>
        /// <param name="iterationInformation">Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.
        /// Hat keine Wirkung, wenn ein Fehlerkorrekturalgorithmus verwendet werden soll.</param>
        public FieldParameters(string imagePath, string colors, int a, int b, int c, int d, int width, int height, 
            Inter scalingMode,IColorComparison colorMode, Dithering ditherMode, IterationInformation iterationInformation) 
            : base(imagePath, colorMode, ditherMode, colors, iterationInformation)
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
        /// <param name="imagePath">Der (relative) Pfad zum Quellbild</param>
        /// <param name="colors">Der (relative) Pfad zur Farbendatei</param>
        /// <param name="a">Der horizontale Abstand zwischen zwei Spalten/Steinen.</param>
        /// <param name="b">Die horizonale Breite der Steine.</param>
        /// <param name="c">Die vertikale Breite der Steine.</param>
        /// <param name="d">Der vertikale Abstand zwischen zwei Reihen/Steinen.</param>
        /// <param name="width">Die Anzahl der Steine in horizonaler Richtung.</param>
        /// <param name="height">Die Anzahl der Steine in vertikaler Richtung.</param>
        /// <param name="scalingMode">Gibt an, mit welcher Genauigkeit das Bild verkleinert werden soll.
        /// Eine niedrige Genauigkeit eignet sich v.a. bei Logos.</param>
        /// <param name="ditherMode">Gibt an, ob ein Fehlerkorrekturalgorithmus verwendet werden soll.</param>
        /// <param name="colorMode">Der Interpolationsmodus, der zur Farberkennung berechnet wird.</param>
        /// <param name="iterationInformation">Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.
        /// Hat keine Wirkung, wenn ein Fehlerkorrekturalgorithmus verwendet werden soll.</param>
        /// <param name="targetSize">Gibt die Zielgröße des Feldes an.
        /// Dabei wird versucht, das Seitenverhältnis des Quellbildes möglichst zu wahren.</param>
        public FieldParameters(String imagePath, string colors, int a, int b, int c, int d, int targetSize, 
            Inter scalingMode,  IColorComparison interpolationMode, Dithering ditherMode, IterationInformation iterationInformation) 
            : this(imagePath, colors, a, b, c, d, 1, 1, scalingMode,  interpolationMode, ditherMode, iterationInformation)
        {
            TargetCount = targetSize;
        }
        public FieldParameters(int imageWidth, int imageHeight, Color background, string colors, int a, int b, int c, int d, int targetSize,
            Inter scalingMode, IColorComparison interpolationMode, Dithering ditherMode, IterationInformation iterationInformation)
            : base(imageWidth, imageHeight, background, interpolationMode, ditherMode, colors, iterationInformation)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;
            this.resizeMode = scalingMode;
            this.ditherMode = ditherMode;
            hasProcotolDefinition = true;
            TargetCount = targetSize;
        }
        private FieldParameters() : base() { }
        #endregion
        #region override methods
        internal override void ReadUsedColors()
        {
            if (!shapesValid) throw new InvalidOperationException("erst die Shapes aktualisieren");
            if (length < 2) length = 2;
            if (height < 2) height = 2;
            resizedImage = new Mat();
            CvInvoke.Resize(image_filtered, resizedImage,
                new System.Drawing.Size() { Height = height, Width = length }, interpolation: resizeMode);
            using (var image = resizedImage.ToImage<Bgra, byte>())
            {
                Parallel.For(0, length, new ParallelOptions { MaxDegreeOfParallelism = 1 }, (xi) =>
                {
                    for (int yi = 0; yi < height; yi++)
                    {
                        usedColorsValid = true;
                        if (shapes == null) restoreShapes();
                        shapes[length * yi + xi].originalColor = image[yi, xi];
                    }
                });
            }
            
        }
        internal override void GenerateShapes()
        {
            IDominoShape[] array = new IDominoShape[length * height];

            Parallel.For(0, length, new ParallelOptions { MaxDegreeOfParallelism = -1 }, (xi) =>
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
                    array[length * yi + xi] = shape;
                }
            });
            shapes = array;
            shapesValid = true;
            usedColorsValid = false;
        }
        #endregion
        #region private helper methods
        /// <summary>
        /// Verkleinert das Bild mit der spezifizierten Genauigkeit und auf die spezifizierte Größe.
        /// </summary>
        /// <param name="image">Das zu verkleinernde Bild.</param>
        [ProtoAfterDeserialization]
        
        /// <summary>
        /// Berechnet die Shapes mit den angegebenen Parametern.
        /// </summary>
        
        public void restoreShapes()
        {
            //bool last_valid_temp = lastValid;
            shapes = last.shapes;
            ReadUsedColors();
            //lastValid = last_valid_temp;
        }
        /// <summary>
        /// Berechnet aus dem Shape-Array die Farben.
        /// </summary>
        internal override void CalculateColors()
        {
            var colors = this.color_filtered.RepresentionForCalculation;
            IterationInformation.weights = Enumerable.Repeat(1.0, colors.Length).ToArray();
            
            for (int iter = 0; iter < IterationInformation.maxNumberOfIterations; iter++)
            {
                ResetDitherColors(shapes);
                IterationInformation.numberofiterations = iter;
                Console.WriteLine($"Iteration {iter}");
                Parallel.For(0, height, new ParallelOptions() { MaxDegreeOfParallelism = ditherMode.maxDegreeOfParallelism }, (y) =>
                {
                    for (int x = 0; x < length; x++)
                    {
                        shapes[length * y + x].CalculateColor(colors, colorMode, TransparencySetting, IterationInformation.weights);
                        int error_r = (int)(shapes[length * y + x].ditherColor.Red - colors[shapes[length * y + x].color].mediaColor.R);
                        int error_g = (int)(shapes[length * y + x].ditherColor.Green - colors[shapes[length * y + x].color].mediaColor.G);
                        int error_b = (int)(shapes[length * y + x].ditherColor.Blue - colors[shapes[length * y + x].color].mediaColor.B);
                        DiffuseErrorField(
                            x, y,(int) (error_r), (int)(error_g), (int)(error_b),
                            shapes, height);
                    }
                });
                IterationInformation.EvaluateSolution(colors, shapes);
                if (IterationInformation.colorRestrictionsFulfilled != false) break;
            }
            last = new DominoTransfer(shapes, this.colors);
            lastValid = true;
            
        }
        
        public virtual void DiffuseErrorField(int x, int y, int v1, int v2, int v3, IDominoShape[] shapes, int height)
        {
            for (int j = 0; j < ditherMode.weights.GetLength(0); j++)
            {
                for (int i = ditherMode.startindizes[j]; i < ditherMode.weights.GetLength(1); i++)
                {
                    int akt_x = x + i - ditherMode.startindizes[0] + 1;
                    int akt_y = y + j;
                    if (akt_x >= length) continue; //akt_x = length - 1;
                    if (akt_x < 0) continue; //akt_x = 0;
                    if (akt_y < 0) continue; // akt_y = 0;
                    if (akt_y >= height) continue; // akt_y = height - 1;
                    ditherMode.AddToPixel(shapes[akt_y * length + akt_x],
                       v1 * ditherMode.weights[j, i] / ditherMode.divisor,
                       v2 *ditherMode.weights[j, i] / ditherMode.divisor,
                        v3 * ditherMode.weights[j, i] / ditherMode.divisor);
                }
            }
        }
        /// <summary>
        /// Berechnet das Basisfeld für ein Feldprotokoll
        /// </summary>
        /// <param name="o">Gibt an, ob das Feld gedreht sein soll.</param>
        /// <returns>Das Basisfeld als int[,]-Array.</returns>
        public override int[,] GetBaseField(Orientation o = Orientation.Horizontal)
        {
            if (!shapesValid || !usedColorsValid) throw new InvalidOperationException("There are unreflected changes in this field.");
            int[,] result = new int[length, height];
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        result[i, j] = last.shapes[i*height + j].color;
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
        
        #endregion
    }
}
