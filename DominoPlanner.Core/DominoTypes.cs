using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Stellt einen rechteckigen Stein bereit (nicht gedrehtes Rechteck!)
    /// Implementiert IDominoShape.
    /// </summary>
    [ProtoContract]
    public class RectangleDomino : IDominoShape
    {
        [ProtoMember(1)]
        public double x;
        [ProtoMember(2)]
        public double y;
        [ProtoMember(3)]
        public double width;
        [ProtoMember(4)]
        public double height;

        public override DominoRectangle GetContainer(double scaling_x, double scaling_y)
        {
            return new DominoRectangle() { width = this.width * scaling_x, height = this.height * scaling_y, x = this.x * scaling_x, y = this.y * scaling_y};
        }

        public override DominoPath GetPath(double scaling_x, double scaling_y)
        {
            return new DominoPath()
            {
                points = new Point[] {
                new Point(x * scaling_x, y * scaling_y),
                new Point((x + width) * scaling_x, y * scaling_y),
                new Point((x + width) * scaling_x, (y + height) * scaling_y),
                new Point(x * scaling_x, (y + height) * scaling_y)}
            };

        }

        public override bool IsInside(Point point, double scaling_x, double scaling_y)
        {
            if (point.X < x * scaling_x) return false;
            if (point.X > (x + width) * scaling_x) return false;
            if (point.Y < y * scaling_y) return false;
            if (point.Y > (y + height) * scaling_y) return false;
            return true;
        }

        public override bool Equals(IDominoShape other)
        {
            if (other is RectangleDomino)
            {
                RectangleDomino o = other as RectangleDomino;
                if (o.x == this.x && o.y == this.y && o.height == this.height && o.width == this.width) return true;
            }
            return false;
        }
        public RectangleDomino() { }
        public RectangleDomino(XElement domino)
        {
            x = double.Parse(domino.Attribute("x").Value, CultureInfo.InvariantCulture);
            y = double.Parse(domino.Attribute("y").Value, CultureInfo.InvariantCulture);
            width = double.Parse(domino.Attribute("Width").Value, CultureInfo.InvariantCulture);
            height = double.Parse(domino.Attribute("Height").Value, CultureInfo.InvariantCulture);
        }

        public override IDominoShape TransformDomino(double moveX, double moveY, int i, int j, int width, int height)
        {
            return new RectangleDomino()
            {
                x = this.x + moveX,
                y = this.y + moveY,
                width = this.width,
                height = this.height,
                position = TransformProtocol(i, j, width, height)
            };
        }
    }
    /// <summary>
    /// Stellt einen Dominostein beliebiger Form bereit, der durch Eckpunkte definiert wird.
    /// Implementiert IDominoShape.
    /// </summary>
    class PathDomino : IDominoShape
    {
        public Point[] points;
        public override bool Equals(IDominoShape other)
        {
            if (other is PathDomino)
            {
                PathDomino o = other as PathDomino;
                if (o.points.Length == this.points.Length)
                {
                    for (int i = 0; i < o.points.Length; i++)
                    {
                        if (!o.points[i].Equals(this.points[i])) return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public override DominoRectangle GetContainer(double scaling_x, double scaling_y)
        {
            double xmin = points.Min(p => p.X);
            double ymin = points.Min(p => p.Y);
            double xmax = points.Max(p => p.X);
            double ymax = points.Max(p => p.Y);
           
            return new DominoRectangle() { x = xmin * scaling_x, y = ymin * scaling_y, width = (xmax - xmin) * scaling_x, height = (ymax - ymin) * scaling_y };
        }

        public override DominoPath GetPath(double scaling_x, double scaling_y)
        {
            return new DominoPath()
            {
                points = this.points.Select(p => new Point(p.X * scaling_x, p.Y * scaling_y)).ToArray()
            };
        }

        public override bool IsInside(Point point, double scaling_x, double scaling_y)
        {
            int i, j;
            bool c = false;
            for (i = 0, j = points.Length - 1; i < points.Length; j = i++)
            {
                if (((points[i].Y * scaling_y > point.Y) != (points[j].Y * scaling_y > point.Y)) &&
                 (point.X < (points[j].X * scaling_x - points[i].X * scaling_x) *
                    (point.Y - points[i].Y * scaling_y) / (points[j].Y * scaling_y - points[i].Y * scaling_y) + points[i].X * scaling_x))
                    c = !c;
            }
            return c;
        }

        public override IDominoShape TransformDomino(double moveX, double moveY, int i, int j, int width, int height)
        {
            return new PathDomino()
            {
                points = this.points.Select(x => new Point(x.X + moveX, x.Y + moveY)).ToArray(),
                position = TransformProtocol(i, j, width, height)
            };
        }
        public PathDomino() { }
        public PathDomino(XElement domino)
        {
            IEnumerable<XElement> PointCollection = domino.Elements("Point");
            points = domino.Elements("Point").Select(p =>
                new Point(
                    double.Parse(p.Attribute("x").Value, CultureInfo.InvariantCulture),
                    double.Parse(p.Attribute("y").Value, CultureInfo.InvariantCulture))).ToArray();
        }
    }
    [ProtoContract]
    public class Point
    {
        [ProtoMember(1)]
        public double X { get; set; }
        [ProtoMember(2)]
        public double Y { get; set; }
        public Point(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public static implicit operator System.Windows.Point(Point v)
        {
            throw new NotImplementedException();
        }
    }
}