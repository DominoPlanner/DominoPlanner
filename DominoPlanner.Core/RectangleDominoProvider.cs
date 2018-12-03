using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using DominoPlanner.Core.Dithering;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Util;
using System.ComponentModel;
using Emgu.CV.Structure;
using ProtoBuf;
//using Emgu.CV.Structure;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Oberklasse für alle Strukturen, deren Farben aus beliebig angeordneten Rechtecken oder Pfaden berechnet werden.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(StructureParameters))]
    [ProtoInclude(101, typeof(SpiralParameters))]
    [ProtoInclude(102, typeof(CircleParameters))]
    public abstract class RectangleDominoProvider : IDominoProvider
    {
        #region public properties
        private AverageMode _average;
        /// <summary>
        /// Gibt an, ob nur ein Punkt des Dominos (linke obere Ecke) oder ein Durchschnittswert aller Pixel unter dem Pfad verwendet werden soll, um die Farbe auszuwählen.
        /// </summary>
        [ProtoMember(1)]
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
        [ProtoMember(2)]
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
        protected RectangleDominoProvider(string imagePath, string colors, IColorComparison comp, 
            AverageMode averageMode, bool allowStretch, IterationInformation iterationInformation)
            : base(imagePath, comp, colors, iterationInformation)
        {
            this.allowStretch = allowStretch;
            average = averageMode;
        }
        protected RectangleDominoProvider() : base() { }
        #endregion
        #region public methods
        /// <summary>
        /// Generiert das Objekt.
        /// Die Methode erkennt automatisch, welche Teile des DominoTransfers regeneriert werden müssen.
        /// </summary>
        /// <param name="progressIndicator">Kann für Threading verwendet werden.</param>
        /// <returns>Einen DominoTransfer, der alle Informationen über das fertige Objekt erhält.</returns>
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
            }
            if (!shapesValid)
            {
                if (progressIndicator != null) progressIndicator.Report("Calculating Domino Positions...");
                GenerateShapes();
            }
            if (!lastValid)
            {
                if (progressIndicator != null) progressIndicator.Report("Calculating ideal colors...");
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
            var colors = this.colors.RepresentionForCalculation;
            if (!shapesValid) throw new InvalidOperationException("Current shapes are invalid!");
            IterationInformation.weights = Enumerable.Repeat(1.0, colors.Length).ToArray();
            /*if (IterationInformation is IterativeColorRestriction)
            {
                if (colors.Sum(color => color.count) < source.Width * source.Height)
                    throw new InvalidOperationException("Gesamtsteineanzahl ist größer als vorhandene Anzahl, kann nicht konvergieren");
            }*/
            Bgra[] usecolors = getUseColors();
            int[] dominoes = new int[shapes.dominoes.Length];
            // tatsächlich genutzte Farben auslesen
            for (int iter = 0; iter < IterationInformation.maxNumberOfIterations; iter++)
            {
                IterationInformation.numberofiterations = iter;
                Console.WriteLine($"Iteration {iter}");
                Parallel.For(0, shapes.dominoes.Length, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, (i) =>
                {
                    double minimum = int.MaxValue;
                    for (int color = 0; color < colors.Length; color++)
                    {
                        double value = colors[color].distance(usecolors[i], colorMode, TransparencySetting)
                        * IterationInformation.weights[color];
                        if (value < minimum)
                        {
                            minimum = value;
                            dominoes[i] = color;
                        }
                    }
                });
                // Farben zählen
                IterationInformation.EvaluateSolution(colors.ToArray(), dominoes);
                if (IterationInformation.colorRestrictionsFulfilled != false) break;
            }
            return dominoes;
        }
        private Bgra[] getUseColors()
        {
            var usecolors = new Bgra[shapes.dominoes.Length];
            using (Image<Bgra, Byte> img = image_filtered.ToImage<Bgra, Byte>())
            {
                double scalingX = (source.Width - 1) / shapes.width;
                double scalingY = (source.Height - 1) / shapes.height;
                if (!allowStretch)
                {
                    if (scalingX > scalingY) scalingX = scalingY;
                    else scalingY = scalingX;
                }
                
                // tatsächlich genutzte Farben auslesen
                Parallel.For(0, shapes.dominoes.Length, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, (i) =>
                {
                    Bgra c = new Bgra();
                    if (average == AverageMode.Corner)
                    {
                        DominoRectangle container = shapes.dominoes[i].GetContainer(scalingX, scalingY);
                        c = new Bgra(img.Data[container.y1, container.x1, 0], img.Data[container.y1, container.x1, 1],
                            img.Data[container.y1, container.x1, 2], img.Data[container.y1, container.x1, 3]);
                    }
                    else if (average == AverageMode.Average)
                    {
                        DominoRectangle container = shapes.dominoes[i].GetContainer(scalingX, scalingY);


                        double R = 0, G = 0, B = 0, A = 0;
                        int counter = 0;

                        // for loop: each container
                        for (int x_iterator = container.x1; x_iterator <= container.x2; x_iterator++)
                        {
                            for (int y_iterator = container.y1; y_iterator <= container.y2; y_iterator++)
                            {
                                if (shapes.dominoes[i].IsInside(new Point(x_iterator, y_iterator), scalingX, scalingY))
                                {
                                    R += img.Data[container.y1, container.x1, 2];
                                    G += img.Data[container.y1, container.x1, 1];
                                    B += img.Data[container.y1, container.x1, 0];
                                    A += img.Data[container.y1, container.x1, 3];
                                    counter++;
                                }
                            }
                        }
                        if (counter != 0)
                        {
                            c = new Bgra((byte)(B / counter), (byte)(G / counter), (byte)(R / counter), (byte)(A / counter));
                        }
                        else // rectangle too small
                        {
                            c = new Bgra(img.Data[container.y1, container.x1, 0], img.Data[container.y1, container.x1, 1],
                            img.Data[container.y1, container.x1, 2], img.Data[container.y1, container.x1, 3]);
                        }
                    }
                    usecolors[i] = c;
                });
            }
            return usecolors;
        }
        #endregion
    }

}
