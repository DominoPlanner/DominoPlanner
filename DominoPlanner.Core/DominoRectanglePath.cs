using ClipperLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Eigene Datenstruktur zum Abspeichern von Rechtecken. Dies ist kein DominoShape, da es keine Protokolldefinition enthält.
    /// </summary>
    public class DominoRectangle
    {
        public double x;
        public double y;
        public double width;
        public double height;
        public int x2 {  get { return (int)(width + x); } }
        public int y2 { get { return (int)(height + y); } }
        public int x1 { get { return (int)(x); } }
        public int y1 { get { return (int)(y); } }

        public System.Windows.Rect getWPFRectangle()
        {
            return new System.Windows.Rect(x, y, width, height);
        }
    }
    /// <summary>
    /// Eigene Datenstruktur zum Abspeichern von Pfaden aus beliebigen Punkten.
    /// </summary>
    public class DominoPath
    {
        public System.Windows.Point[] points;
        public PointCollection getWPFPath()
        {
            var pointcol = new PointCollection();
            for (int i = 0; i < points.Length; i++)
            {
                pointcol.Add(points[i]);
            }
            return pointcol;
        }
        /// <summary>
        /// Gibt einen Pfad zurück, wie er für ein WriteableBitmap benötigt wird.
        /// </summary>
        /// <returns>den Pfad als Punkteliste, wobei der erste Punkt auch der letzte ist</returns>
        public int[] getWBXPath()
        {
            var array = new int[points.Length * 2+2];
            for (int i = 0; i < points.Length; i++)
            {
                array[i * 2] = (int)points[i].X;
                array[i * 2 + 1] = (int)points[i].Y;
            }
            array[points.Length*2] = (int)points[0].X;
            array[points.Length*2+1] = (int)points[0].Y;
            return array;
        }
        public DominoPath getOffsetRectangle(int offset)
        {
            List<IntPoint> intpoints = points.Select(p => new IntPoint(p.X, p.Y)).ToList();
            intpoints.Add(new IntPoint(points[0].X, points[0].Y));
            List<List<IntPoint>> solution = new List<List<IntPoint>>();
            ClipperOffset co = new ClipperOffset();
            co.AddPath(intpoints, JoinType.jtMiter, EndType.etClosedLine);
            co.Execute(ref solution, offset);
            if (solution.Count == 0)
                return this;
            return new DominoPath() { points = solution[0].Select(p => new System.Windows.Point(p.X, p.Y)).ToArray() };
        }
    }
}
