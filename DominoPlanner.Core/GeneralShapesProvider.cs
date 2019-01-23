using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Windows;
using Emgu.CV;
using Emgu.CV.Util;
using System.ComponentModel;
using Emgu.CV.Structure;
using ProtoBuf;
using DominoPlanner.Core.RTree;
//using Emgu.CV.Structure;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Oberklasse für alle Strukturen, deren Farben aus beliebig angeordneten Rechtecken oder Pfaden berechnet werden.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(StructureParameters))]
    [ProtoInclude(101, typeof(CircularStructure))]
    public abstract class GeneralShapesProvider : IDominoProvider
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
                usedColorsValid = false;
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
        protected GeneralShapesProvider(string filepath, string imagePath, string colors, IColorComparison comp, Dithering ditherMode,
            AverageMode averageMode, bool allowStretch, IterationInformation iterationInformation)
            : base(filepath, imagePath, comp, ditherMode, colors, iterationInformation)
        {
            this.allowStretch = allowStretch;
            average = averageMode;
        }
        protected GeneralShapesProvider(int imageWidth, int imageHeight, Color background, string colors, IColorComparison comp, Dithering ditherMode,
            AverageMode averageMode, bool allowStretch, IterationInformation iterationInformation)
            : base(imageWidth, imageHeight, background, comp, ditherMode, colors, iterationInformation)
        {
            this.allowStretch = allowStretch;
            average = averageMode;
        }
        public int charLength;
        protected GeneralShapesProvider() : base() { }
        #endregion
        #region public methods
        #endregion
        #region private helper methods
        /// <summary>
        /// Weist jedem Shape die ideale Farbe zu, basierend auf den festgelegten Eigenschaften.
        /// </summary>
        /// <returns>ein Int-Array mit den Farbindizes</returns>
        internal override void CalculateColors()
        {
            var colors = this.colors.RepresentionForCalculation;
            if (!shapesValid) throw new InvalidOperationException("Current shapes are invalid!");
            IterationInformation.weights = Enumerable.Repeat(1.0, colors.Length).ToArray();
            RTree<IDominoShape> tree = new RTree<IDominoShape>(9, new GuttmannQuadraticSplit<IDominoShape>());
            // wird nur beim Dithering benötigt und nur dann ausgeführt; sortiert alle Shapes nach deren Mittelpunktskoordinate 
            // erst nach x, bei gleichem x nach y
            var list = shapes.dominoes.OrderByDescending(x =>
            {
                var container = x.GetContainer();
                return container.y + container.height / 2;
            }).ThenBy(x =>
            {
                var container = x.GetContainer();
                return container.x + container.width / 2;
            }).ToList();
            if (ditherMode.weights.GetLength(0) + ditherMode.weights.GetLength(1) > 2)
            {
                for (int i = 0; i < shapes.dominoes.Length; i++)
                {
                    tree.Insert(shapes.dominoes[i]);
                }
            }
            for (int iter = 0; iter < IterationInformation.maxNumberOfIterations; iter++)
            {
                if (ditherMode.weights.GetLength(0) + ditherMode.weights.GetLength(1) > 2)
                {
                    double extent_r = (ditherMode.matrix_width - ditherMode.start_first_row) * charLength;
                    double extent_l = (ditherMode.start_first_row - 1) * charLength;
                    double extent_u = (ditherMode.matrix_height - 1) * charLength;
                    // ditherColors im Baum ersetzen
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].ditherColor = list[i].originalColor;
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        var originalColor = list[i].ditherColor;
                        list[i].CalculateColor(colors, colorMode, TransparencySetting, IterationInformation.weights);
                        // Abweichung der beiden Farben bestimmen
                        int fehler_r = (int)(originalColor.Red - colors[list[i].color].mediaColor.R);
                        int fehler_g = (int)(originalColor.Green - colors[list[i].color].mediaColor.G);
                        int fehler_b = (int)(originalColor.Blue - colors[list[i].color].mediaColor.B);
                        // bestimme Abmessungen des Suchbereichs
                        DominoRectangle orig = list[i].getBoundingRectangle();
                        double orig_x = orig.x + orig.width / 2;
                        double orig_y = orig.y + orig.height / 2;
                        DominoRectangle viewport = new DominoRectangle()
                        {
                            x = orig_x - extent_l,
                            y = orig_y - extent_u,
                            width = extent_r + extent_l,
                            height = extent_u
                        };
                        var result = tree.Search(viewport);
                        var weights = new double[result.Count];
                        // Rohgewichte aller gefundenen Shapes finden
                        for (int j = 0; j < result.Count; j++)
                        {
                            var bounding = result[j].getBoundingRectangle();
                            // alle rausschmeißen, die nicht komplett im Viewport liegen
                            double center_x = bounding.x + bounding.width / 2;
                            double center_y = bounding.y + bounding.height / 2;
                            // überprüfen, ob das Shape schon abgearbeitet wurde
                            if (center_y == orig_y && center_x <= orig_x)
                                continue;
                            if (center_y > orig_y) continue;
                            weights[j] = ditherMode.Weight((center_x - orig_x) / charLength, (orig_y - center_y) / charLength);
                        }
                        var divisor = weights.Sum();
                        if (divisor == 0)
                        {

                        }
                        for (int j = 0; j < result.Count; j++)
                        {
                            if (weights[j] == 0) continue;
                            ditherMode.AddToPixel(result[j],
                        (int)(fehler_r * weights[j] / divisor),
                        (int)(fehler_g * weights[j] / divisor),
                        (int)(fehler_b * weights[j] / divisor));
                        }

                    }
                }
                else
                {
                    ResetDitherColors(shapes.dominoes);
                    IterationInformation.numberofiterations = iter;
                    Console.WriteLine($"Iteration {iter}");
                    Parallel.For(0, shapes.dominoes.Length, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, (i) =>
                    {
                        shapes.dominoes[i].CalculateColor(colors, colorMode, TransparencySetting, IterationInformation.weights);
                    });
                }
                // Farben zählen
                IterationInformation.EvaluateSolution(colors.ToArray(), shapes.dominoes);
                if (IterationInformation.colorRestrictionsFulfilled != false) break;

            }
            last = new DominoTransfer(shapes.dominoes, this.colors);
        }
        internal override void ReadUsedColors()
        {
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
                Parallel.For(0, shapes.dominoes.Length, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, (i) =>
                {
                    if (average == AverageMode.Corner)
                    {
                        DominoRectangle container = shapes.dominoes[i].GetContainer(scalingX, scalingY);
                        shapes.dominoes[i].originalColor = 
                        new Bgra(img.Data[container.y1, container.x1, 0], img.Data[container.y1, container.x1, 1],
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
                            shapes.dominoes[i].originalColor = new Bgra((byte)(B / counter), (byte)(G / counter), (byte)(R / counter), (byte)(A / counter));
                        }
                        else // rectangle too small
                        {
                            shapes.dominoes[i].originalColor = new Bgra(img.Data[container.y1, container.x1, 0], img.Data[container.y1, container.x1, 1],
                            img.Data[container.y1, container.x1, 2], img.Data[container.y1, container.x1, 3]);
                        }
                    }
                });
            }
            usedColorsValid = true;
        }
        
        #endregion
    }

}
