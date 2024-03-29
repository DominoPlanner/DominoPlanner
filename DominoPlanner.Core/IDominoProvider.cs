﻿using OfficeOpenXml;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Avalonia.Media;
using SkiaSharp;
using System.Linq;

namespace DominoPlanner.Core
{
    [ProtoContract]
    [ProtoInclude(100, typeof(FieldParameters))]
    [ProtoInclude(101, typeof(GeneralShapesProvider))]
    public abstract class IDominoProvider : IWorkspaceLoadable, IWorkspaceLoadColorList, IWorkspaceLoadImageFilter, IHasThumbnail
    {
        #region properties
        [ProtoMember(1, OverwriteList = true)]
        public int[] Counts
        {
            get
            {
                if (Last == null) return null;
                //if (!Editing && !lastValid) throw new InvalidOperationException("Unreflected changes in this object, please recalculate to get counts");
                int[] counts = new int[colors.Length];
                if (Last != null)
                {
                    foreach (var shape in Last.shapes)
                    {
                        if (shape.Color < counts.Length)
                            counts[shape.Color]++;
                    }
                }
                return counts;
            }
            private set { }
        }
        [ProtoMember(5)]
        public byte[] Thumbnail
        {
            get
            {
                SKSurface surf = SKSurface.Create(new SKImageInfo(256, 256));
                if (Last != null && Last.colors != null)
                {
                    surf = Last.GenerateImage(Colors.White, 256, expanded: true);
                }
                var image = surf.Snapshot().Encode();
                using MemoryStream memoryStream = new MemoryStream();
                image.SaveTo(memoryStream);
                return memoryStream.ToArray();

            }
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
                if (value.Contains("\\")) value = value.Replace("\\", "/");

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
                    shapesValid = false;
                    if (this is IRowColumnAddableDeletable && !value)
                    {
                        // we only want to reset the size when we switch from editing away.
                        // (otherwise, we also reset it when loading the file and editing is active)
                        (this as IRowColumnAddableDeletable).ResetSize();
                    }
                }
            }
        }
        public bool SecondarySideCalculated { get => SecondaryImageTreatment != null; }
        [ProtoMember(19)]
        protected ImageTreatment _primaryImageTreatment;
        
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
                    _primaryImageTreatment.parent = this;
                }
            }
        }
        protected ImageTreatment _secondaryImageTreatment;
        [ProtoMember(20)]
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
                    _secondaryImageTreatment.parent = this;
                }
            }
        }
        protected Calculation _primaryCalculation;
        [ProtoMember(21)]
        public virtual Calculation PrimaryCalculation
        {
            get => _primaryCalculation;
            set
            {
                if (value != _primaryCalculation && value != null && SuitableCalculation(value))
                {
                    _primaryCalculation = value;
                    _primaryCalculation.LastValid = false;
                    if (_primaryCalculation is UncoupledCalculation calculation)
                    {
                        calculation.StateReference = StateReference.Before;
                    }
                    if (_primaryCalculation is CoupledCalculation)
                    {
                        _secondaryCalculation = new EmptyCalculation();
                    }
                }
            }
        }
        protected Calculation _secondaryCalculation = new EmptyCalculation();
        [ProtoMember(22)]
        public virtual Calculation SecondaryCalculation
        {
            get => _secondaryCalculation;
            set
            {
                if (value != _secondaryCalculation && value != null && SuitableCalculation(value) && !(PrimaryCalculation is CoupledCalculation))
                {
                    _secondaryCalculation = value;
                    _secondaryCalculation.LastValid = false;
                    if (_secondaryCalculation is UncoupledCalculation calculation)
                    {
                        calculation.StateReference = StateReference.After;
                    }
                }
            }
        }
        [ProtoMember(23)]
        public Orientation FieldPlanDirection { get; set; } = Orientation.Horizontal;
        protected bool LastValid { get => Editing || ((SecondaryCalculation?.LastValid != false) && (PrimaryCalculation?.LastValid != false)); }
        #endregion
        #region internal vars
        public int charLength;
        private DominoTransfer _last;
        [ProtoMember(2)]
        public DominoTransfer Last
        {
            get => _last;
            set
            {

                if (_last != value)
                {
                    _last = value;
                    shapesValid = false;
                }
            }
        }
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
        /// <summary>
        /// Generiert das Objekt.
        /// Die Methode erkennt automatisch, welche Teile des DominoTransfers regeneriert werden müssen.
        /// </summary>
        /// <param name="progressIndicator">Kann für Threading verwendet werden.</param>
        /// <returns>Einen DominoTransfer, der alle Informationen über das fertige Objekt erhält.</returns>
        public virtual DominoTransfer Generate(CancellationToken ct, IProgress<string> progressIndicator = null)
        {
            if (Editing) return Last;
            ct.ThrowIfCancellationRequested();
            if (!shapesValid)
            {
                RegenerateShapes();
                shapesValid = true;
                if (PrimaryImageTreatment != null)
                    PrimaryImageTreatment.colorsValid = false;
                if (SecondaryImageTreatment != null)
                    SecondaryImageTreatment.colorsValid = false;
            }
            Last.colors = colors;

            ct.ThrowIfCancellationRequested();
            if (SecondaryImageTreatment != null && !SecondaryImageTreatment.colorsValid)
            {

                SecondaryImageTreatment.parent = this;
                SecondaryImageTreatment.FillDominos(Last);
                SecondaryCalculation.LastValid = false;
                if (PrimaryCalculation is CoupledCalculation)
                {
                    PrimaryCalculation.LastValid = false;
                }
                SecondaryImageTreatment.colorsValid = true;
            }
            ct.ThrowIfCancellationRequested();
            if (!PrimaryImageTreatment.colorsValid || !PrimaryImageTreatment.sourceValid)
            {
                PrimaryImageTreatment.parent = this;
                PrimaryImageTreatment.FillDominos(Last);
                PrimaryImageTreatment.colorsValid = true;
                PrimaryCalculation.LastValid = false;
            }
            ct.ThrowIfCancellationRequested();
            if (!PrimaryCalculation.LastValid)
            {
                PrimaryCalculation.Calculate(Last, charLength, ct);
                PrimaryCalculation.LastValid = true;
            }
            ct.ThrowIfCancellationRequested();
            if (!SecondaryCalculation.LastValid)
            {
                SecondaryCalculation.Calculate(Last, charLength, ct);
                SecondaryCalculation.LastValid = true;
            }
            return Last;
        }
        public DominoTransfer Generate()
        {
            return Generate(new CancellationToken());
        }
        // <summary>
        /// Liefert das HTML-Protokoll eines Objekts.
        /// Falls das Objekt keine Strukturdefinition besitzt, wird eine InvalidOperationException geworfen.
        /// </summary>
        /// <param name="parameters">Die Parameter des Protokolls.</param>
        /// <returns>Ein String, der das HTML des Protokoll enthält.</returns>
        public string GetHTMLProcotol(ObjectProtocolParameters parameters)
        {
            return parameters.GetHTMLProcotol(GenerateProtocol(parameters.Templates, parameters.orientation, parameters.mirrorHorizontal, parameters.mirrorVertical));
        }
        /// <summary>
        /// Speichert das Excel-Protokoll eines Objekts am angegebenen Ort.
        /// Falls das Objekt keine Strukturdefinition besitzt, wird eine InvalidOperationException geworfen.
        /// </summary>
        /// <param name="path">Der Speicherort des Protokolls.</param>
        /// <param name="parameters">Die Parameter des Protokolls.</param>
        public void SaveXLSFieldPlan(string path, ObjectProtocolParameters parameters)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            FileInfo file = new FileInfo(path);
            if (file.Exists) file.Delete();
            ExcelPackage pack = new ExcelPackage(file);
            pack = parameters.GenerateExcelFieldplan(GenerateProtocol(parameters.Templates, parameters.orientation, parameters.mirrorHorizontal, parameters.mirrorVertical), pack);
            pack.Save();
            pack.Dispose();
            GC.Collect();
        }
        public ProtocolTransfer GenerateProtocol(List<int> templates = null, bool MirrorX = false, bool MirrorY = false)
        {
            return GenerateProtocol(templates, FieldPlanDirection, MirrorX, MirrorY);
        }
        /// <summary>
        /// Generiert das Protokoll eines Objekts.
        /// </summary>
        /// <param name="templateLength">Die Länge der Blöcke (optional)</param>
        /// <param name="o">Die Orientierung des Protokolls (optional)</param>
        /// <param name="reverse">Gibt an, ob das Objekt von der anderen Seite gebaut werden soll. Macht eigentlich nur bei Felder Sinn (optional)</param>
        /// <returns></returns>
        public ProtocolTransfer GenerateProtocol(List<int> templates = null, Orientation o = Orientation.Horizontal, bool MirrorX = false, bool MirrorY = false)
        {
            int[,] dominoes = GetBaseField(o, MirrorX, MirrorY);

            ProtocolTransfer d = new ProtocolTransfer
            {
                dominoes = new List<List<Tuple<int, int>>>[dominoes.GetLength(1)],
                orientation = o,
                colors = colors
            };
            for (int i = 0; i < dominoes.GetLength(1); i++) // foreach line
            {
                int posX = 0;
                int templateIndex = 0;
                d.dominoes[i] = new List<List<Tuple<int, int>>>();
                for (int j = 0; posX < dominoes.GetLength(0); j++) // foreach block in this line
                {
                    int currentcount = 0;
                    int currentColor = -2;
                    int blockCounter = 0;
                    List<Tuple<int, int>> currentColors = new List<Tuple<int, int>>();
                    int templateLength = int.MaxValue;
                    if(templates != null)
                    {
                        if(templateIndex == templates.Count)
                        {
                            templateIndex = 0;
                        }
                        templateLength = templates[templateIndex];
                    }
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
                    templateIndex++;
                }
            }
            d.counts = Counts;
            d.rows = (o == Orientation.Horizontal) ? dominoes.GetLength(1) : dominoes.GetLength(0);
            d.columns = (o == Orientation.Horizontal) ? dominoes.GetLength(0) : dominoes.GetLength(1);
            return d;
        }
        public virtual int[,] GetBaseField(Orientation o = Orientation.Horizontal, bool MirrorX = false, bool MirrorY = false)
        {
            if (!HasProtocolDefinition) throw new InvalidOperationException("This object does not have a protocol definition.");
            if (!Editing && (!LastValid || !shapesValid)) throw new InvalidOperationException("This object has unreflected changes.");
            int[,] basefield = new int[Last.FieldPlanLength, Last.FieldPlanHeight];
            for (int i = 0; i < basefield.GetLength(0); i++)
            {
                for (int j = 0; j < basefield.GetLength(1); j++)
                {
                    basefield[i, j] = -2; // set all values to no domino
                }
            }
            for (int i = 0; i < Last.Length; i++)
            {
                if (Last[i].position != null)
                {
                    basefield[Last[i].position.x, Last[i].position.y] = Last[i].Color;
                }
            }
            if (o == Orientation.Vertical) basefield = TransposeArray(basefield);
            if (MirrorX == true)
            {
                basefield = MirrorArrayX(basefield);
            }
            if (MirrorY == true)
            {
                basefield = MirrorArrayY(basefield);
            }
            return basefield;
        }
        public void Save(string filepath = "")
        {
            Workspace.Save(this, filepath);
            Workspace.Save(colors); // TODO: Notify the Color tab that it has been saved. (low priority, doesn't really matter tbh)
        }
        #endregion
        #region events
        #endregion
        #region abstracts
        public abstract void RegenerateShapes();
        
        protected static T[,] TransposeArray<T>(T[,] array)
        {
            T[,] temp = new T[array.GetLength(1), array.GetLength(0)];
            for (int i = 0; i < array.GetLength(1); i++)
            {
                for (int j = 0; j < array.GetLength(0); j++)
                {
                    //temp[i, j] = array[j, array.GetLength(0) - i - 1];
                    temp[i, j] = array[j, i];
                }
            }
            return temp;
        }
        protected static T[,] MirrorArrayY<T>(T[,] array)
        {
            T[,] temp = new T[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    temp[i, j] = array[i, array.GetLength(1) - j - 1];
                }
            }
            return temp;
        }
        protected static T[,] MirrorArrayX<T>(T[,] array)
        {
            T[,] temp = new T[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    temp[i, j] = array[array.GetLength(0) - i - 1, j];
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "called by protobuf after serialization")]
        private void RestoreShapes()
        {
            if (colors.Length < Last.shapes.Max(x => x.Color))
                throw new InvalidDataException("More colors are used in the project than are available in the color list.");
            if (PrimaryImageTreatment == null || PrimaryCalculation == null)
            {
                throw new InvalidDataException("Probably a very old file, please convert with DominoPlanner 3.0.0alpha4");
            }
            Generate();
            Last.colors = colors;
        }
        #endregion

        #region compatibility properties
        /*private Calculation TempCalculation;
        private ImageTreatment TempImageTreatment;
        internal Calculation CreatePrimaryCalculation()
        {
            if (TempCalculation == null)
            {
                if (this is FieldParameters)
                {
                    TempCalculation = new FieldCalculation(new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
                }
                else
                {
                    TempCalculation = new UncoupledCalculation(new CieDe2000Comparison(), new Dithering(), new NoColorRestriction());
                }
            }
            return TempCalculation;
        }
        internal ImageTreatment CreatePrimaryTreatment()
        {
            if (TempImageTreatment == null)
            {
                if (this is FieldParameters)
                {
                    TempImageTreatment = new FieldReadout((FieldParameters)this, 10, 10, Emgu.CV.CvEnum.Inter.Cubic);
                }
                else
                {
                    TempImageTreatment = new NormalReadout(this, 10, 10, AverageMode.Average, true);
                }
            }
            return TempImageTreatment;
        }
        
        [ProtoMember(5)]
        private IterationInformation IterationInformation
        {
            get
            {
                return ((NonEmptyCalculation)PrimaryCalculation)?.IterationInformation ?? new NoColorRestriction();
                return new NoColorRestriction();
            }
            set
            {
                if (CreatePrimaryCalculation() is NonEmptyCalculation calc)
                {
                    calc.IterationInformation = value;
                }
                
            }
        }
        
        [ProtoMember(6)]
        private byte TransparencySetting
        {
            get => ((NonEmptyCalculation)PrimaryCalculation)?.TransparencySetting ?? 0;
            set
            {
                if (CreatePrimaryCalculation() is NonEmptyCalculation calc)
                {
                    calc.TransparencySetting = value;
                }
            }
        }
        
        [ProtoMember(11)]
        private string colorMode_surrogate
        {
            get
            {
                return ((NonEmptyCalculation)PrimaryCalculation)?.ColorMode.GetType().Name ?? "CieDe2000Comparison";
            }
            set
            {
                if (CreatePrimaryCalculation() is NonEmptyCalculation calc)
                {
                    calc.ColorMode = (IColorComparison)Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.{value}"));
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
        
        [ProtoMember(13, OverwriteList = true)]
        private ObservableCollection<ImageFilter> ImageFilters
        {
            get
            {
                return (PrimaryImageTreatment)?.ImageFilters ?? new ObservableCollection<ImageFilter>();
            }
            set
            {
                if (CreatePrimaryTreatment().ImageFilters != null)
                    CreatePrimaryTreatment().ImageFilters = value;
            }
        }
        
        [ProtoMember(15)]
        private int ImageWidth
        {
            get => 
                PrimaryImageTreatment?.Width ?? 10;
            set
            {
                CreatePrimaryTreatment().Width = value;
            }
        }
        [ProtoMember(16)]
        private int ImageHeight
        {
            get => PrimaryImageTreatment?.Height ?? 10;
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
                return PrimaryImageTreatment?.Background.ToString() ?? Colors.Transparent.ToString();
            }
            set { CreatePrimaryTreatment().Background = (Color)ColorConverter.ConvertFromString(value); }
        }
        
        [ProtoMember(18)]
        private string DitheringSurrogate
        {
            get
            {
                return ((NonEmptyCalculation)PrimaryCalculation)?.Dithering.GetType().Name ?? new Dithering().GetType().Name;
            }
            set
            {
                if (CreatePrimaryCalculation() is NonEmptyCalculation calc)
                {
                    calc.Dithering =
                        (Dithering)Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.{value}"));
                }
            }
        }
        */
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
            PrimaryImageTreatment = new NormalReadout(this, imageWidth, imageHeight, averageMode, allowStretch)
            {
                Background = background
            };
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
        /*
        [ProtoMember(1)]
        private AverageMode average
        {
            get
            {
                return ((NormalReadout)PrimaryImageTreatment)?.Average ?? AverageMode.Corner; 
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
                return ((NormalReadout)PrimaryImageTreatment)?.AllowStretch ?? true;
            }
            set
            {
                ((NormalReadout)CreatePrimaryTreatment()).AllowStretch = value;
            }
        }
        */
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

        private string _ColorPath;
        [ProtoMember(7)]
        public string ColorPath
        {
            get
            {
                return _ColorPath;
            }

            private set
            {
                _ColorPath = value.Replace("\\", "/");
            }
        }

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
    public interface IHasThumbnail
    {
        byte[] Thumbnail { get; }
    }
    [ProtoContract]
    public class IDominoProviderThumbnail : IHasThumbnail
    {
        [ProtoMember(5)]
        public byte[] Thumbnail { get; private set; }
    }

}
