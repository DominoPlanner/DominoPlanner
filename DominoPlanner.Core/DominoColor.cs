using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ColorMine.ColorSpaces;
using DominoPlanner.Core.Dithering;
using System.Xml.Linq;
using ColorMine.ColorSpaces.Comparisons;
using Emgu.CV.Structure;

namespace DominoPlanner.Core
{
    /*public class DominoColor : IEquatable<DominoColor>
    {

        internal Lab labColor;
        /// <summary>
        /// Die Anzahl der Farbe.
        /// </summary>
        public int count
        {
            get; set;
        }
        /// <summary>
        /// Der Name der Farbe.
        /// </summary>
        public string name
        {
            get; set;
        }
        /// <summary>
        /// Die Farbe, die zugrunde liegen soll.
        /// </summary>
        public Color mediaColor { get; private set; }
        /// <summary>
        /// Erstellt eine DominoColor mit den angegebenen Parametern.
        /// </summary>
        /// <param name="c">Die Farbe, die die DominoColor haben soll.</param>
        /// <param name="count">Die Anzahl der Farbe</param>
        /// <param name="name">Der Name der Farbe</param>
        public DominoColor(Color c, int count, string name)
        {
            labColor = c.ToLab();
            this.count = count;
            this.name = name;
            mediaColor = c;
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
        public XElement Save()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Vergleicht zwei Farben auf Gleichheit. Die Anzahl wird hierbei nicht berücksichtigt.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(DominoColor other)
        {
            if (this.name != other.name) return false;
            if (this.mediaColor.R != other.mediaColor.R) return false;
            if (this.mediaColor.G != other.mediaColor.G) return false;
            if (this.mediaColor.B != other.mediaColor.B) return false;
            return true;
        }
    }*/
    public abstract class IDominoColor // in Ermangelung eines besseren Namens
    {
        internal Lab labColor;
        public abstract double distance(Color color, IColorSpaceComparison comp);
        public Color mediaColor { get; set; }
        public virtual int count { get; set; }
        public string name { get; set; }
        public int AnzeigeID { get; set; }

        public virtual bool show { get { return count != 0; } }
    }
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
        public override double distance(Color color, IColorSpaceComparison comp)
        {
            if (color.A == 0) return 0;
            else return Int32.MaxValue;
        }
        public EmptyDomino()
        {
            mediaColor = Colors.Transparent;
        }
        
    }
    public class DominoColor : IDominoColor
    {
        public override double distance(Color color, IColorSpaceComparison comp)
        {
            if (count == 0) return Int32.MaxValue;
            return comp.Compare(color.ToLab(), mediaColor.ToLab());
        }
    }
    public class ColorRepository
    {
        public List<int> Anzeigeindizes;
        private List<DominoColor> colors;
        private EmptyDomino first;
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
            return colors.IndexOf(color) + 1;
        }
        public void Add(DominoColor color)
        {
            colors.Add(color);
            Anzeigeindizes.Add(Anzeigeindizes.Max() + 1);
        }
        public ColorRepository()
        {
            first = new EmptyDomino();
            Anzeigeindizes = new List<int>();
            colors = new List<DominoColor>();
            Anzeigeindizes[0] = 0;
        }
        public void MoveUp(DominoColor color)
        {
            int index = IndexOf(color);
            int anzeigeindex = Anzeigeindizes[index];
            if (anzeigeindex == 1) throw new InvalidOperationException("Die Farbe ist bereits ganz oben");
            int position_neuer_index = Anzeigeindizes.IndexOf(anzeigeindex - 1);
            Anzeigeindizes[position_neuer_index]++;
            Anzeigeindizes[index]--;
        }
        public void MoveDown(DominoColor color)
        {
            int index = IndexOf(color);
            int anzeigeindex = Anzeigeindizes[index];
            if (anzeigeindex == Anzeigeindizes.Max()) throw new InvalidOperationException("Die Farbe ist bereits ganz unten");
            int position_neuer_index = Anzeigeindizes.IndexOf(anzeigeindex - 1);
            Anzeigeindizes[position_neuer_index]++;
            Anzeigeindizes[index]--;
        }
        
        
    }
}

