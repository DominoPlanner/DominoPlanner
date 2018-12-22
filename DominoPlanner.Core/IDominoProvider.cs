using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using OfficeOpenXml;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ProtoBuf;
using System.ComponentModel;
using System.Collections.Specialized;
using Emgu.CV.Structure;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Die allgemeine Basisklasse für die Erstellung jeglicher Domino-Objekte.
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(FieldParameters))]
    [ProtoInclude(101, typeof(GeneralShapesProvider))]
    public abstract class IDominoProvider : ICloneable, IWorkspaceLoadColorList
    {
        #region public properties
        /// <summary>
        /// Gibt an, ob das Objekt eine Protokolldefinition besitzt oder nicht.
        /// Auf der Basis dieser Information sollten die entsprechenden Buttons angezeigt werden oder nicht.
        /// </summary>
        [ProtoMember(3)]
        public bool hasProcotolDefinition { get; set; }
        [ProtoMember(4, OverwriteList = true)]
        private byte[] source_surrogate
        {
            get
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    source.Bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    return memoryStream.GetBuffer();
                }
            }
            set
            {
                using (MemoryStream memoryStream = new MemoryStream(value))
                {
                    source = new Image<Emgu.CV.Structure.Bgra, byte>(new System.Drawing.Bitmap(memoryStream)).Mat;
                }
            }
        }
        /// <summary>
        /// Das Bitmap, welchem dem aktuellen Objekt zugrunde liegt.
        /// </summary>
        protected Mat source;
        IterationInformation _iterationInfo;
        /// <summary>
        /// Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.
        /// </summary>
        [ProtoMember(5)]
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
                    new PropertyChangedEventHandler(delegate (object s, PropertyChangedEventArgs e) { lastValid = false; });
                lastValid = false;
            }
        }
        private byte _TransparencySetting;
        [ProtoMember(6)]
        public byte TransparencySetting { get => _TransparencySetting; set { lastValid = false; _TransparencySetting = value; }}
        // das Repo wird nicht serialisiert, nur der Pfad dazu
        private ColorRepository _colors;
        public ColorRepository colors
        {
            get
            {
                return _colors;
            }
            set
            {
                _colors = value;
                lastValid = false;
            }
        }
        private string _colorPath;
        [ProtoMember(7)]
        public string ColorPath
        {
            get
            {
                return _colorPath;
            }
            set
            {
                _colorPath = value;
                colors = Workspace.Load<ColorRepository>(value);
            }
        }
        public ColorRepository color_filtered { get; private set; }
        [ProtoMember(8, OverwriteList = true)]
        private byte[] image_filtered_surrogate
        {
            get
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    image_filtered.Bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    return memoryStream.GetBuffer();
                }
            }
            set
            {
                using (MemoryStream memoryStream = new MemoryStream(value))
                {
                    image_filtered = new Image<Emgu.CV.Structure.Bgra, byte>(new System.Drawing.Bitmap(memoryStream)).Mat;
                }
            }
        }
        public Mat image_filtered { get; private set; }
        [ProtoMember(9)]
        public DominoTransfer last_filtered;
        /// <summary>
        /// Wird diese Eigenschaft gesetzt, wird ein Objekt generiert, dessen Steinanzahl möglichst nahe am angegeben Wert liegt.
        /// Dabei wird versucht, das Seitenverhältnis des Quellbildes möglichst zu wahren.
        /// </summary>
        private IColorComparison _colorMode;
        /// <summary>
        /// Der Interpolationsmodus, der zur Farberkennung berechnet wird.
        /// </summary>
        
        public IColorComparison colorMode
        {
            get
            {
                return _colorMode;
            }
            set
            {
                _colorMode = value;
                lastValid = false;
            }
        }
        [ProtoMember(11)]
        public string colorMode_surrogate
        {
            get
            {
                Console.WriteLine(_colorMode.GetType().AssemblyQualifiedName);
                Console.WriteLine(_colorMode.GetType().Name);
                return _colorMode.GetType().Name;
            }
            set
            {
                _colorMode = (IColorComparison)Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.{value}"));
            }
        }
        /// <summary>
        /// Liste der Filter, die vor der Berechnung angewendet werden
        /// </summary>
        [ProtoMember(12)]
        public ObservableCollection<ColorFilter> ColorFilters { get; private set; }
        [ProtoMember(13)]
        public ObservableCollection<ImageFilter> ImageFilters { get; private set; }
        [ProtoMember(14)]
        public ObservableCollection<PostFilter> PostFilters { get; private set; }
        /// <summary>
        /// Gibt einen Array zurück, der für alle Farben der colors-Eigenschaft die Anzahl in dem Objekt angibt.
        /// </summary>
        [ProtoMember(1, OverwriteList = true)]
        public int[] counts
        {
            get
            {
                if (!shapesValid || !lastValid) throw new InvalidOperationException("Unreflected changes in this object, please recalculate to get counts");
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
        private int _imagewidth;
        [ProtoMember(15)]
        public int ImageWidth
        {
            get { return _imagewidth; }
            set { _imagewidth = value; sourceValid = false; }
        }
        private int _imageheigth;
        [ProtoMember(16)]
        public int ImageHeight
        {
            get => _imageheigth;
            set { _imageheigth = value; sourceValid = false; }
        }
        private Color _background;
        [ProtoMember(17)]
        private String ColorSerialized
        {
            get
            {
                return _background.ToString();
            }
            set { background = (Color)ColorConverter.ConvertFromString(value); }
        }
        public Color background
        {
            get { return _background; }
            set { _background = value; sourceValid = false; }
        }
        private Dithering _ditherMode;
        /// <summary>
        /// Gibt an, ob ein Fehlerkorrekturalgorithmus verwendet werden soll.
        /// </summary>
        public Dithering ditherMode
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
        [ProtoMember(18)]
        private string DitheringSurrogate
        {
            get
            {
                return (_ditherMode.GetType().Name);
            }
            set
            {
                _ditherMode = (Dithering)Activator.CreateInstance(Type.GetType($"DominoPlanner.Core.{value}"));
            }
        }
        #endregion
        // müssen nach den Unterklassen deserialisiert werden
        [ProtoMember(1000)]
        protected bool shapesValid = false;
        [ProtoMember(1001)]
        public bool lastValid = false;
        [ProtoMember(1002)]
        public bool colorsValid = false;
        [ProtoMember(1003)]
        public bool imageValid = false;
        [ProtoMember(1004)]
        public bool sourceValid = false;
        [ProtoMember(1005)]
        public bool usedColorsValid = false;
        [ProtoMember(2)]
        public DominoTransfer last;
        #region const
        protected IDominoProvider(string bitmapPath, IColorComparison comp, Dithering ditherMode, string colorpath, IterationInformation iterationInformation) 
            : this()
        {
            //source = overlayImage(bitmap);
            var BlendFileFilter = new BlendFileFilter() { FilePath = bitmapPath };
            
            ImageWidth = BlendFileFilter.GetSizeOfMat().Width;
            ImageHeight = BlendFileFilter.GetSizeOfMat().Height;
            BlendFileFilter.CenterX = ImageWidth / 2;
            BlendFileFilter.CenterY = ImageHeight / 2;
            UpdateSource();
            this.ImageFilters.Add(BlendFileFilter);
            this.colorMode = comp;
            this.ColorPath = colorpath;
            this.IterationInformation = iterationInformation;
            this.ditherMode = ditherMode;
            
        }
        protected IDominoProvider()
        {
            this.ColorFilters = new ObservableCollection<ColorFilter>();
            this.ImageFilters = new ObservableCollection<ImageFilter>();
            this.PostFilters = new ObservableCollection<PostFilter>();
            this.ColorFilters.CollectionChanged +=
                new NotifyCollectionChangedEventHandler((sender, e) => ColorFiltersChanged(sender,e));
            this.ImageFilters.CollectionChanged +=
               new NotifyCollectionChangedEventHandler((sender, e) => ImageFiltersChanged(sender, e));
            this.PostFilters.CollectionChanged +=
                new NotifyCollectionChangedEventHandler((sender, e) => lastValid = false);
            background = Colors.Transparent;
        }
        protected IDominoProvider(int imageWidth, int imageHeight, Color background, 
            IColorComparison comp, Dithering ditherMode, string colorpath, IterationInformation iterationInformation) : this()
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            this.background = background;
            UpdateSource();
            this.colorMode = comp;
            this.ColorPath = colorpath;
            this.IterationInformation = iterationInformation;
            this.ditherMode = ditherMode;
        }
        #endregion
        #region public methods
        /// <summary>
        /// Generiert das Objekt.
        /// Die Methode erkennt automatisch, welche Teile des DominoTransfers regeneriert werden müssen.
        /// </summary>
        /// <param name="progressIndicator">Kann für Threading verwendet werden.</param>
        /// <returns>Einen DominoTransfer, der alle Informationen über das fertige Objekt erhält.</returns>
        public virtual DominoTransfer Generate(IProgress<string> progressIndicator = null)
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
                if (progressIndicator != null) progressIndicator.Report("Calculating domino shapes...");
                GenerateShapes();
                usedColorsValid = false;
            }
            if (!usedColorsValid)
            {
                if (progressIndicator != null) progressIndicator.Report("Reading pixels from image...");
                ReadUsedColors();
                lastValid = false;
            }
            if (!lastValid)
            {
                if (progressIndicator != null) progressIndicator.Report("Calculating ideal colors...");
                CalculateColors();
                lastValid = true;
            }
            return last;
        }
        /// <summary>
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
            int[,] tempdominoes = new int[dominoes.GetLength(0), dominoes.GetLength(1)];
            if (reverse == true)
            {
                // if reversed building direction
                tempdominoes = new int[dominoes.GetLength(0), dominoes.GetLength(1)];
                for (int i = 0; i < dominoes.GetLength(0); i++)
                {
                    for (int j = 0; j < dominoes.GetLength(1); j++)
                    {
                        dominoes[i, j] = tempdominoes[dominoes.GetLength(0) - i - 1, dominoes.GetLength(1) - j - 1];
                    }
                }
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
            d.counts = counts;
            d.rows = (o == Orientation.Horizontal) ? dominoes.GetLength(0) : dominoes.GetLength(1);
            d.columns = (o == Orientation.Horizontal) ? dominoes.GetLength(1) : dominoes.GetLength(0);
            return d;
        }
        #endregion
        #region internal methods
        
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
        private void ColorFiltersChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (color_filtered == null)
            {
                this.color_filtered = Serializer.DeepClone(colors);
            }
            // dann nur den neu hinzugekommenen Filter anwenden und mit einem Event versehen
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var newitems = e.NewItems;
                foreach (var item in newitems)
                {
                    ((ColorFilter)item).PropertyChanged += new PropertyChangedEventHandler((s, param) => ApplyColorFilters());
                    ((ColorFilter)item).Apply(color_filtered);
                }
            }
            else ApplyColorFilters();
            lastValid = false;
        }
        internal void UpdateSource()
        {
            this.source = new Image<Emgu.CV.Structure.Bgra, byte>(ImageWidth, ImageHeight,
                new Emgu.CV.Structure.Bgra(background.B, background.G, background.R, background.A)).Mat;
            imageValid = false;
            sourceValid = true;
        }
        internal abstract void GenerateShapes();

        internal abstract void ReadUsedColors();

        internal abstract void CalculateColors();

        internal void ResetDitherColors(IDominoShape[] shapes)
        {
            foreach (var domino in shapes)
            {
                domino.ditherColor = domino.originalColor;
                domino.color = 0;
            }
        }

        /// <summary>
        /// Berechnet das Basisfeld eines Objekts aus dessen Protokolldefinition. 
        /// </summary>
        /// <param name="o">Die Orientierung des gewünschten Basisfeldes</param>
        /// <returns>int-Array mit den Indizes des Farben</returns>
        public virtual int[,] GetBaseField(Orientation o = Orientation.Horizontal)
        {
            if (!hasProcotolDefinition) throw new InvalidOperationException("This object does not have a protocol definition.");
            if (!lastValid || !shapesValid) throw new InvalidOperationException("This object has unreflected changes.");
            int[,] basefield = new int[last.dominoLength, last.dominoHeight];
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
        public Mat overlayImage(Mat overlay)
        {
            if (overlay.NumberOfChannels != 4) return overlay;
            Image<Emgu.CV.Structure.Bgra, byte> overlay_img = overlay.ToImage<Emgu.CV.Structure.Bgra, byte>();

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            Parallel.For(0, overlay_img.Height, (y) =>
           {
               for (int x = overlay_img.Width - 1; x >= 0 ; x--)
               {
                   double opacity = (double)(overlay_img.Data[y, x, 3]) / 255;

                   for (int c = 2; c >= 0; c--)
                   {
                       overlay_img.Data[y, x, c] = (byte)(255 * (1 - opacity) + overlay_img.Data[y, x, c] * (opacity));
                   }
               }
           });
           
            watch.Stop();
            Console.WriteLine("Blend " + watch.ElapsedMilliseconds);
            return overlay_img.Mat;
        }
    
        /// <summary>
        /// Spiegelt einen Array an der Nicht-Delta-Diagonale
        /// </summary>
        /// <typeparam name="T">Der Typ des Arrays</typeparam>
        /// <param name="array">Der Array, der gespiegelt werden soll</param>
        /// <returns></returns>
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
        
        internal void ApplyColorFilters()
        {
            color_filtered = Serializer.DeepClone(colors);
            foreach (ColorFilter filter in ColorFilters)
            {
                filter.Apply(color_filtered);
            }
            lastValid = false;
            colorsValid = true;
        }
        [ProtoAfterDeserialization]
        internal void ColorAfterDeserial()
        {
            // Beim Load werden Stück für Stück die Farbfilter angewendet, da die Liste mit den Events versehen wird 
            // LastValid wird erst nach der Liste deserialisiert, das sollte also passen
        }
        internal void ApplyImageFilters()
        {
            var image_filtered = source.ToImage<Emgu.CV.Structure.Bgra, byte>();
            foreach (ImageFilter filter in ImageFilters)
            {
                filter.Apply(image_filtered);
            }
            this.image_filtered = image_filtered.Mat;
            lastValid = false;
            imageValid = true;
        }
        public void Save(string filepath)
        {
            filepath = Workspace.Instance.MakePathAbsolute(filepath);
            using (FileStream stream = new FileStream(filepath, FileMode.Create))
            {
                Serializer.Serialize(stream, this);
            }
        }
        public abstract object Clone();
        #endregion
        
    }

    public interface IWorkspaceLoadColorList : IWorkspaceLoadable
    {
        int[] counts { get; }
    }
    public interface IWorkspaceLoadable
    {

    }

    [ProtoContract]
    public class IDominoProviderPreview
    {
        [ProtoMember(1)]
        public int[] counts { get; set; }
    }

    public interface ICountTargetable
    {
        int TargetCount { set; }
    }

}

