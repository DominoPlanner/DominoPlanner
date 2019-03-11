using DominoPlanner.Core.RTree;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner.Core
{
    [ProtoContract]
    [ProtoInclude(100, typeof(EmptyCalculation))]
    [ProtoInclude(101, typeof(NonEmptyCalculation))]
    public abstract class Calculation
    {
        [ProtoMember(1000)]
        internal bool LastValid;

        public abstract void Calculate(DominoTransfer shapes, int charLength);
    }
    [ProtoContract]
    public class EmptyCalculation : Calculation
    {
        public EmptyCalculation()
        {
            LastValid = true;
        }
        public override void Calculate(DominoTransfer shapes, int charLength)
        {
            LastValid = true;
        }
    }
    [ProtoContract]
    [ProtoInclude(100, typeof(UncoupledCalculation))]
    [ProtoInclude(101, typeof(CoupledCalculation))]
    public abstract class NonEmptyCalculation : Calculation
    {
        #region properties
        private IColorComparison _colorMode;
        /// <summary>
        /// Der Interpolationsmodus, der zur Farberkennung berechnet wird.
        /// </summary>
        public IColorComparison ColorMode
        {
            get => _colorMode;
            set
            {
                if (value.GetType() != _colorMode?.GetType())
                {
                    _colorMode = value;
                    LastValid = false;
                }
            }
        }
        [ProtoMember(1)]
        public string ColorModeSurrogate
        {
            get => _colorMode.GetType().Name;
            set
            {
                _colorMode = (IColorComparison)Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.{value}"));
            }
        }

        private Dithering _dithering;
        /// <summary>
        /// Gibt an, ob ein Fehlerkorrekturalgorithmus verwendet werden soll.
        /// </summary>
        public Dithering Dithering
        {
            get => _dithering;
            set
            {
                if (value.GetType() != _dithering?.GetType())
                {
                    _dithering = value;
                    LastValid = false;
                }
            }
        }
        [ProtoMember(2)]
        private string DitheringSurrogate
        {
            get => _dithering.GetType().Name;
            set
            {
                _dithering = (Dithering)Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.{value}"));
            }
        }
        IterationInformation _iterationInfo;
        /// <summary>
        /// Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.
        /// </summary>
        [ProtoMember(3)]
        public IterationInformation IterationInformation
        {
            get
            {
                return _iterationInfo;
            }
            set
            {
                _iterationInfo = value;
                _iterationInfo.PropertyChanged +=
                    new PropertyChangedEventHandler(delegate (object s, PropertyChangedEventArgs e) { LastValid = false; });
                LastValid = false;
            }
        }
        private byte _TransparencySetting = 0;
        [ProtoMember(4)]
        public byte TransparencySetting
        {
            get => _TransparencySetting;
            set
            {
                LastValid = false; _TransparencySetting = value;
            }
        }
        [ProtoMember(5)]
        public ObservableCollection<ColorFilter> ColorFilters { get; private set; }
        #endregion
        #region constructors
        public NonEmptyCalculation()
        {
            ColorFilters = new ObservableCollection<ColorFilter>();
            ColorFilters.CollectionChanged += ColorFiltersChanged;
        }
        public NonEmptyCalculation(IColorComparison colorMode, Dithering dithering, IterationInformation iterationInformation) : this()
        {
            ColorMode = colorMode;
            Dithering = dithering;
            IterationInformation = iterationInformation;
        }
        #endregion
        #region private methods
        private void ColorFiltersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newitems = e.NewItems;
                foreach (var item in newitems)
                {
                    ((ColorFilter)item).PropertyChanged += new PropertyChangedEventHandler((s, param) => LastValid = false);
                }
            }
        }
        internal ColorRepository ApplyColorFilters(ColorRepository repo)
        {
            var color_filtered = Serializer.DeepClone(repo);
            foreach (ColorFilter filter in ColorFilters)
            {
                filter.Apply(color_filtered);
            }
            return color_filtered;
        }
        #endregion
    }
    [ProtoContract]
    public class CoupledCalculation : NonEmptyCalculation
    {
        public override void Calculate(DominoTransfer shapes, int charLength)
        {
            throw new NotImplementedException();
        }
    }
    [ProtoContract(SkipConstructor =true)]
    [ProtoInclude(100, typeof(FieldCalculation))]
    public class UncoupledCalculation : NonEmptyCalculation
    {
        #region properties
        public StateReference StateReference { get; set; }
        #endregion
        #region constructors
        public UncoupledCalculation(IColorComparison colorMode, Dithering dithering, IterationInformation iterationInformation)
            : base(colorMode, dithering, iterationInformation)
        {
            
        }
        #endregion
        #region overrides
        public override void Calculate(DominoTransfer shapes, int charLength)
        {
            var colors = ApplyColorFilters(shapes.colors).RepresentionForCalculation;
            //if (!shapesValid) throw new InvalidOperationException("Current shapes are invalid!");
            IterationInformation.weights = Enumerable.Repeat(1.0, colors.Length).ToArray();
            RTree<IDominoShape> tree = new RTree<IDominoShape>(9, new GuttmannQuadraticSplit<IDominoShape>());
            // wird nur beim Dithering benötigt und nur dann ausgeführt; sortiert alle Shapes nach deren Mittelpunktskoordinate 
            // erst nach x, bei gleichem x nach y
            var list = shapes.shapes.OrderByDescending(x =>
            {
                var container = x.GetContainer();
                return container.y + container.height / 2;
            }).ThenBy(x =>
            {
                var container = x.GetContainer();
                return container.x + container.width / 2;
            }).ToList();
            if (Dithering.weights.GetLength(0) + Dithering.weights.GetLength(1) > 2)
            {
                for (int i = 0; i < shapes.length; i++)
                {
                    tree.Insert(shapes[i]);
                }
            }
            for (int iter = 0; iter < IterationInformation.maxNumberOfIterations; iter++)
            {
                if (Dithering.weights.GetLength(0) + Dithering.weights.GetLength(1) > 2)
                {
                    double extent_r = (Dithering.matrix_width - Dithering.start_first_row) * charLength;
                    double extent_l = (Dithering.start_first_row - 1) * charLength;
                    double extent_u = (Dithering.matrix_height - 1) * charLength;
                    // ditherColors im Baum ersetzen
                    for (int i = 0; i < list.Count; i++)
                    {
                        list[i].PrimaryDitherColor = list[i].PrimaryOriginalColor;
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        var originalColor = list[i].PrimaryDitherColor;
                        list[i].CalculateColor(colors, ColorMode, TransparencySetting, IterationInformation.weights);
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
                            weights[j] = Dithering.Weight((center_x - orig_x) / charLength, (orig_y - center_y) / charLength);
                        }
                        var divisor = weights.Sum();
                        if (divisor == 0)
                        {

                        }
                        for (int j = 0; j < result.Count; j++)
                        {
                            if (weights[j] == 0) continue;
                            Dithering.AddToPixel(result[j],
                        (int)(fehler_r * weights[j] / divisor),
                        (int)(fehler_g * weights[j] / divisor),
                        (int)(fehler_b * weights[j] / divisor));
                        }

                    }
                }
                else
                {
                    ResetDitherColors(shapes);
                    IterationInformation.numberofiterations = iter;
                    Console.WriteLine($"Iteration {iter}");
                    Parallel.For(0, shapes.length, new ParallelOptions() { MaxDegreeOfParallelism = -1 }, (i) =>
                    {
                        shapes[i].CalculateColor(colors, ColorMode, TransparencySetting, IterationInformation.weights);
                    });
                }
                // Farben zählen
                IterationInformation.EvaluateSolution(colors.ToArray(), shapes.shapes);
                if (IterationInformation.colorRestrictionsFulfilled != false) break;
            }
        }
        #endregion
        #region internal methods
        internal void ResetDitherColors(DominoTransfer shapes)
        {
            foreach (var domino in shapes.shapes)
            {
                if (StateReference == StateReference.Before)
                {
                    domino.PrimaryDitherColor = domino.PrimaryOriginalColor;
                    domino.color = 0;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
        #endregion
    }
    [ProtoContract]
    public class FieldCalculation : UncoupledCalculation
    {
        #region constructors
        public FieldCalculation(IColorComparison colorMode, Dithering dithering, IterationInformation iterationInformation)
            : base(colorMode, dithering, iterationInformation)
        {

        }
        #endregion
        #region overrides
        public override void Calculate(DominoTransfer shapes, int charLength)
        {
            int height = shapes.FieldPlanHeight;
            int length = shapes.FieldPlanLength;
            var colors = ApplyColorFilters(shapes.colors).RepresentionForCalculation;
            IterationInformation.weights = Enumerable.Repeat(1.0, colors.Length).ToArray();

            for (int iter = 0; iter < IterationInformation.maxNumberOfIterations; iter++)
            {
                ResetDitherColors(shapes);
                IterationInformation.numberofiterations = iter;
                Console.WriteLine($"Iteration {iter}");
                Parallel.For(0, height, new ParallelOptions() { MaxDegreeOfParallelism = Dithering.maxDegreeOfParallelism }, (y) =>
                {
                    for (int x = 0; x < length; x++)
                    {
                        shapes[length * y + x].CalculateColor(colors, ColorMode, TransparencySetting, IterationInformation.weights);
                        int error_r = (int)(shapes[length * y + x].PrimaryDitherColor.Red - colors[shapes[length * y + x].color].mediaColor.R);
                        int error_g = (int)(shapes[length * y + x].PrimaryDitherColor.Green - colors[shapes[length * y + x].color].mediaColor.G);
                        int error_b = (int)(shapes[length * y + x].PrimaryDitherColor.Blue - colors[shapes[length * y + x].color].mediaColor.B);
                        DiffuseErrorField(x, y, error_r, error_g, error_b, shapes, length, height);
                    }
                });
                IterationInformation.EvaluateSolution(colors, shapes.shapes);
                if (IterationInformation.colorRestrictionsFulfilled != false) break;
            }
            LastValid = true;

        }
        #endregion
        #region private methods
        private void DiffuseErrorField(int x, int y, int v1, int v2, int v3, DominoTransfer shapes, int length, int height)
        {
            for (int j = 0; j < Dithering.weights.GetLength(0); j++)
            {
                for (int i = Dithering.startindizes[j]; i < Dithering.weights.GetLength(1); i++)
                {
                    int akt_x = x + i - Dithering.startindizes[0] + 1;
                    int akt_y = y + j;
                    if (akt_x >= length) continue; //akt_x = length - 1;
                    if (akt_x < 0) continue; //akt_x = 0;
                    if (akt_y < 0) continue; // akt_y = 0;
                    if (akt_y >= height) continue; // akt_y = height - 1;
                    Dithering.AddToPixel(shapes[akt_y * length + akt_x],
                       v1 * Dithering.weights[j, i] / Dithering.divisor,
                       v2 * Dithering.weights[j, i] / Dithering.divisor,
                        v3 * Dithering.weights[j, i] / Dithering.divisor);
                }
            }
        }
        #endregion
    }
}
