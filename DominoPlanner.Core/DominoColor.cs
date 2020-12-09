using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using ProtoBuf;
using System.IO;
using System.ComponentModel;
using SkiaSharp;

namespace DominoPlanner.Core
{
    [ProtoContract]
    [ProtoInclude(500, typeof(DominoColor))]
    public abstract class IDominoColor
    {
        [ProtoMember(3, DataFormat = DataFormat.FixedSize)]
        private String ColorSerialized
        {
            get
            {
                return mediaColor.ToString();

            }
            set { mediaColor = Color.Parse(value); }
        }
        internal Lab labColor;
        public abstract double distance(SKColor color, IColorComparison comp, byte transparencyThreshold);
        private Color _mediacolor;
        [DisplayName ("Color")]
        public Color mediaColor
        {
            get { return _mediacolor; }
            set { _mediacolor = value; labColor = value.ToLab(); MediaColorChanged(); }
        }

        internal virtual void MediaColorChanged()
        {
            PropertyChanged?.Invoke(this, "mediaColor");
        }

        private int _count;
        [ProtoMember(2)]
        [DisplayName("Count")]
        public virtual int count
        {
            get { return _count; }
            set
            {
                if (_count != value)
                {
                    _count = value;
                    PropertyChanged?.Invoke(this, "count");
                }
            }
        }

        private string _name;
        [ProtoMember(1)]
        [DisplayName("Name")]
        public string name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    PropertyChanged?.Invoke(this, "name");
                }
            }
        }
        [Browsable(false)]
        public virtual bool show { get { return count != 0; } }

        [ProtoMember(4)]
        public bool Deleted { get; set; }

        public abstract XElement Save();

        public event EventHandler<string> PropertyChanged;
    }
    [ProtoContract]
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
        public override double distance(SKColor color, IColorComparison comp, byte transparencyThreshold)
        {
            if (color.Alpha < transparencyThreshold)
                return 0;
            else return Int32.MaxValue;
        }
        public override XElement Save()
        {
            return new XElement("EmptyDomino",
                new XAttribute("name", name));
        }
        public EmptyDomino()
        {
            mediaColor = Colors.Transparent;
            name = "[empty]";
        }
    }
    [ProtoContract(SkipConstructor = true)]
    public class DominoColor : IDominoColor
    {
        public override double distance(SKColor color, IColorComparison comp, byte transparencyThreshold)
        {
            if (count == 0 || Deleted) // if count is zero or the color was deleted, don't use for computation
                return Int32.MaxValue;
            return comp.Distance(color.SKToLab(), labColor);
        }
        public DominoColor(XElement source, int old_version)
        {
            if (old_version == 1)
            {
                mediaColor = Color.FromRgb(
                    byte.Parse(source.Element("r").Value),
                byte.Parse(source.Element("g").Value),
                byte.Parse(source.Element("b").Value));
                count = int.Parse(source.Element("Anzahl").Value);
                name = source.Element("Farbname").Value;
                labColor = mediaColor.ToLab();
            }
            else if (old_version == 2)
            {
                var color = source.Element("rgb");
                mediaColor = Color.FromRgb(
                    byte.Parse(color.Element("R").Value),
                byte.Parse(color.Element("G").Value),
                byte.Parse(color.Element("B").Value));
                count = int.Parse(source.Element("count").Value);
                name = source.Element("name").Value;
                labColor = mediaColor.ToLab();
            }
            else throw new ArgumentException("Version of old color list must be 1 or 2");
        }
        internal override void MediaColorChanged()
        {
            base.MediaColorChanged();
            if(mediaColor.A != 255)
            {
                mediaColor = Color.FromRgb(mediaColor.R, mediaColor.G, mediaColor.B);
            }
        }
        public DominoColor(Color c, int count, string name)
        {
            labColor = c.ToLab();
            this.count = count;
            this.name = name;
            mediaColor = c;
        }
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
    [ProtoContract(SkipConstructor = true)]
    public class ColorRepository : IWorkspaceLoadable
    {
        [ProtoMember(3)]
        public ObservableCollection<int> Anzeigeindizes;
        [ProtoMember(2)]
        public List<DominoColor> colors; //todo - nur vorrübergehend public
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
            Anzeigeindizes.Add((Anzeigeindizes.Count == 0) ? 0 : Anzeigeindizes.Max() + 1);
        }
        public ColorRepository()
        {
            first = new EmptyDomino();
            Anzeigeindizes = new ObservableCollection<int>();
            colors = new List<DominoColor>();
        }
        public void MoveUp(DominoColor color)
        {
            int index = IndexOf(color);
            int anzeigeindex = Anzeigeindizes[index];
            if (anzeigeindex == 0) throw new InvalidOperationException("Die Farbe ist bereits ganz oben");
            int position_neuer_index = Anzeigeindizes.IndexOf(anzeigeindex - 1);
            if (position_neuer_index != -1)
            {
                // otherwise, the index is not currently in use
                Anzeigeindizes[position_neuer_index]++;
            }
            Anzeigeindizes[index]--;
        }
        public void MoveDown(DominoColor color)
        {
            int index = IndexOf(color);
            int anzeigeindex = Anzeigeindizes[index];
            if (anzeigeindex == Anzeigeindizes.Max()) throw new InvalidOperationException("Die Farbe ist bereits ganz unten");
            int position_neuer_index = Anzeigeindizes.IndexOf(anzeigeindex + 1);
            if (position_neuer_index != -1)
            {
                // otherwise, the index is not currently in use
                Anzeigeindizes[position_neuer_index]--;
            }
            Anzeigeindizes[index]++;
        }
        public IEnumerable<IDominoColor> SortedRepresentation
        {
            get
            {
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
        public void Save(string path)
        {
            Workspace.Save(this, path);
        }
        public ColorRepository(string absolutePath) : this()
        {
            var doc = XDocument.Load(absolutePath);
            if (doc.Element("ColorArrayDocument") != null)
            {
                foreach (XElement x in doc.Descendants("DominoColor"))
                {
                    this.Add(new DominoColor(x, 2));
                }
            }
            else if (doc.Element("Farbenarray") != null)
            {
                foreach (XElement x in doc.Descendants("Farbe"))
                {
                    this.Add(new DominoColor(x, 1));
                }
            }
            else throw new ArgumentException("Format of color repository unknown");
        }
    }
}

