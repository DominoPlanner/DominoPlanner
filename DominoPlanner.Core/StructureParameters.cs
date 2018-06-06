using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Stellt die Eigenschaften und Methoden bereit, eine Struktur zu erstellen.
    /// </summary>
    public class StructureParameters : RectangleDominoProvider
    {
        #region public properties
        /// <summary>
        /// Das XElement, das die Strukturdefinition beinhaltet.
        /// </summary>
        public XElement structureDefinitionXML
        {
            set
            {
                structuredef = new ClusterStructureDefinition(value);
                hasProcotolDefinition = structuredef.hasProtocolDefinition;
                shapesValid = false;
                lastValid = false;
            }
        }
        private int _length;
        /// <summary>
        /// Die Länge der Struktur (Wiederholungen des mittleren Blocks in x-Richtung)
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
                lastValid = false;
            }
        }
        private int _height;
        /// <summary>
        /// Die Breite der Struktur (Wiederholungen des mittleren Blocks in y-Richtung)
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
                lastValid = false;
            }
        }
        /// <summary>
        /// Die Zielgröße des Objekts. Setzen überschreibt Länge und Breite.
        /// </summary>
        public override int targetCount
        {
            set
            {
                double cw = structureDefinition.cells[1, 1].width;
                double ch = structureDefinition.cells[1, 1].height;
                double addw = structureDefinition.cells[0, 0].width + structureDefinition.cells[2, 2].width;
                double addh = structureDefinition.cells[0, 0].height + structureDefinition.cells[2, 2].height;
                double lc = structureDefinition.cells[0, 1].dominoes.Length;
                double tc = structureDefinition.cells[1, 0].dominoes.Length;
                double rc = structureDefinition.cells[2, 1].dominoes.Length;
                double bc = structureDefinition.cells[1, 2].dominoes.Length;
                double cc = structureDefinition.cells[1, 1].dominoes.Length;
                double constant = structureDefinition.cells[0, 0].dominoes.Length + structureDefinition.cells[0, 2].dominoes.Length + structureDefinition.cells[2, 0].dominoes.Length + structureDefinition.cells[2, 2].dominoes.Length;
                double root = Math.Sqrt(Math.Pow(cc * addw - cw * (lc + rc), 2) * Math.Pow(source.PixelHeight, 2) +
                    (2 * (-addh * addw * cc * cc + (((-2 * constant + 2 * value) * cw + addw * (bc + tc)) * ch + cw * addh * (lc + rc)) * cc + ch * cw * (lc + rc) * (bc + tc))) * source.PixelWidth * source.PixelHeight
                    + Math.Pow(source.PixelWidth, 2) * Math.Pow(-addh * cc + ch * (bc + tc), 2));
                double templength = (.5d * (root + (-cc * addw + (-lc - rc) * cw) * source.PixelHeight + (addh * cc + (-bc - tc) * ch) * source.PixelWidth)) / (cc * cw * source.PixelHeight);
                double tempheight = (.5d * (root + (-addh * cc + (-bc - tc) * ch) * source.PixelWidth + (cc * addw + (-lc - rc) * cw) * source.PixelHeight)) / (cc * ch * source.PixelWidth);

                if (templength < tempheight)
                {
                    length = (int)Math.Round(templength);
                    height = (int)Math.Round(-(-cw * length * source.PixelHeight + addh * source.PixelWidth - addw * source.PixelHeight) / (ch * source.PixelWidth));
                }
                else
                {
                    height = (int)Math.Round(tempheight);
                    length = (int)Math.Round((ch * height * source.PixelWidth + addh * source.PixelWidth - addw * source.PixelHeight) / (cw * source.PixelHeight));
                }


            }
        }
        #endregion
        private ClusterStructureDefinition structuredef;
        internal ClusterStructureDefinition structureDefinition
        {
            get
            {
                return structuredef;
            }
        }
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
        public StructureParameters(WriteableBitmap bitmap, XElement definition, int length, int height, List<DominoColor> colors, IColorSpaceComparison colorMode, AverageMode averageMode, bool allowStretch = false, bool useOnlyMyColors = false) :
            base(bitmap, colors, colorMode, useOnlyMyColors, null, averageMode, allowStretch)
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
        public StructureParameters(WriteableBitmap bitmap, XElement definition, int targetSize, List<DominoColor> colors, IColorSpaceComparison colorMode, AverageMode averageMode, bool allowStretch = false, bool useOnlyMyColors = false)
            : this(bitmap, definition, 1, 1, colors, colorMode, averageMode, allowStretch, useOnlyMyColors)
        {
            targetCount = targetSize;
        }
        #endregion
        #region private helper methods
        protected override void GenerateShapes()
        {
            shapes = structuredef.GenerateStructure(length, height);
            shapesValid = true;
        }
        #endregion
    }
}
