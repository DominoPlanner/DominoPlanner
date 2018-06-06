using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Comparisons;
using DominoPlanner.Util;
using Dominorechner_V2.ColorMine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;

namespace DominoPlanner.Document_Classes
{
    public enum structure_type
    {
        Rectangular,
        Spiral,
        Circle
    }
    [Serializable()]
    public class StructureDocument : ProjectDocument
    {
        /*
        Allgemein: Eine Struktur besteht aus 9 Blöcken, die wiederholt werden:
            Die 4 in den Ecken liegenden werden nur in der Ecke verwendet(habe bisher noch keine Struktur gefunden, die die wirklich braucht)#
            Die in der Mitte am Rand wird am Rand wiederholt, so oft die die daneben stehende Zahl (also links mitte + ecken ist der linke Rand)
	        Die in der Mitte wird in Länge und Breite wiederholt.
        Also etwa folgendes Schema (Length: 3, Height: 2):
	        ERRRE
	        RMMMR
	        RMMMR
            ERRRE
        Die Definition der Blöcke geschieht in der Strukturdefinition, die in den Einstellungen gespeichert ist und ein spezielles XML-Schema hat.

        Die Berechnung einer Struktur ist aufgeteilt in 5 Schritte, und je nach dem, welche Property geändert wurde, wird an der entsprechenden Stelle eingestiegen:
	        S: Beim Start: Lese alle Strukturdefinitionen ein. Diese sind in Settings.StructureTemplates.
	        F: Fülle Vorschaubilder der Strukturvorlage (unten links). Muss nur gemacht werden, wenn sich der Strukturtyp ändert.
	        R: Generiert die Menge der Dominos der Struktur in einem GenStructHelper und zeichnet diese auf dem Originalbild. Hier z.B. beim Ändern der Abmessungen einsteigen
	        C: Berechnet die Farben neu, die die Elemente der Struktur haben. Hier beim Ändern des Regressionsmodus einsteigen
	        D: Zeichnet die Struktur im Vorschaubild. Hier z.B. beim Ändern des ShowBorders-Wert einsteigen
        Die detaillierte Beschreibung der einzelnen Methoden befindet sich bei ihnen.
        */

        [field: NonSerialized]

        public Image SourceImage;
        
        public override List<DominoColor> Colors
        {
            get { return m_Colors; }
            set { m_Colors = value; }
        }
        [field: NonSerialized()]
        List<Lab> lab_colors;
        [field: NonSerialized()]
        private BitmapSource m_SourePreview;
        public BitmapSource SourcePreview { get { return m_SourePreview; } set { m_SourePreview = value; } }
        [field: NonSerialized()]
        private BitmapSource m_TargetPreview;
        public BitmapSource TargetPreview { get { return m_TargetPreview; } set { m_TargetPreview = value; } }
        [field: NonSerialized()]
        
        private ImageEditingDocument _filter;
        public override ImageEditingDocument filters
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
            }
        }
        #region UI accessible properties
        private structure_type _typ;
        public structure_type typ
        { 
            get
            {
                return _typ;
            }
            set
            {
                _typ = value;
                if (typ == structure_type.Rectangular)
                {
                    provider = new ClusterStructureProvider(5, 5, 0);
                    provider.parent = this;
                }
                else if (typ == structure_type.Spiral)
                {
                    provider = new SpiralProvider(100, 8, 24, 8, 16);
                    provider.parent = this;
                }
                else if (typ == structure_type.Circle)
                {
                    provider = new CircleProvider(10, 24, 8, 8, 8);
                    provider.parent = this;
                }

            }
        }

        private StructureRectProvider _provider;
        public StructureRectProvider provider
        {
            get
            {
                return _provider;
            }
            set
            {
                _provider = value;
                OnPropertyChanged("RCD");
            }

        }

        private bool m_calculation_mode;
        public bool calculation_mode
        {
            get
            {
                return m_calculation_mode;
            }
            set
            {
                m_calculation_mode = value;
                OnPropertyChanged("CD");
            }
        }

        private int m_regression_mode;
        public int regression_mode
        {
            get
            {
                return m_regression_mode;
            }
            set
            {
                m_regression_mode = value;
                OnPropertyChanged("CD");
            }
        }

        private bool m_allow_stretch;
        public bool allow_stretch
        {
            get
            {
                return m_allow_stretch;
            }
            set
            {
                m_allow_stretch = value;
                OnPropertyChanged("RCD");
            }
        }
        private bool? m_draw_borders;
        public bool? draw_borders
        {
            get
            {
                return m_draw_borders;
            }
            set
            {
                m_draw_borders = value;
                OnPropertyChanged("D");
            }
        }


        [field: NonSerialized()]
        private string m_imageInfo;

        public string imageInfo
        {
            get { return m_imageInfo; }
            set { m_imageInfo = value; }

        }
        private bool m_HasProtocolDefinition;
        public bool HasProtocolDefinition
        {
            get
            {
                return m_HasProtocolDefinition;
            }
            set
            {
                m_HasProtocolDefinition = value;
            }
        }
        #endregion
        // dominoes: Wie beim Feld. Enthält im Index n die Farbe des Steins mit der Position, der in rectangles an Stelle n gespeichert ist.
        public int[] dominoes
        {
            get; set;
        }

        public override int length
        {
            get
            {
                return 0;
            }

            set
            {
            }
        }

        public override int height
        {
            get
            {
                return 0;
            }

            set
            {
                
            }
        }

        // rectangles: Enthält alle Steine. Hat Rectangles die Abmessung [n, 10] ist eine Protokolldefinition vorhanden, falls nicht ist sie [n, 4]. Die werte sind in der zweiten Komponente wie oben angegeben gespeichert.
        public DominoDefinition[] shapes;

        private float size_x, size_y;
        [field: NonSerialized()]
        public override event PropertyChangedEventHandler PropertyChanged;

        public override void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                if (property.Contains('R')) // Rectangles
                {
                    // recalculate structure
                    UpdateDrawRectangles(property.Contains("filter"));
                }
                if (property.Contains('C')) // Colors
                {
                    UpdateDominoes();
                }
                if (property.Contains('D'))
                {
                    Draw();
                }
                if (property == "Locked") PropertyChanged(this, new PropertyChangedEventArgs(property));
                else PropertyChanged(this, new PropertyChangedEventArgs(null));
            }
        }

        public StructureDocument(String Path, String SourcePath, List<DominoColor> colors, StructureRectProvider prov) : this()
        {
            _provider = prov;
            _provider.parent = this;
            // assign object variables
            this.t = type.sct;
            this.Colors = colors;
            this.path = Path;
            this.SourcePath = SourcePath;
            m_allow_stretch = false;

            FileInfo f = new FileInfo(this.SourcePath); // möglicherweise hier Absturz wegen nicht relativem Pfad?
            using (FileStream s = new FileStream(base.SourcePath, FileMode.Open))
            {
                SourceImage = Image.FromStream(s);
            }
            InitFilters();


            m_draw_borders = true;
            this.filename = System.IO.Path.GetFileName(Path);
            
            m_regression_mode = 3;
            m_calculation_mode = false;
            // as converting from rgb to lab is too slow do it only once
            lab_colors = new List<Lab>();
            for (int i = 0; i < colors.Count; i++)
            {
                lab_colors.Add((new Rgb { R = colors[i].rgb.R, G = colors[i].rgb.G, B = colors[i].rgb.B }).To<Lab>());
            }
            UpdateDrawRectangles();
            UpdateDominoes();
            Draw();

        }
        public StructureDocument()
        {
            Locked = Visibility.Hidden;
        }

        public void InitFilters()
        {
            _filter = new ImageEditingDocument(SourcePath);
            _filter.t = type.img;
            _filter.path = this.SourcePath;
            _filter.filename = Path.GetFileName(SourcePath);
            _filter.parent = this;
        }
        // Siehe Klasse StructureDefinition, die macht eigentlich die ganze Arbeit.

        // berechnet die Rechtecke der Struktur und zeichnet sie auf dem Originalbild
        public void UpdateDrawRectangles(bool update_filter = false)
        {
            if (update_filter)
            {
                SourceImage = ImageHelper.BitmapImageToBitmap(filters.preview);
            }
            GenStructHelper gr = provider.UpdateRectangles();

            shapes = gr.dominoes;
            size_x = gr.width;
            size_y = gr.height;
            HasProtocolDefinition = gr.HasProtocolDefinition;

            //draw structure on top of image
            float scaling_x = SourceImage.Width / size_x;
            float scaling_y = SourceImage.Height / size_y;
            // if structure is not stretched to image size
            if (!m_allow_stretch)
            {
                if (scaling_x > scaling_y) scaling_x = scaling_y;
                else scaling_y = scaling_x;
            }
            // GDI+ ist einfach praktisch
            Image tempcopy = new Bitmap((Image)SourceImage);
            Graphics g = Graphics.FromImage(tempcopy);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            for (int i = 0; i < shapes.Length; i++)
            {
                GraphicsPath p = shapes[i].GetPath(scaling_x, scaling_y);
                g.FillPath(new SolidBrush(Color.FromArgb(180, 200, 200, 200)), p);
                g.DrawPath(new Pen(Color.Black, SourceImage.Width / 600), p);
            }
            SourcePreview = ImageHelper.BitmapToBitmapSource((Bitmap)tempcopy);
        }
        public void UpdateDominoes()
        {
            IColorSpaceComparison comp;
            switch (m_regression_mode)
            {
                case 0: comp = new CmcComparison(); break;
                case 1: comp = new Cie1976Comparison(); break;
                case 2: comp = new Cie94Comparison(); break;
                default: comp = new CieDe2000Comparison(); break;
            }

            float scaling_x = (SourceImage.Width - 1) / size_x;
            float scaling_y = (SourceImage.Height - 1) / size_y;
            if (!m_allow_stretch)
            {
                if (scaling_x > scaling_y) scaling_x = scaling_y;
                else scaling_y = scaling_x;
            }
            // fix transparency: if image is transparent, background appears black
            Bitmap notransparency = new Bitmap(SourceImage.Width, SourceImage.Height);
            Graphics temp = Graphics.FromImage(notransparency);
            temp.Clear(Color.White);
            System.Drawing.Imaging.ImageAttributes Att = new System.Drawing.Imaging.ImageAttributes();
            Att.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
            temp.DrawImage(SourceImage, new Rectangle(0, 0, SourceImage.Width, SourceImage.Height), 0, 0, SourceImage.Width, SourceImage.Height, GraphicsUnit.Pixel, Att);
            // image to read from
            WriteableBitmap pixelsource = BitmapFactory.ConvertToPbgra32Format(ImageHelper.BitmapToBitmapSource(notransparency));
            pixelsource.Lock();

            System.Windows.Media.Color[] sourceColors = new System.Windows.Media.Color[shapes.Length];
            dominoes = new int[shapes.Length];
            if (!calculation_mode)
            {
                // if source pixel = top left pixel of each domino
                for (int i = 0; i < shapes.Length; i++)
                {
                    RectangleF container = shapes[i].GetContainer(scaling_x, scaling_y);
                    try
                    {
                        sourceColors[i] = pixelsource.GetPixel((int)container.X, (int)container.Y);
                    }
                    catch (Exception) { }

                    dominoes[i] = Dithering.Compare(new Rgb() { R = sourceColors[i].R, G = sourceColors[i].G, B = sourceColors[i].B }.To<Lab>(), comp, lab_colors, 0);

                }
            }
            else
            {
                // if source pixel is average of region
                for (int i = 0; i < shapes.Length; i++)
                {
                    RectangleF container = shapes[i].GetContainer(scaling_x, scaling_y);

                    int r = 0, g = 0, b = 0;
                    int counter = 0;
                    // for loop: each container
                    for (float x_iterator = container.X; x_iterator < container.X + container.Width; x_iterator++)
                    {
                        for (float y_iterator = container.Y; y_iterator < container.Y + container.Width; y_iterator++)
                        {
                            if (shapes[i].IsInside(new PointF(x_iterator, y_iterator), true, scaling_x, scaling_y))
                            {
                                System.Windows.Media.Color c = pixelsource.GetPixel((int)x_iterator, (int)y_iterator);
                                r += c.R;
                                g += c.G;
                                b += c.B;
                                counter++;
                            }
                        }
                    }
                    sourceColors[i] = System.Windows.Media.Color.FromRgb((byte)(r / counter), (byte)(g / counter), (byte)(b / counter));
                    // calculates the color of the domino
                    dominoes[i] = Dithering.Compare(new Rgb() { R = sourceColors[i].R, G = sourceColors[i].G, B = sourceColors[i].B }.To<Lab>(), comp, lab_colors, 0);
                }
            }

        }
        // Zeichnet die Struktur im Vorschaubild. GDI+
        public void Draw(bool export = false)
        {
            Image b = new Bitmap((int)size_x, (int)size_y);
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.White);
            for (int i = 0; i < shapes.Length && i < dominoes.Length; i++)
            {
                GraphicsPath p = shapes[i].GetPath();
                g.FillPath(new SolidBrush(SystemMediaColorToSystemDrawingColor(Colors[dominoes[i]].rgb)), p);
                if (draw_borders == true) g.DrawPath(new Pen(Color.Black, size_x / ((export) ? 1200 : 600)), p);

            }
            TargetPreview = ImageHelper.BitmapToBitmapSource((Bitmap)b);
        }
        // Converter Method between different color classes
        private Color SystemMediaColorToSystemDrawingColor(System.Windows.Media.Color c)
        {
            return Color.FromArgb(c.R, c.G, c.B);
        }
        // global nützliche Methoden
        public static StructureDocument LoadStructureDocumentWithoutDrawing(string path)
        {
            StructureDocument structure;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter serialisierer = new BinaryFormatter();
                structure = (StructureDocument)serialisierer.Deserialize(fs);
            }
            structure.Colors = new List<DominoColor>();
            structure.UsedColors = new ObservableCollection<DominoColor>();
            structure.lab_colors = new List<Lab>();
            bool flag = false;
            if (structure.ColorList[0].IndexOf('\r') > -1)
            {
                flag = true;
            }
            for (int i = 0; i < structure.ColorList.Count; i++)
            {
                String[] values = structure.ColorList[i].Split(flag ? '\r' : ' ');
                structure.Colors.Add(new DominoColor(values[3], System.Windows.Media.Color.FromRgb(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2])), int.Parse(values[4])));
                structure.lab_colors.Add((new Rgb { R = structure.Colors[i].rgb.R, G = structure.Colors[i].rgb.G, B = structure.Colors[i].rgb.B }).To<Lab>());
            }
            structure.path = path;
            structure.filename = Path.GetFileName(path);
            if (structure.SourcePath == null || structure.SourcePath == "")
            {
                String imagepath = Path.Combine(Path.GetDirectoryName(path), "Source Images");
                structure.SourcePath = Directory.GetFiles(imagepath, Path.GetFileNameWithoutExtension(path) + "*")[0];
            }
            return structure;

        }
        public static StructureDocument LoadStructureDocument(string path)
        {
            StructureDocument d = LoadStructureDocumentWithoutDrawing(path);

            if (d.filters == null)
            {
                d.InitFilters();
            }
            d.filters.Deserialize_Finish(d.SourcePath);

            using (FileStream s = new FileStream(d.SourcePath, FileMode.Open))
            {
                d.SourceImage = Image.FromStream(s);
            }
            FileInfo f = new FileInfo(d.SourcePath);
            d.imageInfo = "Image Info: Resolution: " + d.SourceImage.Width + "x" + d.SourceImage.Height + ", Format: " + System.IO.Path.GetExtension(d.SourcePath) + ", File Size: " +
                ((f.Length > 1000000) ? (f.Length / 1000000) + " MB" : ((f.Length > 1000) ? (f.Length / 1000) + " KB" : f.Length + " B"));

            d.provider.OnLoad();
            d.UpdateDrawRectangles();
            d.Draw();
            return d;
        }
        public override bool Compare(Document d)
        {
            if (d is StructureDocument)
            {
                StructureDocument doc = d as StructureDocument;
                if (doc.shapes.Length != this.shapes.Length) return false;
                for (int i = 0; i < doc.shapes.Length; i++)
                {
                    if (!doc.shapes[i].Compare(this.shapes[i])) return false;
                }
                if (doc.Colors.Count != this.Colors.Count)
                    return false;
                for (int i = 0; i < doc.Colors.Count; i++)
                {
                    if (doc.Colors[i].rgb != this.Colors[i].rgb)
                        return false;
                    if (doc.Colors[i].name != this.Colors[i].name)
                        return false;
                    if (doc.Colors[i].count != this.Colors[i].count)
                        return false;
                }
                if (doc.regression_mode != this.regression_mode || doc.calculation_mode != this.calculation_mode || doc.allow_stretch != this.allow_stretch)
                    return false;
                if (!Enumerable.SequenceEqual(this.dominoes, doc.dominoes))
                    return false;
                if (doc.Locked != this.Locked) return false;
                return true;
            }
            return false;
        }

        public override void Save(string path)
        {
            ColorList = new List<String>();
            for (int i = 0; i < Colors.Count; i++)
            {
                System.Windows.Media.Color c = Colors[i].rgb;
                ColorList.Add(c.R + "\r" + c.G + "\r" + c.B + "\r" + Colors[i].name + "\r" + Colors[i].count);
            }
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter serialisierer = new BinaryFormatter();
                serialisierer.Serialize(fs, this);
            }
        }

        public override void SavePNG()
        {
            Draw(true);
            Microsoft.Win32.SaveFileDialog ofd = new Microsoft.Win32.SaveFileDialog();
            ofd.Filter = "Image File (*.png)|*.png";
            ofd.InitialDirectory = Path.GetDirectoryName(path);
            if (ofd.ShowDialog() == true)
            {

                if (ofd.FileName != string.Empty)
                {
                    try
                    {
                        using (FileStream stream = new FileStream(ofd.FileName, FileMode.Create))
                        {
                            PngBitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(TargetPreview.Clone()));
                            encoder.Save(stream);
                            stream.Close();
                        }
                    }
                    catch
                    {
                        System.Windows.MessageBox.Show("File not saved. Check if the path is locked by another application.");
                    }
                }
            }
        }
        // Berechnet aus der Struktur das Protokoll (bzw. ein Stub-Fielddocument, den Rest macht FieldPlanEditor)
        public FieldDocument GenerateBaseField()
        {
            if (!HasProtocolDefinition) return null;
            // get field dimensions
            int rowmax = 0; int colmax = 0;
            for (int i = 0; i < shapes.Length; i++)
            {
                if (shapes[i].ProtocolDefinition.x > rowmax) rowmax = shapes[i].ProtocolDefinition.x;
                if (shapes[i].ProtocolDefinition.y > colmax) colmax = shapes[i].ProtocolDefinition.y;
            }
            FieldDocument d = new FieldDocument() { filename = this.filename, dominoes = new int[rowmax + 1, colmax + 1], m_horizontal = true, path = this.path };
            for (int i = 0; i < d.dominoes.GetLength(0); i++)
            {
                for (int j = 0; j < d.dominoes.GetLength(1); j++)
                {
                    d.dominoes[i, j] = -1; // -1 ist der Wert für nicht zugewiesener Domino, diese werden im Feldprotokoll bis jetzt ignoriert
                }

            }
            for (int i = 0; i < shapes.Length; i++)
            {
                d.dominoes[(int)shapes[i].ProtocolDefinition.x, (int)shapes[i].ProtocolDefinition.y] = dominoes[i];
                d.Colors = Colors;
            }
            return d;
        }

    }
}
