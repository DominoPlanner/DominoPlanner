using System;
using System.Collections.Generic;
using ColorMine.ColorSpaces.Comparisons;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using OfficeOpenXml;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Die allgemeine Basisklasse für die Erstellung jeglicher Domino-Objekte.
    /// </summary>
    public abstract class IDominoProvider : ICloneable
    {
        #region public properties
        /// <summary>
        /// Gibt an, ob das Objekt eine Protokolldefinition besitzt oder nicht.
        /// Auf der Basis dieser Information sollten die entsprechenden Buttons angezeigt werden oder nicht.
        /// </summary>
        public bool hasProcotolDefinition { get; set; }
        /// <summary>
        /// Das Bitmap, welchem dem aktuellen Objekt zugrunde liegt.
        /// </summary>
        protected WriteableBitmap source;
        /// <summary>
        /// Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.
        /// </summary>
        public bool useOnlyMyColors { get; set; }
        private List<DominoColor> _colors;
        /// <summary>
        /// Die Farben, die für dieses Objekt verwendet werden sollen.
        /// </summary>
        public List<DominoColor> colors
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
        /// <summary>
        /// Wird diese Eigenschaft gesetzt, wird ein Objekt generiert, dessen Steinanzahl möglichst nahe am angegeben Wert liegt.
        /// Dabei wird versucht, das Seitenverhältnis des Quellbildes möglichst zu wahren.
        /// </summary>
        public abstract int targetCount { set; }
        private IColorSpaceComparison _colorMode;
        /// <summary>
        /// Der Interpolationsmodus, der zur Farberkennung berechnet wird.
        /// </summary>
        public IColorSpaceComparison colorMode
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

        /// <summary>
        /// Liste der Filter, die vor der Berechnung angewendet werden
        /// </summary>
        public List<PreFilter> PreFilters { get; set; }

        public List<ImageFilter> ImageFilters { get; set; }

        
        public List<PostFilter> PostFilters { get; set; }
        /// <summary>
        /// Gibt einen Array zurück, der für alle Farben der colors-Eigenschaft die Anzahl in dem Objekt angibt.
        /// </summary>
        public int[] counts
        {
            get
            {
                if (!shapesValid || !lastValid) throw new InvalidOperationException("Unreflected Changes in this object, please recalculate to get counts");
                int[] counts = new int[colors.Count];
                if (last != null)
                {
                    foreach (int item in last.dominoes)
                    {
                        if (item >= 0)
                        {
                            counts[item]++;
                        }
                  }
                }
                return counts;
            }
        }
        #endregion
        protected bool shapesValid = false;
        public bool lastValid = false;

        public DominoTransfer last;
        #region const
        protected IDominoProvider(WriteableBitmap bitmap, bool useOnlyMyColors, IColorSpaceComparison comp, List<DominoColor> colors, DominoFilter calculationFilters)
        {
            source = FixTransparency(bitmap);
            this.colorMode = comp;
            this.useOnlyMyColors = useOnlyMyColors;
            this.colors = colors;
            this.calculationFilters = calculationFilters;
        }
        #endregion
        #region public methods
        /// <summary>
        /// Generiert das Objekt.
        /// Die Methode erkennt automatisch, welche Teile des DominoTransfers regeneriert werden müssen.
        /// </summary>
        /// <param name="progressIndicator">Kann für Threading verwendet werden.</param>
        /// <returns>Einen DominoTransfer, der alle Informationen über das fertige Objekt erhält.</returns>
        public abstract DominoTransfer Generate(IProgress<string> progressIndicator = null);
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
        /// <summary>
        /// Behebt das Transparenzproblem, indem das Bitmap auf ein weißes Bitmap überblendet wird.
        /// </summary>
        /// <param name="source">Das zu fixende Bitmap.</param>
        /// <returns>Das Bitmap, nur ohne Transparenz auf weißem Hintergrund.</returns>
        private static WriteableBitmap FixTransparency(WriteableBitmap source)
        {
            WriteableBitmap bitm = BitmapFactory.New(source.PixelWidth, source.PixelHeight);
            bitm.Clear(Color.FromArgb(255, 255, 255, 255));
            bitm.Blit(new Rect(0, 0, source.PixelWidth, source.PixelHeight), source, new Rect(0, 0, source.PixelWidth, source.PixelHeight), WriteableBitmapExtensions.BlendMode.None);
            return bitm;
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
            for (int i= 0; i < basefield.GetLength(0); i++)
            {
                for (int j = 0; j < basefield.GetLength(1); j++)
                {
                    basefield[i, j] = -2; // set all values to no domino
                }
            }
            for (int i = 0; i < last.length; i++)
            {
                if (last[i].Item1.position != null) // to avoid null reference
                {
                    basefield[last[i].Item1.position.x, last[i].Item1.position.y] = last.dominoes[i];
                }
            }
            if (o == Orientation.Vertical) basefield = TransposeArray(basefield);
            return basefield;
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

        public abstract object Clone();
        #endregion
    }
}

