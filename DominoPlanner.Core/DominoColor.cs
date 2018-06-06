using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ColorMine.ColorSpaces;
using DominoPlanner.Core.Dithering;
using System.Xml.Linq;

namespace DominoPlanner.Core
{
    public class DominoColor : IEquatable<DominoColor>
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
        public string name {
            get; set; }
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
    }
}
