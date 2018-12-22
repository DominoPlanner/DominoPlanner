using Emgu.CV;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Stellt die Eigenschaften und Methoden bereit, eine Struktur zu erstellen.
    /// </summary>
    [ProtoContract]
    public partial class StructureParameters : GeneralShapesProvider, ICountTargetable
    {
        // spiegelt das XElement für die Serialisierung, damit wir nicht das gesamte StructureDefinition-Objekt serialisieren müssen
        private string __structureDefXML;
        [ProtoMember(3)]
        private string _structureDefinitionXML
        {
            set
            {
                __structureDefXML = value;
                structureDefinitionXML = XElement.Parse(value);
            }
            get
            {
                return __structureDefXML;
            }
        }
        #region public properties
        /// <summary>
        /// Das XElement, das die Strukturdefinition beinhaltet.
        /// </summary>
        public XElement structureDefinitionXML
        {
            set
            {
                hasProcotolDefinition = value.Attribute("HasProtocolDefinition").Value == "true";
                name = value.Attribute("Name").Value;
                cells = new CellDefinition[3, 3];
                foreach (XElement part in value.Elements("PartDefinition"))
                {
                    int col = GetIndex(part.Attribute("HorizontalPosition").Value);
                    int row = GetIndex(part.Attribute("VerticalPosition").Value);
                    cells[col, row] = new CellDefinition(part);
                }
                __structureDefXML = value.ToString();
                shapesValid = false;
                // calculate and set characteristic lengths for Dithering
                charLength = (int)(Math.Sqrt((cells[1, 1].width * cells[1, 1].height)/cells[1,1].Count) * 1.5d);
            }
        }
        private int _length;
        /// <summary>
        /// Die Länge der Struktur (Wiederholungen des mittleren Blocks in x-Richtung)
        /// </summary>
        [ProtoMember(1)]
        public int length
        {
            get
            {
                return _length;
            }
            set
            {
                _length = value;
                _current_width = value;
                shapesValid = false;
            }
        }
        private int _height;
        /// <summary>
        /// Die Breite der Struktur (Wiederholungen des mittleren Blocks in y-Richtung)
        /// </summary>
        [ProtoMember(2)]
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
        /// <summary>
        /// Die Zielgröße des Objekts. Setzen überschreibt Länge und Breite.
        /// </summary>

        public int TargetCount
        {
            // Maple sagt, dass diese Formel passt... ;)
            set
            {
                double cw = cells[1, 1].width;
                double ch = cells[1, 1].height;
                double addw = cells[0, 0].width + cells[2, 2].width;
                double addh = cells[0, 0].height + cells[2, 2].height;
                double lc = cells[0, 1].dominoes.Length;
                double tc = cells[1, 0].dominoes.Length;
                double rc = cells[2, 1].dominoes.Length;
                double bc = cells[1, 2].dominoes.Length;
                double cc = cells[1, 1].dominoes.Length;
                double constant = cells[0, 0].dominoes.Length + cells[0, 2].dominoes.Length + cells[2, 0].dominoes.Length 
                    + cells[2, 2].dominoes.Length;
                double root = Math.Sqrt(Math.Pow(cc * addw - cw * (lc + rc), 2) * Math.Pow(source.Height, 2) +
                    (2 * (-addh * addw * cc * cc + (((-2 * constant + 2 * value) * cw + addw * (bc + tc)) * ch + cw * addh * (lc + rc)) * cc + ch * cw * (lc + rc) * (bc + tc))) * source.Width * source.Height
                    + Math.Pow(source.Width, 2) * Math.Pow(-addh * cc + ch * (bc + tc), 2));
                double templength = (.5d * (root + (-cc * addw + (-lc - rc) * cw) * source.Height + (addh * cc + (-bc - tc) * ch) * source.Width)) / (cc * cw * source.Height);
                double tempheight = (.5d * (root + (-addh * cc + (-bc - tc) * ch) * source.Width + (cc * addw + (-lc - rc) * cw) * source.Height)) / (cc * ch * source.Width);

                if (templength < tempheight)
                {
                    length = (int)Math.Round(templength);
                    height = (int)Math.Round(-(-cw * length * source.Height + addh * source.Width - addw * source.Height) / (ch * source.Width));
                }
                else
                {
                    height = (int)Math.Round(tempheight);
                    length = (int)Math.Round((ch * height * source.Width + addh * source.Width - addw * source.Height) / (cw * source.Height));
                }


            }
        }
        #endregion
        #region public constructors
        /// <summary>
        /// Generiert eine Struktur mit den angegebenen Wiederholparametern in x- und y-Richtung.
        /// </summary>
        /// <param name="bitmap">Das Bitmap, welchem der Struk zugrunde liegen soll.</param>
        /// <param name="definition">Die XML-Strukturdefinition, die verwendet werden soll.</param>
        /// <param name="length">Die Anzahl der Wiederholung der mittleren Zelle in x-Richtung.</param>
        /// <param name="height">Die Anzahl der Wiederholung der mittleren Zelle in y-Richtung.</param>
        /// <param name="colors">Die Farben, die für dieses Objekt verwendet werden sollen.</param>
        /// <param name="colorMode">Der Interpolationsmodus, der zur Farberkennung verwendet wird.</param>
        /// <param name="averageMode">Gibt an, ob nur ein Punkt des Dominos (linke obere Ecke) oder ein Durchschnittswert aller Pixel unter dem Pfad verwendet werden soll, um die Farbe auszuwählen.</param>
        /// <param name="allowStretch">Gibt an, ob beim Berechnen die Struktur an das Bild angepasst werden darf.</param>
        /// <param name="useOnlyMyColors">Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.</param>
        public StructureParameters(string imagepath, XElement definition, int length, int height, string colors, 
            IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, IterationInformation iterationInformation, bool allowStretch = false) :
            base(imagepath, colors, colorMode, ditherMode, averageMode, allowStretch, iterationInformation)
        {
            structureDefinitionXML = definition;
            this.length = length;
            this.height = height;
        }
        /// <summary>
        /// Generiert eine Struktur mit der angegebenen Steineanzahl.
        /// Dabei wird versucht, das Seitenverhältnis des Bildes möglichst anzunähern.
        /// </summary>
        /// <param name="bitmap">Das Bitmap, welchem der Struktur zugrunde liegen soll.</param>
        /// <param name="definition">Die XML-Strukturdefinition, die verwendet werden soll.</param>
        /// <param name="colors">Die Farben, die für dieses Objekt verwendet werden sollen.</param>
        /// <param name="colorMode">Der Interpolationsmodus, der zur Farberkennung verwendet wird.</param>
        /// <param name="averageMode">Gibt an, ob nur ein Punkt des Dominos (linke obere Ecke) oder ein Durchschnittswert aller Pixel unter dem Pfad verwendet werden soll, um die Farbe auszuwählen.</param>
        /// <param name="allowStretch">Gibt an, ob beim Berechnen die Struktur an das Bild angepasst werden darf.</param>
        /// <param name="useOnlyMyColors">Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.</param>
        /// <param name="targetSize">Die Zielgröße des Objekts.</param>
        public StructureParameters(string imagepath, XElement definition, int targetSize, String colors, 
            IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, IterationInformation iterationInformation, bool allowStretch = false)
            : this(imagepath, definition, 1, 1, colors, colorMode, ditherMode, averageMode, iterationInformation, allowStretch)
        {
            TargetCount = targetSize;
        }
        public StructureParameters(int imageWidth, int imageHeight, Color background, XElement definition,
            int targetSize, String colors,
            IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, IterationInformation iterationInformation, bool allowStretch = false)
            : base(imageWidth, imageHeight, background, colors, colorMode, ditherMode, averageMode, allowStretch, iterationInformation)
        {
            structureDefinitionXML = definition;
            TargetCount = targetSize;
        }
        private StructureParameters() : base() { }
        #endregion
        #region private helper methods
        internal override void GenerateShapes()
        {
            shapes = GenerateStructure(length, height);
            shapesValid = true;
        }

        
        #endregion
    }
}
