using OfficeOpenXml;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;

namespace DominoPlanner.Core
{
    [ProtoContract(SkipConstructor = true)]
    [ProtoInclude(100, typeof(FieldParameters))]
    [ProtoInclude(101, typeof(GeneralShapesProvider))]
    public abstract class IDominoProvider : IWorkspaceLoadable, IWorkspaceLoadColorList, IWorkspaceLoadImageFilter
    {
        #region properties
        [ProtoMember(1, OverwriteList = true)]
        public int[] Counts
        {
            get
            {
                if (last == null) return null;
                if (!Editing && !lastValid) throw new InvalidOperationException("Unreflected changes in this object, please recalculate to get counts");
                int[] counts = new int[colors.Length];
                if (last != null)
                {
                    foreach (var shape in last.shapes)
                    {
                        counts[shape.color]++;
                    }
                }
                return counts;
            }
            private set { }
        }

        [ProtoMember(3)]
        public virtual bool HasProtocolDefinition { get; set; }

        private string _colorPath;
        [ProtoMember(7)]
        public string ColorPath
        {
            get => _colorPath;
            set
            {
                _colorPath = value;
                colors = Workspace.Load<ColorRepository>(Workspace.AbsolutePathFromReference(ref _colorPath, this));
            }
        }
        public ColorRepository colors;
        
        bool _editing;
        [ProtoMember(4)]
        public bool Editing
        {
            get { return _editing; }
            set
            {
                if (_editing != value)
                {
                    _editing = value;
                    EditingChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public bool SecondarySideCalculated { get => SecondaryImageTreatment != null; }

        protected ImageTreatment _primaryImageTreatment;
        //[ProtoMember(19)]
        public virtual ImageTreatment PrimaryImageTreatment
        {
            get => _primaryImageTreatment;
            set
            {
                if (value != _primaryImageTreatment && value != null && SuitableImageTreatment(value))
                {
                    _primaryImageTreatment = value;
                    _primaryImageTreatment.colorsValid = false;
                    _primaryImageTreatment.StateReference = StateReference.Before;
                }
            }
        }
        protected ImageTreatment _secondaryImageTreatment;
        //[ProtoMember(20)]
        public virtual ImageTreatment SecondaryImageTreatment
        {
            get => _secondaryImageTreatment;
            set
            {
                if (value != _secondaryImageTreatment && value != null && SuitableImageTreatment(value))
                {
                    _secondaryImageTreatment = value;
                    _secondaryImageTreatment.colorsValid = false;
                    _secondaryImageTreatment.StateReference = StateReference.After;
                }
            }
        }
        protected Calculation _primaryCalculation;
        //[ProtoMember(21)]
        public virtual Calculation PrimaryCalculation
        {
            get => _primaryCalculation;
            set
            {
                if (value != _primaryCalculation && value != null && SuitableCalculation(value))
                {
                    _primaryCalculation = value;
                    _primaryCalculation.LastValid = false;
                    if (_primaryCalculation is UncoupledCalculation)
                    {
                        ((UncoupledCalculation)_primaryCalculation).StateReference = StateReference.Before;
                    }
                    if (_primaryCalculation is CoupledCalculation)
                    {
                        _secondaryCalculation = new EmptyCalculation();
                    }
                }
            }
        }
        protected Calculation _secondaryCalculation = new EmptyCalculation();
        //[ProtoMember(22)]
        public virtual Calculation SecondaryCalculation
        {
            get => _secondaryCalculation;
            set
            {
                if (value != _secondaryCalculation && value != null && SuitableCalculation(value) && !(PrimaryCalculation is CoupledCalculation))
                {
                    _secondaryCalculation = value;
                    _secondaryCalculation.LastValid = false;
                    if (_secondaryCalculation is UncoupledCalculation)
                    {
                        ((UncoupledCalculation)_secondaryCalculation).StateReference = StateReference.After;
                    }
                }
            }
        }
        protected bool lastValid { get => (SecondaryCalculation?.LastValid != false) && (PrimaryCalculation?.LastValid != false); }
        #endregion
        #region internal vars
        protected int charLength;
        [ProtoMember(2)]
        internal DominoTransfer last;

        [ProtoMember(1000)]
        internal bool shapesValid;
        #endregion
        #region constructors
        public IDominoProvider(string filepath)
        {
            Save(filepath);
        }
        protected IDominoProvider()
        {

        }
        #endregion
        #region public methods
        public DominoTransfer Generate()
        {
            if (Editing) return last;
            
            if (!shapesValid)
            {
                RegenerateShapes();
                shapesValid = true;
                if (PrimaryImageTreatment != null)
                    PrimaryImageTreatment.colorsValid = false;
                if (SecondaryImageTreatment != null)
                    SecondaryImageTreatment.colorsValid = false;
            }
            last.colors = colors;
            if (SecondaryImageTreatment != null && !SecondaryImageTreatment.colorsValid)
            {
                SecondaryImageTreatment.FillDominos(last);
                SecondaryCalculation.LastValid = false;
                if (PrimaryCalculation is CoupledCalculation)
                {
                    PrimaryCalculation.LastValid = false;
                }
                SecondaryImageTreatment.colorsValid = true;
            }
            if (!PrimaryImageTreatment.colorsValid)
            {
                PrimaryImageTreatment.FillDominos(last);
                PrimaryImageTreatment.colorsValid = true;
                PrimaryCalculation.LastValid = false;
            }
            last.colors = this.colors;
            if (!PrimaryCalculation.LastValid)
            {
                PrimaryCalculation.Calculate(last, charLength);
                PrimaryCalculation.LastValid = true;
            }
            if (!SecondaryCalculation.LastValid)
            {
                SecondaryCalculation.Calculate(last, charLength);
                SecondaryCalculation.LastValid = true;
            }
            return last;
        }
        // <summary>
        /// Liefert das HTML-Protokoll eines Objekts.
        /// Falls das Objekt keine Strukturdefinition besitzt, wird eine InvalidOperationException geworfen.
        /// </summary>
        /// <param name="parameters">Die Parameter des Protokolls.</param>
        /// <returns>Ein String, der das HTML des Protokoll enthält.</returns>
        public string GetHTMLProcotol(ObjectProtocolParameters parameters)
        {
            return parameters.GetHTMLProcotol(GenerateProtocol(parameters.templateLength, parameters.orientation, parameters.reverse));
        }
        /// <summary>
        /// Speichert das Excel-Protokoll eines Objekts am angegebenen Ort.
        /// Falls das Objekt keine Strukturdefinition besitzt, wird eine InvalidOperationException geworfen.
        /// </summary>
        /// <param name="path">Der Speicherort des Protokolls.</param>
        /// <param name="parameters">Die Parameter des Protokolls.</param>
        public void SaveXLSFieldPlan(string path, ObjectProtocolParameters parameters)
        {
            parameters.path = path;
            FileInfo file = new FileInfo(path);
            if (file.Exists) file.Delete();
            ExcelPackage pack = new ExcelPackage(file);
            pack = parameters.GenerateExcelFieldplan(GenerateProtocol(parameters.templateLength, parameters.orientation, parameters.reverse), pack);
            pack.Save();
            pack.Dispose();
            GC.Collect();
        }
        /// <summary>
        /// Generiert das Protokoll eines Objekts.
        /// </summary>
        /// <param name="templateLength">Die Länge der Blöcke (optional)</param>
        /// <param name="o">Die Orientierung des Protokolls (optional)</param>
        /// <param name="reverse">Gibt an, ob das Objekt von der anderen Seite gebaut werden soll. Macht eigentlich nur bei Felder Sinn (optional)</param>
        /// <returns></returns>
        public ProtocolTransfer GenerateProtocol(int templateLength = int.MaxValue, Orientation o = Orientation.Horizontal, bool reverse = false)
        {
            int[,] dominoes = GetBaseField(o);
            int[,] tempdominoes;
            if (reverse == true)
            {
                // if reversed building direction
                tempdominoes = new int[dominoes.GetLength(0), dominoes.GetLength(1)];
                for (int i = 0; i < dominoes.GetLength(0); i++)
                {
                    for (int j = 0; j < dominoes.GetLength(1); j++)
                    {
                        tempdominoes[i, j] = dominoes[dominoes.GetLength(0) - i - 1, dominoes.GetLength(1) - j - 1];
                    }
                }
                dominoes = tempdominoes;
            }
            ProtocolTransfer d = new ProtocolTransfer();
            d.dominoes = new List<List<Tuple<int, int>>>[dominoes.GetLength(1)];
            d.orientation = o;
            d.colors = colors;
            for (int i = 0; i < dominoes.GetLength(1); i++) // foreach line
            {
                int posX = 0;
                d.dominoes[i] = new List<List<Tuple<int, int>>>();
                for (int j = 0; posX < dominoes.GetLength(0); j++) // foreach block in this line
                {
                    int currentcount = 0;
                    int currentColor = -2;
                    int blockCounter = 0;
                    List<Tuple<int, int>> currentColors = new List<Tuple<int, int>>();
                    while (blockCounter < templateLength && posX < dominoes.GetLength(0))
                    {
                        if (dominoes[posX, i] == currentColor)
                        {
                            currentcount++;
                        }
                        else
                        {
                            if (currentColor != -2) currentColors.Add(new Tuple<int, int>(currentColor, currentcount));
                            currentcount = 1;
                            currentColor = dominoes[posX, i];
                        }
                        posX++;
                        if (currentColor == -2) continue;
                        blockCounter++;
                    }
                    if (currentColor != -2) currentColors.Add(new Tuple<int, int>(currentColor, currentcount));
                    d.dominoes[i].Add(currentColors);
                }
            }
            d.counts = Counts;
            d.rows = (o == Orientation.Horizontal) ? dominoes.GetLength(0) : dominoes.GetLength(1);
            d.columns = (o == Orientation.Horizontal) ? dominoes.GetLength(1) : dominoes.GetLength(0);
            return d;
        }
        public virtual int[,] GetBaseField(Orientation o = Orientation.Horizontal)
        {
            if (!HasProtocolDefinition) throw new InvalidOperationException("This object does not have a protocol definition.");
            if (!Editing && (!lastValid || !shapesValid)) throw new InvalidOperationException("This object has unreflected changes.");
            int[,] basefield = new int[last.FieldPlanLength, last.FieldPlanHeight];
            for (int i = 0; i < basefield.GetLength(0); i++)
            {
                for (int j = 0; j < basefield.GetLength(1); j++)
                {
                    basefield[i, j] = -2; // set all values to no domino
                }
            }
            for (int i = 0; i < last.length; i++)
            {
                if (last[i].position != null)
                {
                    basefield[last[i].position.x, last[i].position.y] = last[i].color;
                }
            }
            if (o == Orientation.Vertical) basefield = TransposeArray(basefield);
            return basefield;
        }
        public void Save(string filepath = "")
        {
            Workspace.Save(this, filepath);
        }
        #endregion
        #region events
        public event EventHandler EditingChanged;
        #endregion
        #region abstracts
        protected abstract void RegenerateShapes();
        
        protected static T[,] TransposeArray<T>(T[,] array)
        {
            T[,] temp = new T[array.GetLength(1), array.GetLength(0)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    temp[i, j] = array[j, array.GetLength(0) - i - 1];
                }
            }
            return temp;
        }
        protected virtual bool SuitableCalculation(Calculation calc)
        {
            return true;
        }
        protected virtual bool SuitableImageTreatment(ImageTreatment treat)
        {
            return true;
        }
        #endregion
        #region private methods
        [ProtoAfterDeserialization]
        private void restoreShapes()
        {
            //bool lastValidTemp = lastValid;
            ////if (!Editing)
            ////{
            //PrimaryImageTreatment?.();
            //SecondaryImageTreatment?.UpdateSource();

            //ApplyImageFilters();
            //ApplyColorFilters();
            //GenerateShapes();
            //ReadUsedColors();
            ////}
            //lastValid = lastValidTemp;
        }
        #endregion
        #region compatibility properties
        internal Calculation CreatePrimaryCalculation()
        {
            if (PrimaryCalculation == null)
            {
                if (this is FieldParameters)
                {
                    PrimaryCalculation = new FieldCalculation(new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
                }
                else
                {
                    PrimaryCalculation = new UncoupledCalculation(new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
                }
            }
            return PrimaryCalculation;
        }
        internal ImageTreatment CreatePrimaryTreatment()
        {
            if (PrimaryImageTreatment == null)
            {
                if (this is FieldParameters)
                {
                    PrimaryImageTreatment = new FieldReadout((FieldParameters)this, 10, 10, Emgu.CV.CvEnum.Inter.Cubic);
                }
                else
                {
                    PrimaryImageTreatment = new NormalReadout(this, 10, 10, AverageMode.Average, true);
                }
            }
            return PrimaryImageTreatment;
        }
        [ProtoMember(5)]
        private IterationInformation IterationInformation
        {
            get
            {
                return new NoColorRestriction(); // ((NonEmptyCalculation)PrimaryCalculation).IterationInformation;
            }
            set
            {
                if (CreatePrimaryCalculation() is NonEmptyCalculation)
                {
                    ((NonEmptyCalculation)PrimaryCalculation).IterationInformation = value;
                }
                
            }
        }
        [ProtoMember(6)]
        private byte TransparencySetting
        {
            get => 0; //((NonEmptyCalculation)PrimaryCalculation).TransparencySetting;
            set
            {
                if (CreatePrimaryCalculation() is NonEmptyCalculation)
                {
                    ((NonEmptyCalculation)PrimaryCalculation).TransparencySetting = value;
                }
            }
        }
        [ProtoMember(11)]
        private string colorMode_surrogate
        {
            get
            {
                return "CieDe2000Comparison";//((NonEmptyCalculation)PrimaryCalculation).ColorMode.GetType().Name;
            }
            set
            {
                if (CreatePrimaryCalculation() is NonEmptyCalculation)
                {
                    ((NonEmptyCalculation)PrimaryCalculation).ColorMode = (IColorComparison)Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.{value}"));
                }
            }
        }
        [ProtoMember(12)]
        private ObservableCollection<ColorFilter> ColorFilters
        {
            get
            {
                return new ObservableCollection<ColorFilter>();
            }
            set
            {
                //
            }
        }
        [ProtoMember(13, OverwriteList =true)]
        private ObservableCollection<ImageFilter> ImageFilters
        {
            get
            {
                return new ObservableCollection<ImageFilter>(); //(PrimaryImageTreatment).ImageFilters;
            }
            set
            {
                CreatePrimaryTreatment().ImageFilters = value;
            }
        }
        [ProtoMember(15)]
        private int ImageWidth
        {
            get => 10; //PrimaryImageTreatment.Width;
            set
            {
                CreatePrimaryTreatment().Width = value;
            }
        }
        [ProtoMember(16)]
        private int ImageHeight
        {
            get => 10; // return PrimaryImageTreatment.Height;
            set
            {
                CreatePrimaryTreatment().Height = value;
            }
        }
        [ProtoMember(17)]
        private string ColorSerialized
        {
            get
            {
                return Colors.Transparent.ToString(); //PrimaryImageTreatment.Background.ToString();
            }
            set { CreatePrimaryTreatment().Background = (Color)ColorConverter.ConvertFromString(value); }
        }
        [ProtoMember(18)]
        private string DitheringSurrogate
        {
            get
            {
                return new Dithering().GetType().Name; //((NonEmptyCalculation)PrimaryCalculation).Dithering.GetType().Name;
            }
            set
            {
                if (CreatePrimaryCalculation() is NonEmptyCalculation)
                {
                    ((NonEmptyCalculation)PrimaryCalculation).Dithering =
                        (Dithering)Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.{value}"));
                }
            }
        }
        #endregion

    }
    [ProtoContract]
    [ProtoInclude(100, typeof(StructureParameters))]
    [ProtoInclude(101, typeof(CircularStructure))]
    public abstract class GeneralShapesProvider : IDominoProvider
    {
        #region constructors
        protected GeneralShapesProvider(string filePath, string imagePath, string colorPath, 
            IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, bool allowStretch, 
            IterationInformation iterationInformation) : base(filePath)
        {
            ColorPath = colorPath;
            PrimaryImageTreatment = new NormalReadout(this, imagePath, averageMode, allowStretch);
            PrimaryCalculation = new UncoupledCalculation(colorMode, ditherMode, iterationInformation);
        }

        protected GeneralShapesProvider(int imageWidth, int imageHeight, Color background, string colorPath, 
            IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, bool allowStretch, 
            IterationInformation iterationInformation)
        {
            ColorPath = colorPath;
            PrimaryImageTreatment = new NormalReadout(this, imageWidth, imageHeight, averageMode, allowStretch);
            PrimaryImageTreatment.Background = background;
            PrimaryCalculation = new UncoupledCalculation(colorMode, ditherMode, iterationInformation);
        }
        protected GeneralShapesProvider() { }
        #endregion
        #region overrides
        protected override bool SuitableCalculation(Calculation calc)
        {
            return !(calc is FieldCalculation);
        }
        protected override bool SuitableImageTreatment(ImageTreatment treat)
        {
            return !(treat is FieldReadout);
        }
        #endregion
        #region compatibility properties
        [ProtoMember(1)]
        private AverageMode average
        {
            get
            {
                return AverageMode.Corner; //((NormalReadout)PrimaryImageTreatment).Average;
            }
            set
            {
                ((NormalReadout)CreatePrimaryTreatment()).Average = value;
            }
        }
        [ProtoMember(2)]
        private bool allowStretch
        {
            get
            {
                return true; //return ((NormalReadout)PrimaryImageTreatment).AllowStretch;
            }
            set
            {
                ((NormalReadout)CreatePrimaryTreatment()).AllowStretch = value;
            }
        }
        #endregion
    }

    public enum StateReference
    {
        Before,
        After
    }
    public interface IWorkspaceLoadColorList : IWorkspaceLoadable
    {
        int[] Counts { get; }

        bool Editing { get; }

        string ColorPath { get;  }

        bool HasProtocolDefinition { get; }
    }
    public interface IWorkspaceLoadImageFilter : IWorkspaceLoadable
    {
        ImageTreatment PrimaryImageTreatment { get; }
    }
    public interface IWorkspaceLoadable
    {

    }

    [ProtoContract]
    public class IDominoProviderPreview : IWorkspaceLoadColorList
    {
        [ProtoMember(1)]
        public int[] Counts { get; private set; }

        [ProtoMember(4)]
        public bool Editing { get; private set; }

        [ProtoMember(7)]
        public string ColorPath { get; private set; }

        [ProtoMember(3)]
        public bool HasProtocolDefinition { get; private set; }
    }
    [ProtoContract]
    public class IDominoProviderImageFilter : IWorkspaceLoadImageFilter
    {
        
        private ImageTreatment _a;
        [ProtoMember(19)]
        public ImageTreatment PrimaryImageTreatment { get => _a;
            private set
            {
                _a = value;
            }
        }
    }
    public interface ICountTargetable
    {
        int TargetCount { set; }
    }

}
