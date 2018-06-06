using ColorMine.ColorSpaces.Comparisons;
using DominoPlanner.Util;
using Dominorechner_V2.ColorMine;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace DominoPlanner.Document_Classes

{
    [Serializable()]
    public class FieldDocument : ProjectDocument
    {
        #region properties
        [field: NonSerialized]
        public override event PropertyChangedEventHandler PropertyChanged;
        
        public int[,] dominoes;

        public int[] used_dominoes { get; set; }
        
        [NonSerialized()]
        private WriteableBitmap m_img;
        public WriteableBitmap img
        {
            get
            {
                return m_img;
            }
            set
            {
                m_img = value;
                OnPropertyChanged("img");
            }
        }

        public int m_ResizeMode;

        public int ResizeMode
        {
            get
            {
                return m_ResizeMode;
            }
            set { m_ResizeMode = value; OnPropertyChanged(null); }
        }

        private int m_count;

        public int count
        {
            get
            {
                return m_count;
            }
            set
            {
                m_count = value;
                OnPropertyChanged(null);
            }
        }

        public override int length
        {
            get
            {
                return m_length;
            }
            set
            {
                int temp = (int)Math.Round((double)(value * (a + b) / (c + d) * br / l));
                if (temp > 0 && value > 0)
                {
                    m_length = value;
                    m_height = temp;
                    count = height * value;
                    OnPropertyChanged(null);
                }
            }
        }
        
        public override int height
        {
            get
            {
                return m_height;
            }
            set
            {
                int temp = (int)Math.Round((double)(value * (c + d) / (a + b) * l / br));
                if (temp > 0 && value > 0)
                {
                    m_height = value;
                    m_length = temp;
                    count = length * value;
                    OnPropertyChanged(null);
                }
            }
        }
        private ImageEditingDocument _filters;
        public override ImageEditingDocument filters
        {
            get
            {
                return _filters;
            }
            set
            {
                _filters = filters;
                OnPropertyChanged(null);
            }
        }
        private int m_a;

        public int a
        {
            get
            {
                return m_a;
            }
            set
            {
                m_a = value;
                OnPropertyChanged(null);
            }
        }

        public bool? m_horizontal;

        public bool? horizontal
        {
            get
            {
                return m_horizontal;
            }
            set
            {

                if (m_horizontal != value.Value)
                {
                    int temp = b;
                    m_b = c;
                    m_c = temp;
                    m_length = (int)Math.Round((double)(m_height * (c + d) / (a + b) * l / br));
                    m_count = m_length * m_height;

                    m_horizontal = value.Value;
                }
                OnPropertyChanged(null);
            }
        }

        private int m_b;

        public int b
        {
            get
            {
                return m_b;
            }
            set
            {
                m_b = value;
                OnPropertyChanged(null);
            }
        }

        private int m_c;

        public int c
        {
            get
            {
                return m_c;
            }
            set
            {
                m_c = value;
                OnPropertyChanged(null);
            }
        }

        private int m_d;

        public int d
        {
            get
            {
                return m_d;
            }
            set
            {
                m_d = value;
                OnPropertyChanged(null);
            }
        }


        public override List<DominoColor> Colors
        {
            get
            {
                return m_Colors;
            }
            set
            {
                // check if redraw is necessary (only when colors were changed)
                if (m_Colors != null)
                {
                    for (int i = 0; i < m_Colors.Count && i < value.Count; i++)
                    {
                        if ((value as List<DominoColor>)[i].rgb != m_Colors[i].rgb)
                        {
                            goto flag;
                        }
                    }
                }
                else { goto flag; }
                return;
                flag:
                m_Colors = value;
                OnPropertyChanged("Colors");
            }
        }

        private bool m_ShowSpaces;

        public bool ShowSpaces
        {
            get
            {
                return m_ShowSpaces;
            }
            set
            {
                m_ShowSpaces = value;
                OnPropertyChanged("DrawOnly");
            }
        }

        private int m_ColorRegressionMode;

        public int ColorRegressionMode
        {
            get
            {
                return m_ColorRegressionMode;
            }
            set
            {
                m_ColorRegressionMode = value;
                OnPropertyChanged(null);
            }
        }

        private int m_DiffusionMode;

        public int DiffusionMode
        {
            get
            {
                return m_DiffusionMode;
            }
            set
            {
                m_DiffusionMode = value;
                OnPropertyChanged(null);
            }
        }
        #endregion

        private int l;
        private int br;

        public FieldDocument()
        { }

        public override void OnPropertyChanged(String property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
                if (property == "Filter" || property == null)
                {
                    if (Locked != Visibility.Visible)
                    {
                        GenerateField(property == "Filter");
                    }
                    img = DrawField();
                }
                if (property == "DrawOnly")
                {
                    img = DrawField();
                }

                
            }
        }
        public FieldDocument(String path, String Source, List<DominoColor> cls, int width) : this(path, Source, cls, width, 8, 8, 24, 8) { }
        public FieldDocument(String path, String Source, List<DominoColor> cls, int width, int a, int b, int c, int d)
        {
            t = type.fld;
            this.path = path;
            this.filename = System.IO.Path.GetFileName(path);
            SourcePath = Source;
            Locked = Visibility.Hidden;
            ResizeMode = 2;
            m_ColorRegressionMode = 3;
            m_DiffusionMode = 0;
            m_Colors = cls;
            m_a = a;
            m_b = b;
            m_c = c;
            m_d = d;
            System.Drawing.Image i = InitFilters();
            l = i.Width;
            br = i.Height;
            m_length = width;
            m_height = (int)Math.Round((double)(m_length * (a + b) / (c + d) * br / l));
            count = length * height;
            m_horizontal = true;
            GenerateField();
            m_ShowSpaces = false;
            img = DrawField();
        }

        #region Draw and Generate Field
        public void GenerateField(bool update_filters = false)
        {
            dominoes = new int[length, height];
            System.Drawing.Image i;
            if (!update_filters)
            {
                using (var bmpTemp = new System.Drawing.Bitmap(SourcePath))
                {
                    i = new System.Drawing.Bitmap(bmpTemp);
                }
            }
            else
            {
                i = ImageHelper.BitmapImageToBitmap(filters.preview);
            }
            if (length < 2) m_length = 2;
            if (height < 2) m_height = 2;
            System.Drawing.Bitmap thumb = new System.Drawing.Bitmap(length, height);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(thumb);
            graphics.Clear(System.Drawing.Color.White);
            if (ResizeMode == 0) graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            if (ResizeMode == 1) graphics.InterpolationMode = InterpolationMode.Bicubic;
            if (ResizeMode == 2) graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            System.Drawing.Imaging.ImageAttributes attr = new System.Drawing.Imaging.ImageAttributes();
            System.Drawing.Imaging.ImageAttributes Att = new System.Drawing.Imaging.ImageAttributes();
            Att.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
            graphics.DrawImage(i, new System.Drawing.Rectangle(0, 0, length, height), 0, 0, i.Width, i.Height, System.Drawing.GraphicsUnit.Pixel, Att);
            //graphics.DrawImage(i, 0, 0, length, height);
            BitmapImage bitmapImage = ImageHelper.BitmapToBitmapImage(thumb);
            BitmapSource bitm = new FormatConvertedBitmap(bitmapImage, PixelFormats.Bgr24, null, 0);
            WriteableBitmap b = BitmapFactory.ConvertToPbgra32Format(bitm);

            IColorSpaceComparison comp;
            switch (ColorRegressionMode)
            {
                case 0: comp = new CmcComparison(); break;
                case 1: comp = new Cie1976Comparison(); break;
                case 2: comp = new Cie94Comparison(); break;
                default: comp = new CieDe2000Comparison(); break;
            }

            Dithering d;
            switch (DiffusionMode)
            {
                case 0: d = new NoDithering(comp, Colors); break;
                case 1: d = new FloydSteinbergDithering(comp, Colors); break;
                case 2: d = new JarvisJudiceNinkeDithering(comp, Colors); break;
                default: d = new StuckiDithering(comp, Colors); break;
            }
            dominoes = d.Dither(b, comp);

            b.Lock();
        }

        internal void ChangeSize(int length, int height)
        {
            this.m_height = height;
            this.m_length = length;
            this.m_count = height * length;
            OnPropertyChanged(null);
        }

        public WriteableBitmap DrawField()
        {
            WriteableBitmap bitmap;
            if (!ShowSpaces)
            {
               double ScalingFactor = ggt(a + b, c + d);
                
                if ((length + 1) * (a + b) / ScalingFactor < 1000)
                {
                    ScalingFactor = (length + 1) * (a + b) / 1000d;
                }
                else if ((height + 1) * ((c + d) / ScalingFactor) < 1000)
                {
                    ScalingFactor = (height + 1) * ((c + d) / 1000d);
                }
                int sum1 = (int)((a + b) / ScalingFactor);
                int sum2 = (int)((c + d) / ScalingFactor);
                bitmap = BitmapFactory.New((length + 1) * sum1, (height + 1) * sum2);
                for (int x = 0; x < length; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color cr = Colors[dominoes[x, y]].rgb;
                        bitmap.FillRectangle(x * sum1, y * sum2, sum1 * (x + 1), sum2 * (y + 1), Color.FromRgb(cr.R, cr.G, cr.B));
                    }
                }
            }
            else
            {
                int ScalingFactor = ggt(ggt(ggt(a, b), c), d);
                int a_scaled = a / ScalingFactor;
                int b_scaled = b / ScalingFactor;
                int c_scaled = c / ScalingFactor;
                int d_scaled = d / ScalingFactor;

                if (((length) * (a_scaled + b_scaled)) < 1000)
                {
                    ScalingFactor = (1000 / ((length) * (a_scaled + b_scaled)));
                    a_scaled = a_scaled * ScalingFactor;
                    b_scaled = b_scaled * ScalingFactor;
                    c_scaled = c_scaled * ScalingFactor;
                    d_scaled = d_scaled * ScalingFactor;
                }
                else if (((height) * (c_scaled + d_scaled) < 1000))
                {
                    ScalingFactor = (1000 / ((height) * (c_scaled + d_scaled)));
                    a_scaled = a_scaled * ScalingFactor;
                    b_scaled = b_scaled * ScalingFactor;
                    c_scaled = c_scaled * ScalingFactor;
                    d_scaled = d_scaled * ScalingFactor;
                }

                bitmap = BitmapFactory.New((length) * (a_scaled + b_scaled), (height) * (c_scaled + d_scaled));
                for (int x = 0; x < length; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Color cr = Colors[dominoes[x, y]].rgb;
                        bitmap.FillRectangle(x * (a_scaled + b_scaled), y * (c_scaled + d_scaled),
                            x * (a_scaled + b_scaled) + b_scaled, (c_scaled + d_scaled) * y + c_scaled, Color.FromRgb(cr.R, cr.G, cr.B));
                    }
                }
            }
            return bitmap;
        }

        public int ggt(int a, int b)
        {
            int temp;
            while (a % b != 0)
            {
                temp = a % b;
                a = b;
                b = temp;
            }
            return b;
        }

        public override void Save(String path)
        {
            // fill colors
            ColorList = new List<String>();
            for (int i = 0; i < Colors.Count; i++)
            {
                Color c = Colors[i].rgb;
                ColorList.Add(c.R + "\r" + c.G + "\r" + c.B + "\r" + Colors[i].name + "\r" + Colors[i].count);
            }
            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter serialisierer = new BinaryFormatter();
                serialisierer.Serialize(fs, this);
            }

        }
        public static FieldDocument LoadFieldDocument(string path)
        {
            FieldDocument feld = LoadFieldDocumentWithoutDrawing(path);
            if (feld.filters == null)
            {
                feld.InitFilters();
            }
            feld.filters.Deserialize_Finish(feld.SourcePath);
            feld.img = feld.DrawField();
            return feld;
        }
        public static FieldDocument LoadFieldDocumentWithoutDrawing(string path)
        {
            FieldDocument feld;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter serialisierer = new BinaryFormatter();
                feld = (FieldDocument)serialisierer.Deserialize(fs);
            }
            feld.Colors = new List<DominoColor>();
            feld.UsedColors = new ObservableCollection<DominoColor>();
            bool flag = false;
            if (feld.ColorList[0].IndexOf('\r') > -1)
            {
                flag = true;
            }
            for (int i = 0; i < feld.ColorList.Count; i++)
            {

                String[] values = feld.ColorList[i].Split(flag ? '\r' : ' ');

                feld.Colors.Add(new DominoColor(values[3], Color.FromRgb(byte.Parse(values[0]), byte.Parse(values[1]), byte.Parse(values[2])), int.Parse(values[4])));
            }
            feld.path = path;
            feld.filename = Path.GetFileName(path);
            if (feld.SourcePath == null || feld.SourcePath == "")
            {
                String imagepath = Path.Combine(Path.GetDirectoryName(path), "Source Images");
                feld.SourcePath = Directory.GetFiles(imagepath, Path.GetFileNameWithoutExtension(path) + "*")[0];
            }
            
            return feld;
        }
        public System.Drawing.Image InitFilters()
        {
            System.Drawing.Image i;
            using (var bmpTemp = new System.Drawing.Bitmap(SourcePath))
            {
                i = new System.Drawing.Bitmap(bmpTemp);
            }
            _filters = new ImageEditingDocument(SourcePath);
            _filters.t = type.img;
            _filters.path = this.SourcePath;
            _filters.filename = Path.GetFileName(SourcePath);
            _filters.parent = this;
            return i;
        }
        public override bool Compare(Document d)
        {
            if (d is FieldDocument)
            {
                FieldDocument doc = d as FieldDocument;
                if (!(doc.dominoes.Rank == this.dominoes.Rank &&
                    Enumerable.Range(0, doc.dominoes.Rank).All(dimension => doc.dominoes.GetLength(dimension) == this.dominoes.GetLength(dimension)) &&
                    doc.dominoes.Cast<int>().SequenceEqual(this.dominoes.Cast<int>())))
                {
                    return false;
                }
                if (doc.a != this.a || doc.b != this.b || doc.d != this.d || doc.c != this.c)
                {
                    return false;
                }
                if (doc.ResizeMode != this.ResizeMode || doc.ColorRegressionMode != this.ColorRegressionMode || doc.DiffusionMode != this.DiffusionMode)
                {
                    return false;
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
                if (doc.Locked != this.Locked)
                    return false;
                return true;

            }
            return false;
        }
        public override void SavePNG()
        {
            img = DrawField();
            SaveFileDialog ofd = new SaveFileDialog();
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
                            encoder.Frames.Add(BitmapFrame.Create(img.Clone()));
                            encoder.Save(stream);
                            stream.Close();
                        }
                    }
                    catch
                    {
                        MessageBox.Show("File not saved. Check if the path isn't locked by another application.");
                    }
                }
            }

        }
        #endregion

        internal void CalculateUsedDominoes()
        {
            used_dominoes = new int[Colors.Count];
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    used_dominoes[dominoes[i, j]]++;
                }
            }
        }

    }
}
