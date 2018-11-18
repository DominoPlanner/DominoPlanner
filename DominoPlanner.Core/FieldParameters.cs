﻿using System;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Windows.Threading;
using ColorMine.ColorSpaces.Comparisons;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Stellt die Methoden und Eigenschaften zum Erstellen und Bearbeiten eines Feldes zur Verfügung.
    /// </summary>
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
        public int a
        {
            get
            {
                return _a;
            }
            set
            {
                _a = value;
                imageValid = false;
                shapesValid = false;
                lastValid = false;
            }
        }

        private int _b;
        /// <summary>
        /// Die horizontale Breite der Steine.
        /// </summary>
        public int b
        {
            get
            {
                return _b;
            }
            set
            {
                _b = value;
                imageValid = false;
                shapesValid = false;
                lastValid = false;
            }
        }

        private int _c;
        /// <summary>
        /// Die vertikale Breite der Steine.
        /// </summary>
        public int c
        {
            get
            {
                return _c;
            }
            set
            {
                _c = value;
                imageValid = false;
                shapesValid = false;
                lastValid = false;
            }
        }

        private int _d;
        /// <summary>
        /// Der vertikale Abstand zwischen zwei Steinen/Reihen.
        /// </summary>
        public int d
        {
            get
            {
                return _d;
            }
            set
            {
                _d = value;
                imageValid = false;
                shapesValid = false;
                lastValid = false;
            }
        }

        private Inter _resizeMode;
        /// <summary>
        /// Gibt an, mit welcher Genauigkeit das Bild verkleinert werden soll.
        /// Bicubic eignet sich für Fotos, NearestNeighbor für Logos
        /// </summary>
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
                imageValid = false;
                lastValid = false;
            }
        }
        private DitherMode _ditherMode;
        /// <summary>
        /// Gibt an, ob ein Fehlerkorrekturalgorithmus verwendet werden soll.
        /// </summary>
        public DitherMode ditherMode
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
        private int _length;
        /// <summary>
        /// Die horizontale Steineanzahl.
        /// </summary>
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
                imageValid = false;
                lastValid = false;
            }
        }
        private int _height;
        /// <summary>
        /// Die vertikale Steineanzahl.
        /// </summary>
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
                imageValid = false;
                lastValid = false;
            }
        }
        IterationInformation _iterationInfo;
        public override IterationInformation IterationInformation
        {
            get
            {
                return _iterationInfo;
            }
            set
            {
                _iterationInfo = value;
                _iterationInfo.PropertyChanged +=
                    new PropertyChangedEventHandler(delegate (object s, PropertyChangedEventArgs e) { lastValid = false; });
                lastValid = false;
            }
        }
        public HistoryTree<FieldParameters> history { get; set; }
        public HistoryTree<FieldParameters> current;
        #endregion
        #region private properties
        private Mat resizedImage;
        private IDominoShape[] shapes;
        private bool imageValid = false;
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
        public FieldParameters(Mat bitmap, List<DominoColor> colors, int a, int b, int c, int d, int width, int height, 
            Inter scalingMode, DitherMode ditherMode, IColorSpaceComparison colormode, IterationInformation iterationInformation) 
            : base(bitmap, colormode, colors, iterationInformation)
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
            this.history = new EmptyOperation<FieldParameters>(this);
            current = history;
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
        public FieldParameters(Mat bitmap, List<DominoColor> colors, int a, int b, int c, int d, int targetSize, 
            Inter scalingMode, DitherMode ditherMode, IColorSpaceComparison interpolationMode, IterationInformation iterationInformation) 
            : this(bitmap, colors, a, b, c, d, 1, 1, scalingMode, ditherMode, interpolationMode, iterationInformation)
        {
            targetCount = targetSize;
        }
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
            if (!imageValid)
            {
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
        private void ResizeImage()
        {
            if (length < 2) length = 2;
            if (height < 2) height = 2;
            resizedImage = new Mat();
            CvInvoke.Resize(source, resizedImage, new System.Drawing.Size() { Height = height, Width=length}, interpolation: resizeMode);
            imageValid = true;
            if (!shapesValid) GenerateShapes();
            //shapes = new IDominoShape[height*length];
        }
        /// <summary>
        /// Berechnet die Shapes mit den angegebenen Parametern.
        /// </summary>
        public void GenerateShapes()
        {
            IDominoShape[] array = new IDominoShape[length*height];
            Parallel.For(0, length, new ParallelOptions { MaxDegreeOfParallelism=1}, (xi) =>
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
        /// <summary>
        /// Berechnet aus dem Shape-Array die Farben.
        /// </summary>
        private void GetDominoes()
        {
            // apply filters to color list
            //foreach (PreFilter f in PreFilters) f.Apply(colors);
            // remove transparency
            Dithering.BasicDithering d;
            switch(ditherMode)
            {
                // todo: rewrite Ditherings
                case DitherMode.NoDithering: d = new Dithering.BasicDithering(colorMode, colors, IterationInformation); break;
                case DitherMode.FloydSteinberg: d = new Dithering.FloydSteinbergDithering(colorMode, colors, IterationInformation); break;
                case DitherMode.JarvisJudiceNinke: d = new Dithering.JarvisJudiceNinkeDithering(colorMode, colors, IterationInformation); break;
                default: d = new Dithering.StuckiDithering(colorMode, colors, IterationInformation); break;
            }
            last = new DominoTransfer(d.Dither(resizedImage), shapes, colors);
            lastValid = true;
        }
        /// <summary>
        /// Berechnet das Basisfeld für ein Feldprotokoll
        /// </summary>
        /// <param name="o">Gibt an, ob das Feld gedreht sein soll.</param>
        /// <returns>Das Basisfeld als int[,]-Array.</returns>
        public override int[,] GetBaseField(Orientation o = Orientation.Horizontal)
        {
            if (!shapesValid || !imageValid) throw new InvalidOperationException("There are unreflected changes in this field.");
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
            FieldParameters res = ObjectExtensions.Copy(this);
            res.history = this.history; // History-Objekt soll immer gleich bleiben
            res.current = this.current;
            return res;
        }
        #endregion
    }
}
