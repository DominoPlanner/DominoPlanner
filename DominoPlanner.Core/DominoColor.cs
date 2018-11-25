using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using DominoPlanner.Core.Dithering;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using ProtoBuf;
using System.IO;

namespace DominoPlanner.Core
{
    [ProtoContract]
    [ProtoInclude(500, typeof(DominoColor))]
    public abstract class IDominoColor
    {
        [ProtoMember(3, DataFormat = DataFormat.FixedSize)]
        private String ColorSerialized
        {
            get {
                return mediaColor.ToString();
                
            }
            set { mediaColor = (Color)ColorConverter.ConvertFromString(value);}
        }
        internal Emgu.CV.Structure.Lab labColor;
        public abstract double distance(Emgu.CV.Structure.Bgra color, IColorComparison comp);
        private Color _mediacolor;
        public Color mediaColor { get { return _mediacolor; }
            set { _mediacolor = value; labColor = value.ToLab(); } }
        [ProtoMember(2)]
        public virtual int count { get; set; }
        [ProtoMember(1)]
        public string name { get; set; }
        public virtual bool show { get { return count != 0; } }
        public abstract XElement Save();
    }
    [ProtoContract]
    [ProtoInclude(500, typeof(DominoColor))]
    public class EmptyDomino : IDominoColor
    {
        public override int count
        {
            get { return int.MaxValue; }
        }
        public override bool show
        {
            get { return true; }
        }
        public override double distance(Emgu.CV.Structure.Bgra color, IColorComparison comp)
        {
            if (color.Alpha < 128) return 0;
            else return Int32.MaxValue;
        }
        public override XElement Save()
        {
            return new XElement("EmptyDomino",
                new XAttribute("name", name));
        }
        public EmptyDomino()
        {
            mediaColor = System.Windows.Media.Colors.Transparent;
            name = "[empty]";
        }
        
        
    }
    [ProtoContract]
    public class DominoColor : IDominoColor
    {
        public override double distance(Emgu.CV.Structure.Bgra color, IColorComparison comp)
        {
            if (count == 0) return Int32.MaxValue;
            return comp.Distance(color.ToLab(), labColor);
        }
        public DominoColor(XElement source)
        {
            mediaColor = Color.FromRgb(
                byte.Parse(source.Element("R").Value),
            byte.Parse(source.Element("G").Value),
            byte.Parse(source.Element("B").Value));
            count = int.Parse(source.Element("Count").Value);
            name = source.Element("Name").Value;
            labColor = mediaColor.ToLab();
        }
        public DominoColor(Color c, int count, string name)
        {
            labColor = c.ToLab();
            this.count = count;
            this.name = name;
            mediaColor = c;
        }
        private DominoColor() { }
        public override XElement Save()
        {
            return new XElement("DominoColor",
                new XAttribute("name", name),
                new XAttribute("count", count),
                new XAttribute("r", mediaColor.R),
                new XAttribute("g", mediaColor.G),
                new XAttribute("b", mediaColor.B));
        }
    }
    [ProtoContract]
    public class ColorRepository
    {
        [ProtoMember(3)]
        public List<int> Anzeigeindizes;
        [ProtoMember(2)]
        private List<DominoColor> colors;
        [ProtoMember(1)]
        private EmptyDomino first;
        public int Length
        {
            get { return colors.Count + 1; }
        }
        public IDominoColor this[int index]
        {
            get
            {
                if (index == 0) return first; return colors[index - 1];
            }
            set
            {
                if (!(value is DominoColor)) throw new ArgumentException("nur DominoColors dürfen gesetzt werden");
                if (index == 0) throw new IndexOutOfRangeException("das erste Element ist die leere Farbe, darf nicht verändert werden");
                if (index > colors.Count) throw new IndexOutOfRangeException();
                else if (index != 0 && index <= colors.Count) colors[index - 1] = (DominoColor)value;
            }
        }
        public int IndexOf(DominoColor color)
        {
            return colors.IndexOf(color);
        }
        public void Add(DominoColor color)
        {
            colors.Add(color);
            Anzeigeindizes.Add((Anzeigeindizes.Count == 0) ? 0 : Anzeigeindizes.Max() +1);
        }
        public ColorRepository()
        {
            first = new EmptyDomino();
            Anzeigeindizes = new List<int>();
            colors = new List<DominoColor>();
        }
        public void MoveUp(DominoColor color)
        {
            int index = IndexOf(color);
            int anzeigeindex = Anzeigeindizes[index];
            if (anzeigeindex == 0) throw new InvalidOperationException("Die Farbe ist bereits ganz oben");
            int position_neuer_index = Anzeigeindizes.IndexOf(anzeigeindex - 1);
            Anzeigeindizes[position_neuer_index]++;
            Anzeigeindizes[index]--;
        }
        public void MoveDown(DominoColor color)
        {
            int index = IndexOf(color);
            int anzeigeindex = Anzeigeindizes[index];
            if (anzeigeindex == Anzeigeindizes.Max()) throw new InvalidOperationException("Die Farbe ist bereits ganz unten");
            int position_neuer_index = Anzeigeindizes.IndexOf(anzeigeindex + 1);
            Anzeigeindizes[position_neuer_index]--;
            Anzeigeindizes[index]++;
        }
        public IEnumerable<IDominoColor> SortedRepresentation
        {
            get {
                var list = new List<IDominoColor>();
                list.Add(first);
                list.AddRange(colors);
                List<Tuple<IDominoColor, int>> joined = new List<Tuple<IDominoColor, int>>();
                joined.Add(new Tuple<IDominoColor, int>(first, -1));
                joined.AddRange(colors.Zip(Anzeigeindizes, (a, b) => new Tuple<IDominoColor, int>(a, b)));
                joined = joined.OrderBy(a => a.Item2).ToList();
                return joined.Select(x => x.Item1);
            }
        }
        public IDominoColor[] RepresentionForCalculation
        {
            get
            {
                var list = new List<IDominoColor>();
                list.Add(first);
                list.AddRange(colors);
                return list.ToArray();
            }
        }
        
        public static ColorRepository Load(string path)
        {
            path = Workspace.Instance.MakePathAbsolute(path);
            var open = (ColorRepository)Workspace.Instance.Find(path);
            if (open == null)
            {
                
                Console.WriteLine($"Datei {path} öffnen");
                /*var xml = XDocument.Load(path);
                var root = xml.Descendants("ColorRepository").First();
                string version = root.Attribute("version").Value;
                if (version == "1.0")
                {
                    var empty = new EmptyDomino()
                    { name = root.Descendants("EmptyDomino").First().Attribute("name").Value };
                    var result = root.Descendants("DominoColor")
                        .Select(x => new Tuple<DominoColor, int>(new DominoColor(
                        Color.FromArgb(255, byte.Parse(x.Attribute("r").Value), byte.Parse(x.Attribute("g").Value),
                        byte.Parse(x.Attribute("b").Value)),
                        int.Parse(x.Attribute("count").Value), x.Attribute("name").Value), int.Parse(x.Attribute("index").Value)));
                    var repo = new ColorRepository();
                    repo.colors = result.Select(x => x.Item1).ToList();
                    repo.Anzeigeindizes = result.Select(x => x.Item2).ToList();
                    repo.first = empty;
                    Workspace.Instance.AddToWorkspace(path, repo);
                    return repo;
                }
                throw new InvalidOperationException("Version der Farbdatei nicht unterstützt");
                */
                ColorRepository repo;
                
                using (var file = File.OpenRead(path))
                {
                    repo = Serializer.Deserialize<ColorRepository>(file);
                }
                Workspace.Instance.AddToWorkspace(path, repo);
                return repo;
            }
            else
            {
                Console.WriteLine($"Datei {path} bereits geöffnet");
                return open;
            }
            
        }
        public void Save(string path)
        {
            
            path = Workspace.Instance.MakePathAbsolute(path);
            /*var xml = new XDocument();
            var root = new XElement("ColorRepository", new XAttribute("version", "1.0"));
            xml.Add(root);
            for (int i = 0; i <= colors.Count; i++)
            {
                var current = this[i].Save();
                current.Add(new XAttribute("index", Anzeigeindizes[i]));
                root.Add(current);
            }
            xml.Save(path);
            */
            using (var file = new FileStream(path, FileMode.Create))
            {
                Serializer.Serialize<ColorRepository>(file, this);
            }
        }
    }
}

