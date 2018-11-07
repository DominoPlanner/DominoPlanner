using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using DominoPlanner.Core.Dithering;
using ColorMine.ColorSpaces;
using System.Windows;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Oberklasse für alle Strukturen, deren Farben aus beliebig angeordneten Rechtecken oder Pfaden berechnet werden.
    /// </summary>
    public abstract class RectangleDominoProvider : IDominoProvider
    {
        #region public properties
        public abstract override int targetCount { set; }
        private AverageMode _average;
        /// <summary>
        /// Gibt an, ob nur ein Punkt des Dominos (linke obere Ecke) oder ein Durchschnittswert aller Pixel unter dem Pfad verwendet werden soll, um die Farbe auszuwählen.
        /// </summary>
        public AverageMode average
        {
            get
            {
                return _average;
            }

            set
            {
                _average = value;
                lastValid = false;
            }
        }
        private bool _allowStretch;
        /// <summary>
        /// Gibt an, ob beim Berechnen die Struktur an das Bild angepasst werden darf.
        /// </summary>
        public bool allowStretch
        {
            get
            {
                return _allowStretch;
            }

            set
            {
                _allowStretch = value;
                lastValid = false;
            }
        }
        #endregion
        protected GenStructHelper shapes;
        #region constructors
        /// <summary>
        /// Erzeugt einen RectangleDominoProvider (Basiskonstruktor) mit den angegebenen Eigenschaften.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="colors"></param>
        /// <param name="comp"></param>
        /// <param name="useOnlyMyColors"></param>
        /// <param name="filter"></param>
        /// <param name="averageMode"></param>
        /// <param name="allowStretch"></param>
        protected RectangleDominoProvider(WriteableBitmap bitmap, List<DominoColor> colors, IColorSpaceComparison comp, bool useOnlyMyColors, AverageMode averageMode, bool allowStretch)
            : base(bitmap, useOnlyMyColors, comp, colors)
        {
            this.allowStretch = allowStretch;
            average = averageMode;
        }
        #endregion
        #region public methods
        /// <summary>
        /// Generiert das Objekt.
        /// Die Methode erkennt automatisch, welche Teile des DominoTransfers regeneriert werden müssen.
        /// </summary>
        /// <param name="progressIndicator">Kann für Threading verwendet werden.</param>
        /// <returns>Einen DominoTransfer, der alle Informationen über das fertige Objekt erhält.</returns>
        public override DominoTransfer Generate(IProgress<string> progressIndicator)
        {
            if (!shapesValid)
            {
                progressIndicator.Report("Calculating Domino Positions...");
                GenerateShapes();
            }
            if (!lastValid)
            {
                progressIndicator.Report("Calculating ideal colors...");
                last = new DominoTransfer(CalculateDominoes(), shapes.dominoes, colors);
                lastValid = true;
            }
            return last;
        }
        #endregion
        #region private helper methods
        /// <summary>
        /// Generiert die Shapes. Das Ergebnis wird in die Shapes-Variable geschrieben.
        /// </summary>
        protected abstract void GenerateShapes();
        /// <summary>
        /// Weist jedem Shape die ideale Farbe zu, basierend auf den festgelegten Eigenschaften.
        /// </summary>
        /// <returns>ein Int-Array mit den Farbindizes</returns>
        private int[] CalculateDominoes()
        {
            
            if (!shapesValid) throw new InvalidOperationException("Current shapes are invalid!");
            using (source.GetBitmapContext())
            {
                double scalingX = (source.PixelWidth - 1) / shapes.width;
                double scalingY = (source.PixelHeight - 1) / shapes.height;
                if (!allowStretch)
                {
                    if (scalingX > scalingY) scalingX = scalingY;
                    else scalingY = scalingX;
                }
                int[] dominoes = new int[shapes.dominoes.Length];

                for (int i = 0; i < shapes.dominoes.Length; i++)
                {
                    Lab c = new Lab();
                    if (average == AverageMode.Corner)
                    {
                        DominoRectangle container = shapes.dominoes[i].GetContainer(scalingX, scalingY);
                        c = source.GetPixel(container.x1, container.y1).ToLab();
                    }
                    else if (average == AverageMode.Average)
                    {
                        DominoRectangle container = shapes.dominoes[i].GetContainer(scalingX, scalingY);

                        int r = 0, g = 0, b = 0;
                        int counter = 0;
                        // for loop: each container
                        for (int x_iterator = container.x1; x_iterator <= container.x2; x_iterator++)
                        {
                            for (int y_iterator = container.y1; y_iterator <= container.y2; y_iterator++)
                            {
                                if (shapes.dominoes[i].IsInside(new Point(x_iterator, y_iterator), scalingX, scalingY))
                                {
                                    Color col = source.GetPixel(x_iterator, y_iterator);
                                    r += col.R;
                                    g += col.G;
                                    b += col.B;
                                    counter++;
                                }
                            }
                        }
                        if (counter != 0)
                        {
                            c = Color.FromRgb((byte)(r / counter), (byte)(g / counter), (byte)(b / counter)).ToLab();
                        }
                        else // rectangle too small
                        {
                            c = source.GetPixel(container.x1, container.y1).ToLab();
                        }
                    }
                    // determine ideal color
                    double minimum = double.MaxValue;
                    for (int color = 0; color < colors.Count; color++)
                    {
                        double value = colorMode.Compare(c, colors[color].labColor);
                        if (value < minimum)
                        {
                            minimum = value;
                            dominoes[i] = color;
                        }
                    }
                }
                shapesValid = true;
                return dominoes;
            }
        }
        #endregion
    }

}
