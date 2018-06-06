using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Xml.Serialization;

namespace DominoPlanner.Document_Classes
{

    public class ColorArrayDocument : Document
    {
        public List<DominoColor> cols { get; set; }
        

        public ColorArrayDocument()
        {

        }

        public ColorArrayDocument(String path)
        {
            this.t = type.clr;
            this.path = path;
            this.filename = System.IO.Path.GetFileName(path);
            cols = new List<DominoColor>();
            cols.Add(new DominoColor("black", 1000, 12, 12, 12));
            cols.Add(new DominoColor("dark_grey", 1000, 46, 48, 49));
            cols.Add(new DominoColor("grey", 1000, 151, 159, 160));
            cols.Add(new DominoColor("brown", 1000, 65, 36, 30));
            cols.Add(new DominoColor("reddish_brown", 1000, 106, 38, 35));
            cols.Add(new DominoColor("dark_violet", 1000, 82, 68, 83));
            cols.Add(new DominoColor("violet", 1000, 114, 104, 157));
            cols.Add(new DominoColor("purple", 1000, 164, 86, 148));
            cols.Add(new DominoColor("light_violet", 1000, 191, 156, 189));
            cols.Add(new DominoColor("dark_blue", 1000, 19, 46, 101));
            cols.Add(new DominoColor("royal_blue", 1000, 12, 91, 168));
            cols.Add(new DominoColor("light_blue", 1000, 93, 179, 238));
            cols.Add(new DominoColor("turquoise", 1000, 71, 181, 168));
            cols.Add(new DominoColor("dark_green", 1000, 34, 90, 70));
            cols.Add(new DominoColor("green", 1000, 59, 184, 103));
            cols.Add(new DominoColor("light_green", 1000, 102, 189, 95));
            cols.Add(new DominoColor("screaming_green", 1000, 29, 220, 7));
            cols.Add(new DominoColor("light_yellow", 1000, 240, 240, 14));
            cols.Add(new DominoColor("maize_yellow", 1000, 232, 186, 66));
            cols.Add(new DominoColor("orange", 1000, 229, 123, 63));
            cols.Add(new DominoColor("red", 1000, 236, 53, 57));
            cols.Add(new DominoColor("claret", 1000, 99, 45, 45));
            cols.Add(new DominoColor("skin-colored", 1000, 221, 166, 185));
            cols.Add(new DominoColor("ivory", 1000, 241, 231, 206));
            cols.Add(new DominoColor("white", 1000, 230, 230, 230));
            cols.Add(new DominoColor("gold", 1000, 122, 106, 80));
            cols.Add(new DominoColor("transparent", 1000, 200, 200, 200));
        }

        public void Add()
        {
        }

        public void Remove(DominoColor color)
        {
            cols.Remove(color);
        }

        public override void Save(String path)
        {
            try
            {
                XmlSerializer xmls = new XmlSerializer(typeof(ColorArrayDocument));
                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write)) { xmls.Serialize(fs, this); }
            }
            catch
            { MessageBox.Show("Error"); }
        }
        
        public static Document LoadColorArray(String path)
        {
            try
            {
                ColorArrayDocument clr;
                XmlSerializer xmls = new XmlSerializer(typeof(ColorArrayDocument));
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) { clr = (ColorArrayDocument)xmls.Deserialize(fs); }
                clr.path = path;
                return clr;
            }
            catch
            {
                MessageBox.Show("Aus irgend einem Grund ist ein Fehler aufgetreten.");
                return new ColorArrayDocument(path);
            }
        }
        public override bool Compare(Document d)
        {
            if (d is ColorArrayDocument)
            {
                ColorArrayDocument old = d as ColorArrayDocument;
                return CompareLists(this.cols, old.cols);
            }
            return false;
        }
        public static bool CompareLists(List<DominoColor> l1, List<DominoColor> l2)
        {
            if (l1.Count != l2.Count)
                return false;
            for (int i = 0; i < l1.Count; i++)
            {
                if (l1[i].rgb != l2[i].rgb)
                    return false;
            }
            return true;
        }
        public override void SavePNG()
        {
            //do nothing
        }

    }
    [Serializable()]
    public class DominoColor
    {
        private byte r;
        private byte g;
        private byte b;
        public Color rgb
        {
            get
            {
                return Color.FromRgb(r, g, b);
            }
            set
            {
                r = value.R;
                g = value.G;
                b = value.B;
            }
        }

        public String name { get; set; }

        public int count { get; set; }

        public int remaining { get; set; }

        [XmlIgnore()]
        private List<int> m_used;

        [XmlIgnore()]
        public List<int> used_in_projects {
            get
            {
                return m_used;
            }
            set
            {
                m_used = value;
            }
        }

        public DominoColor()
        {
            used_in_projects = new List<int>();
        }

        public DominoColor(String name, Color c, int count)
        {
            this.name = name;
            this.rgb = c;
            this.count = count;
            used_in_projects = new List<int>();
        }

        public DominoColor(String name, int count, byte r, byte g, byte b)
        {
            this.name = name;
            this.rgb = Color.FromRgb(r, g, b);
            this.count = count;
            used_in_projects = new List<int>();
        }

        public void CalcRemaining()
        {
            remaining = count;
            foreach(int tmpCount in used_in_projects)
            {
                remaining -= tmpCount;
            }
        }

        public override string ToString()
        {
            return String.Format("{0}", name);
            //return String.Format("{0} - {1}", name, count);
        }
    }
}
