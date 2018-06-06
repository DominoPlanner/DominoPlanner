using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Util
{
    [Serializable()]
    public abstract class DominoDefinition
    {
        public DominoProtocolDefinition ProtocolDefinition;
        public bool hasProtocolDefinition {
            get { return (ProtocolDefinition != null); } }
        /// <summary>
        /// Verschiebt einen Stein an die angegebene Position. Optionale Parameter können leer gelassen werden, wenn das Protokoll bei der Operation nicht berücksichtigt werden soll.
        /// </summary>
        /// <param name="move_x">Der Wert, um den der Stein in x-Richtung verschoben werden soll.</param>
        /// <param name="move_y">Der Wert, um den der Stein in y-Richtung verschoben werden soll.</param>
        /// <param name="i">Strukturprotokoll: x-Koordinate der Zelle.</param>
        /// <param name="j">Strukturprotokoll: y-Koordinate der Zelle.</param>
        /// <param name="width">Strukturprotokoll: Anzahl der Mitte-Zellen in x-Richtung</param>
        /// <param name="height">Strukturprotokoll: Anzahl der Mitte-Zellen in y-Richtung</param>
        /// <returns>Die verschobene Definition.</returns>
        public abstract DominoDefinition TransformDefinition(float move_x, float move_y, int i=0, int j=0, int width=0, int height=0);
        /// <summary>
        /// Wendet die Verschiebung auf das Protokoll an.
        /// </summary>
        /// <param name="i">Strukturprotokoll: x-Koordinate der Zelle.</param>
        /// <param name="j">Strukturprotokoll: y-Koordinate der Zelle.</param>
        /// <param name="width">Strukturprotokoll: Anzahl der Mitte-Zellen in x-Richtung</param>
        /// <param name="height">Strukturprotokoll: Anzahl der Mitte-Zellen in y-Richtung</param>
        /// <returns>Das verschobene Protokoll.</returns>
        public DominoProtocolDefinition TransformProtocol(int i, int j, int width, int height)
        {
            if (hasProtocolDefinition)
            {
                return ProtocolDefinition.FinalizeProtocol(i, j, width, height);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Gibt ein Rechteck zurück, dass den Stein umschließt.
        /// </summary>
        /// <returns>Das Rechteck.</returns>
        public abstract RectangleF GetContainer(float scaling_x, float scaling_y);
        public RectangleF GetContainer(float scaling_factor = 1)
        {
            return GetContainer(scaling_factor, scaling_factor);
        }
        /// <summary>
        /// Wandelt den Stein in einen GraphicsPath um
        /// </summary>
        /// <param name="scaling_factor">Optional: Skalieren mit einem Faktor.</param>
        /// <returns>Der GraphicsPath.</returns>
        public GraphicsPath GetPath(float scaling_factor = 1)
        {
            return GetPath(scaling_factor, scaling_factor);
        }
        /// <summary>
        /// Wandelt den Stein in einen GraphicsPath um
        /// </summary>
        /// <param name="scaling_x">Skalieren mit einem Faktor in x-Richtung.</param>
        /// <param name="scaling_y">Skalieren mit einem Faktor in y-Richtung.</param>
        /// <returns>Der GraphicsPath.</returns>
        public abstract GraphicsPath GetPath(float scaling_x, float scaling_y);
        /// <summary>
        /// Lädt eine DominoDefinition aus einem XElement.
        /// </summary>
        /// <param name="domino"></param>
        /// <returns>Das XElement, aus dem geladen werden soll.</returns>
        public static DominoDefinition LoadDefinition(XElement domino)
        {
            DominoProtocolDefinition protocol = null;
            if (domino.Elements("ProtocolDefinition").Count() > 0) // Has protocol definition
            {
                XElement ProtocolDefinition = domino.Element("ProtocolDefinition");
                protocol = new DominoProtocolDefinition(ProtocolDefinition);
            }
            DominoDefinition dominoDefinition = 
                (domino.Name == "RectangleDomino") ? (DominoDefinition)(new RectangleDomino(domino)) : ((domino.Name == "PathDomino") ? new PathDomino(domino) : null);

            dominoDefinition.ProtocolDefinition = protocol;
            return dominoDefinition;
        }
        public bool IsInside(PointF point, bool ContainerChecked,  float scaling = 1)
        {
            return IsInside(point, ContainerChecked, scaling, scaling);
        }
        public virtual bool IsInside(PointF point, bool ContainerChecked, float scaling_x, float scaling_y)
        {
            if (ContainerChecked) return true;

            RectangleF container = GetContainer(scaling_x, scaling_y);
            if (container.Contains(point))
            {
                return true;
            }
            return false;
        }

        public abstract bool Compare(DominoDefinition dominoDefinition);
    }
    [Serializable()]
    public class RectangleDomino : DominoDefinition
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public RectangleDomino() { }
        public RectangleDomino(XElement domino)
        {
            x = float.Parse(domino.Attribute("x").Value, CultureInfo.InvariantCulture);
            y = float.Parse(domino.Attribute("y").Value, CultureInfo.InvariantCulture);
            width = float.Parse(domino.Attribute("Width").Value, CultureInfo.InvariantCulture);
            height = float.Parse(domino.Attribute("Height").Value, CultureInfo.InvariantCulture);
        }

        public override DominoDefinition TransformDefinition(float move_x, float move_y, int i, int j, int width, int height)
        {
            DominoDefinition d = new RectangleDomino()
            {
                x = this.x + move_x,
                y = this.y + move_y,
                width = this.width,
                height = this.height
            };
            d.ProtocolDefinition = TransformProtocol(i, j, width, height);
            return d;
        }

        public override RectangleF GetContainer(float scaling_x, float scaling_y)
        {
            return new RectangleF(x*scaling_x, y*scaling_y, width*scaling_x, height*scaling_y);
        }

        public override GraphicsPath GetPath(float scaling_x, float scaling_y)
        {
            GraphicsPath g = new GraphicsPath();
            g.AddRectangle(new RectangleF(x * scaling_x, y * scaling_y, width * scaling_x, height * scaling_y) );
            return g;
        }

        public override bool Compare(DominoDefinition dominoDefinition)
        {
            if ((dominoDefinition is RectangleDomino))
            {
                RectangleDomino d = dominoDefinition as RectangleDomino;
                if (this.x != d.x || this.y != d.y || this.width != d.width || this.height != d.height) return false;
                return true;
            }
            return false;
        }
    }
    [Serializable()]
    public class PathDomino : DominoDefinition
    {
        public float[] xCoordinates;
        public float[] yCoordinates;
        public PathDomino() { }
        public PathDomino(XElement domino)
        {
            IEnumerable<XElement> PointCollection = domino.Elements("Point");
            xCoordinates = new float[PointCollection.Count()];
            yCoordinates = new float[PointCollection.Count()];
            int counter = 0;
            foreach (XElement point in PointCollection)
            {
                xCoordinates[counter] = float.Parse(point.Attribute("x").Value, CultureInfo.InvariantCulture);
                yCoordinates[counter] = float.Parse(point.Attribute("y").Value, CultureInfo.InvariantCulture);
                counter++;
            }
        }
        public override DominoDefinition TransformDefinition(float move_x, float move_y, int i, int j, int width, int height)
        {
            PathDomino d = new PathDomino();
            d.ProtocolDefinition = TransformProtocol(i, j, width, height);
            float[] xCoord = new float[xCoordinates.Length];
            float[] yCoord = new float[xCoordinates.Length];
            for (int k = 0; k < xCoordinates.Length; k++)
            {
                xCoord[k] = xCoordinates[k] + move_x;
                yCoord[k] = yCoordinates[k] + move_y;
            }
            d.xCoordinates = xCoord;
            d.yCoordinates = yCoord;
            return d;
        }
        public override GraphicsPath GetPath(float scaling_x = 1, float scaling_y = 1)
        {
            GraphicsPath g = new GraphicsPath();
            g.AddPolygon(GetPoints(scaling_x, scaling_y));
            return g;
        }
        public PointF[] GetPoints(float scaling_x = 1, float scaling_y = 1)
        {
            PointF[] points = new PointF[xCoordinates.Length];
            for (int i = 0; i < xCoordinates.Length; i++)
            {
                points[i] = new PointF(xCoordinates[i] * scaling_x, yCoordinates[i] * scaling_y);
            }
            return points;
        }
        public override RectangleF GetContainer(float scaling_x, float scaling_y)
        {
            return new RectangleF(xCoordinates.Min()*scaling_x, yCoordinates.Min()*scaling_y, (xCoordinates.Max() - xCoordinates.Min())*scaling_x, (yCoordinates.Max() - yCoordinates.Min())*scaling_y);
        }
        public override bool IsInside(PointF point, bool ContainerChecked, float scaling_x, float scaling_y)
        {
            if (!ContainerChecked)
            {
                if (!GetContainer().Contains(point)) return false;
            }
            int i, j;
            bool c = false;
            for (i = 0, j = xCoordinates.Length - 1; i < xCoordinates.Length; j = i++)
            {
                if (((yCoordinates[i] * scaling_y > point.Y) != (yCoordinates[j] * scaling_y > point.Y)) &&
                 (point.X < (xCoordinates[j] * scaling_x - xCoordinates[i] * scaling_y) * 
                    (point.Y - yCoordinates[i] * scaling_y) / (yCoordinates[j] * scaling_y - yCoordinates[i] * scaling_y) + xCoordinates[i] * scaling_x))
                    c = !c;
            }
            return c;
        }
        public override bool Compare(DominoDefinition dominoDefinition)
        {
            if ((dominoDefinition is PathDomino))
            {
                PathDomino d = dominoDefinition as PathDomino;
                if (!d.xCoordinates.SequenceEqual(this.xCoordinates) || !d.yCoordinates.SequenceEqual(this.yCoordinates)) return false;
                return true;
            }
            return false;
        }
    }
    [Serializable()]
    public class DominoCell
    {
        public float width;
        public float height;
        public DominoDefinition[] Dominoes;

        public DominoCell() { }
        public DominoCell(XElement part)
        {
            width = float.Parse(part.Attribute("Width").Value, CultureInfo.InvariantCulture);
            height = float.Parse(part.Attribute("Height").Value, CultureInfo.InvariantCulture);
            List<DominoDefinition> dominoes = new List<DominoDefinition>();
            foreach (XElement domino in part.Elements())
            {
                DominoDefinition dominoDefinition = DominoDefinition.LoadDefinition(domino);
                dominoes.Add(dominoDefinition);
            }
            Dominoes = dominoes.ToArray();
        }

        public DominoCell TransformDefinition(float move_x, float move_y, int i, int j, int width, int height)
        {
            DominoCell d = new DominoCell() { width = this.width, height = this.height, Dominoes = new DominoDefinition[this.Dominoes.Length] };
            for (int k = 0; k < this.Dominoes.Length; k++)
            {
                d.Dominoes[k] = Dominoes[k].TransformDefinition(move_x, move_y, i, j, width, height);
            }
            return d;
        }
    }
    [Serializable()]
    public class Multipliers
    {
        public int xConstant;
        public int yConstant;
        public int xMultiplier;
        public int yMultiplier;
        public int WidthMultiplier;
        public int HeightMultiplier;

        public int Finalize (int i, int j, int width, int height)
        {
            return xConstant + yConstant + i * xMultiplier + j * yMultiplier + width * WidthMultiplier + height * HeightMultiplier;
        }
    }
    [Serializable()]
    public class DominoProtocolDefinition
    {
        public Multipliers xParams;
        public Multipliers yParams;
        public int x;
        public int y;

        public DominoProtocolDefinition()
        {
            xParams = new Multipliers();
            yParams = new Multipliers();
        }
        public DominoProtocolDefinition(XElement ProtocolDefinition)
        {
            xParams = new Multipliers();
            yParams = new Multipliers();
            if (ProtocolDefinition.Attributes("xPositionMultiplier").Count() != 0 || ProtocolDefinition.Attributes("yPositionMultiplier").Count() != 0
                || ProtocolDefinition.Attributes("WidthMultiplier").Count() != 0 || ProtocolDefinition.Attributes("HeightMultiplier").Count() != 0)
            {
                xParams.xConstant = GetAttribute(ProtocolDefinition, "xConstant");
                yParams.yConstant = GetAttribute(ProtocolDefinition, "yConstant");
                xParams.xMultiplier = GetAttribute(ProtocolDefinition, "xPositionMultiplier");
                yParams.yMultiplier = GetAttribute(ProtocolDefinition, "yPositionMultiplier");
                xParams.WidthMultiplier = GetAttribute(ProtocolDefinition, "WidthMultiplier");
                yParams.HeightMultiplier = GetAttribute(ProtocolDefinition, "HeightMultiplier");
            }
            else
            {
                xParams.xConstant = GetAttribute(ProtocolDefinition, "xConstant");
                xParams.xMultiplier = GetAttribute(ProtocolDefinition, "xxPositionMultiplier");
                xParams.yMultiplier = GetAttribute(ProtocolDefinition, "xyPositionMultiplier");
                xParams.WidthMultiplier = GetAttribute(ProtocolDefinition, "xWidthMultiplier");
                xParams.HeightMultiplier = GetAttribute(ProtocolDefinition, "xHeightMultiplier");
                yParams.yConstant = GetAttribute(ProtocolDefinition, "yConstant");
                yParams.xMultiplier = GetAttribute(ProtocolDefinition, "yxPositionMultiplier");
                yParams.yMultiplier = GetAttribute(ProtocolDefinition, "yyPositionMultiplier");
                yParams.WidthMultiplier = GetAttribute(ProtocolDefinition, "yWidthMultiplier");
                yParams.HeightMultiplier = GetAttribute(ProtocolDefinition, "yHeightMultiplier");
            }
            
        }
        private int GetAttribute(XElement element, String id)
        {
            if (element.Attribute(id) == null) return 0;
            else return int.Parse(element.Attribute(id).Value);
        }
        public DominoProtocolDefinition FinalizeProtocol(int i, int j, int width, int height)
        {

            if (x == 0 && y == 0)
                return new DominoProtocolDefinition() { x = xParams.Finalize(i, j, width, height), y = yParams.Finalize(i, j, width, height) };
            else return this;
        }
    }
    [Serializable()]
    public struct GenStructHelper
    {
        public float width;
        public float height;
        public DominoDefinition[] dominoes;
        public bool HasProtocolDefinition;
    }
    [Serializable()]
    public class ClusterStructure
    {
        public String name;
        public DominoCell[,] cells;
        public bool HasProtocolDefinition;
        private float PreviewScaleFactor ( int TargetDimension )
        { 
                float largest = 0;
                // get largest dimension
                foreach (DominoCell c in cells)
                {
                    if (c.width > largest) largest = c.width;
                    if (c.height > largest) largest = c.height;
                }
                return TargetDimension / largest;
            
        }
        /// <summary>
        /// Liest eine ClusterStructure aus einem gegebenen XElement ein.
        /// </summary>
        /// <param name="definition">Das XElement, welches die Definition beinhaltet.</param>
        public ClusterStructure(XElement definition) // read definition from structure template
        {
            HasProtocolDefinition = definition.Attribute("HasProtocolDefinition").Value == "true";
            name = definition.Attribute("Name").Value;
            cells = new DominoCell[3,3];
            foreach (XElement part in definition.Elements("PartDefinition"))
            {
                int col = GetIndex(part.Attribute("HorizontalPosition").Value);
                int row = GetIndex(part.Attribute("VerticalPosition").Value);
                cells[col, row] = new DominoCell(part);
            }
        }
        /// <summary>
        /// Berechnet die kleine Vorschau einer Zelle.
        /// </summary>
        /// <param name="x">horizontale Achse</param>
        /// <param name="y">vertikale Achse</param>
        public BitmapSource DrawPreview(int col, int row, int targetDimension)
        {
            DominoCell cell = cells[col, row];
            float scaling_factor = PreviewScaleFactor(targetDimension); // scale everything to the same size
            Bitmap b = new Bitmap((int)(cell.width * scaling_factor + 2), (int)(cell.height * scaling_factor + 2)); // +2 for right borders
            Graphics g = Graphics.FromImage(b);
            for (int colc = (col == 2) ? 1 : 0; colc <= ((col == 0) ? 1 : 2); colc++)
            {
                for (int rowc = (row == 2) ? 1 : 0; rowc <= ((row == 0) ? 1 : 2); rowc++) // only use the cells next to the specified (so top left uses 4 top center, center left and center center).
                {
                    DominoCell current = cells[colc, rowc];
                    int xOffsetMultiplier = colc - col; // for moving the cells
                    int yOffsetMultiplier = rowc - row;
                    for (int i = 0; i < current.Dominoes.Length; i++)
                    {
                        DominoDefinition transformed = current.Dominoes[i].TransformDefinition(
                            xOffsetMultiplier * ((xOffsetMultiplier > 0) ? cell.width : current.width), 
                            yOffsetMultiplier * ((yOffsetMultiplier > 0) ? cell.height : current.height), 0, 0, 0, 0); // move the dominoes
                        RectangleF container = transformed.GetContainer(); // get containing rectangle
                        if (container.X >= cells[col, row].width || container.X + container.Width <= 0 || container.Y >= cells[col, row].height || container.Y + container.Height <= 0) continue; // check if rectangle is out of drawing area
                        g.FillPath(Brushes.LightGray, transformed.GetPath(scaling_factor));
                        g.DrawPath(new Pen(Color.Black, 2 * scaling_factor), transformed.GetPath(scaling_factor));
                    }
                }
            }
            return ImageHelper.BitmapToBitmapImage(b);
        }
        /// <summary>
        /// Konvertiert den Text-Index aus dem XML (Left, Center, Right) / (Top, Center, Bottom) in eine Zahl.
        /// </summary>
        /// <param name="Position">Der Index-String.</param>
        /// <returns>Der Positions-Index als Zahl.</returns>
        private int GetIndex(string Position)
        {
            if (Position == "Center") return 1;
            else if (Position == "Left" || Position == "Right")
            {
                return (Position == "Left") ? 0 : 2;
            }
            else
            {
                return (Position == "Top") ? 0 : 2;
            }
        }
        /// <summary>
        /// Generiert eine Zellstruktur mit der angegebenen Länge und Breite. Die Ränder werden automatisch dazu addiert.
        /// </summary>
        /// <param name="s_width">Breite der gewünschten Struktur.</param>
        /// <param name="s_height">Länge der gewünschten Struktur</param>
        /// <returns>Einen GenStructHelper mit den Strukturinformationen.</returns>
        public GenStructHelper GenerateStructure(int s_width, int s_height)
        {
            List<DominoDefinition> DominoList = new List<DominoDefinition>();
            GenStructHelper g = new GenStructHelper() // Initialize GenStructHelper with final size.
            {
                width = cells[0, 0].width + cells[1, 1].width * s_width + cells[2, 2].width,
                height = cells[0, 0].height + cells[1, 1].height * s_height + cells[2, 2].height
            };
            for (int x = -1; x < s_width + 1; x++)
            {
                for (int y = -1; y < s_height + 1; y++)
                {
                    DominoList.AddRange(
                        (cells[(x == -1) ? 0 : ((x == s_width) ? 2 : 1), (y == -1) ? 0 : ((y == s_height) ? 2 : 1)]
                        .TransformDefinition(
                            (x == -1) ? 0 : (cells[1, 1].width * x + cells[0,0].width), 
                            (y == -1) ? 0 : (cells[1, 1].height * y + cells[0, 0].height), 
                            x, y, s_width, s_height))
                        .Dominoes);
                }
            }
            g.dominoes = DominoList.ToArray();
            g.HasProtocolDefinition = HasProtocolDefinition;
            return g;
        }
    }
    public class SpiralStructure
    {
        int domino_width;
        int domino_height;
        int tangential_distance;
        int normal_distance;
        double a;
        double theta_max;
        double theta_min = Math.PI * 2;
        public SpiralStructure(int quarter_rotations,
            int domino_width, int domino_height, int tangential_distance, int normal_distance)
        {
            a = (normal_distance + domino_height) /(2d * Math.PI);
            this.domino_width = domino_width;
            this.domino_height = domino_height;
            this.tangential_distance = tangential_distance;
            this.normal_distance = normal_distance;
            theta_max = quarter_rotations * Math.PI/2 + theta_min;

        }
        public GenStructHelper GenerateSpiral()
        {
            List<DominoDefinition> dominolist = new List<DominoDefinition>();
            double theta = theta_min;
            int ycounter = 0;
            while (theta < theta_max)
            {
                
                DominoDefinition d = CreateDomino(theta);
                d.ProtocolDefinition = new DominoProtocolDefinition();
                d.ProtocolDefinition.yParams.yConstant = (int)Math.Floor((theta - theta_min) / (2d * Math.PI));
                d.ProtocolDefinition.xParams.xConstant = ycounter;
                dominolist.Add(d);
                double start_value = theta + 0.01d;
                double theta_new = newton_archimedean(theta, start_value, (double)tangential_distance + (double)domino_width);
                while (theta_new < theta)
                {
                    start_value += 0.01d;
                    theta_new = newton_archimedean(theta, start_value, (double)tangential_distance + (double)domino_width);
                }
                ycounter++;
                if ((int)Math.Floor((theta - theta_min) / (2d * Math.PI)) != (int)Math.Floor((theta_new - theta_min) / (2d * Math.PI)))
                    ycounter = 0;
                theta = theta_new;
            }
            DominoDefinition[] Domino = dominolist.ToArray();
            float x_min = int.MaxValue;
            float y_min = int.MaxValue;
            float x_max = int.MinValue;
            float y_max = int.MinValue;
            foreach (DominoDefinition d in Domino)
            {
                RectangleF container = d.GetContainer();
                if (container.X < x_min) x_min = container.X;
                if (container.Y < y_min) y_min = container.Y;
                if (container.X + container.Width > x_max) x_max = container.X + container.Width;
                if (container.Y + container.Height > y_max) y_max = container.Y + container.Height;
            }
            for (int i = 0; i < Domino.Length; i++)
            { 
                Domino[i] = Domino[i].TransformDefinition(-x_min, -y_min, 0, 0, 0, 0);
            }
            GenStructHelper g = new GenStructHelper();
            g.HasProtocolDefinition = true;
            g.dominoes = Domino;
            g.width = x_max - x_min;
            g.height = y_max - y_min;
            return g;
        }
        double newton_archimedean(double theta, double startwert, double abstand)
        {
            double cos = Math.Cos(theta - startwert);
            double result = startwert - (Math.Sqrt(a * a * (theta * theta - 2 * theta * startwert * cos + startwert * startwert)) * abstand -
              a * a * (theta * theta - 2 * theta * startwert * cos + startwert * startwert)) /
              (a * a * (theta * startwert * Math.Sin(theta - startwert) + theta * cos - startwert));
            if (Math.Abs((result - startwert)) < 0.000001) return result;
            else return newton_archimedean(theta, result, abstand);
        }
        public PointD getPoint(double theta)
        {
            return new PointD() { X = theta * Math.Cos(theta) * a, Y = theta * Math.Sin(theta) * a };
        }
        public DominoDefinition CreateDomino(double theta)
        {
            double normal_angle = GetTangentAngle(theta); // mal sehen ob das passt
            double x1 = getPoint(theta).X + 0.5d * (double)domino_width * Math.Cos(0.5d * Math.PI - normal_angle) - 0.5d * (double)domino_height * Math.Cos(normal_angle);
            double y1 = getPoint(theta).Y - 0.5d * (double)domino_width * Math.Sin(0.5d * Math.PI - normal_angle) - 0.5d * (double)domino_height * Math.Sin(normal_angle);
            double x2 = x1 - (double)domino_width * Math.Cos(0.5d * Math.PI - normal_angle);
            double y2 = y1 + (double)domino_width * Math.Sin(0.5d * Math.PI - normal_angle);
            double x3 = x2 + Math.Cos(normal_angle) * (double)domino_height;
            double y3 = y2 + Math.Sin(normal_angle) * (double)domino_height;
            double x4 = x3 + (double)domino_width * Math.Cos(0.5d * Math.PI - normal_angle);
            double y4 = y3 - (double)domino_width * Math.Sin(0.5d * Math.PI - normal_angle);
            PathDomino d = new PathDomino()
            {
                xCoordinates = new float[] { (float)x1, (float)x2, (float)x3, (float)x4 },
                yCoordinates = new float[] { (float)y1, (float)y2, (float)y3, (float)y4 }
            };
            return d;
        }
        public double GetTangentAngle(double theta)
        {
            PointD point = getPoint(theta);
            PointD point2 = getPoint(theta + 0.00001);
            return Math.PI/2d - Math.Atan((point.Y - point2.Y) / (point2.X - point.X));
        }
    }
    public class PointD
    {
        public double X;
        public double Y;
    }
    public class CircleStructure
    {
        int domino_width;
        int domino_height;
        int tangential_distance;
        int normal_distance;
        int d_min;
        int circles;

        public CircleStructure(int rotations, int domino_width, int domino_height, int tangential_distance, int normal_distance, int start_diameter)
        {
            this.circles = rotations;
            this.domino_width = domino_width;
            this.domino_height = domino_height;
            this.tangential_distance = tangential_distance;
            this.normal_distance = normal_distance;
            d_min = start_diameter;
        }
        public GenStructHelper GenerateCircle()
        {
            List<DominoDefinition> dominos = new List<DominoDefinition>();
            int circlecount = 0;
            int diameter = d_min;
            while (circlecount < circles)
            {
                diameter += 2 * domino_height + 2 * normal_distance;
                // get number of dominoes in this spiral
                double domino_angle = Math.Asin((double) domino_width / diameter) * 2;
                double distance_angle = Math.Asin((double) tangential_distance / diameter) * 2;
                int current_domino_count = (int)Math.Floor(2*Math.PI / ((double)domino_angle + distance_angle));
                // equally space the distance between all dominoes
                distance_angle = (2 * Math.PI - (domino_angle * current_domino_count)) / current_domino_count;
                // calculate dominoes
                double angle = 0;
                for (int i = 0; i < current_domino_count; i++)
                {
                    DominoDefinition d = GenerateDomino(diameter, angle, domino_angle);
                    angle += domino_angle + distance_angle;
                    dominos.Add(d);
                }
                circlecount++;
            }
            DominoDefinition[] Domino = dominos.ToArray();
            float x_min = int.MaxValue;
            float y_min = int.MaxValue;
            float x_max = int.MinValue;
            float y_max = int.MinValue;
            foreach (DominoDefinition d in Domino)
            {
                RectangleF container = d.GetContainer();
                if (container.X < x_min) x_min = container.X;
                if (container.Y < y_min) y_min = container.Y;
                if (container.X + container.Width > x_max) x_max = container.X + container.Width;
                if (container.Y + container.Height > y_max) y_max = container.Y + container.Height;
            }
            for (int i = 0; i < Domino.Length; i++)
            {
                Domino[i] = Domino[i].TransformDefinition(-x_min, -y_min, 0, 0, 0, 0);
            }
            GenStructHelper g = new GenStructHelper();
            g.HasProtocolDefinition = false;
            g.dominoes = Domino;
            g.width = x_max - x_min;
            g.height = y_max - y_min;
            return g;

            GenStructHelper circle = new GenStructHelper();
            // do some magic
            return circle;
        }

        private DominoDefinition GenerateDomino(int diameter, double angle, double domino_angle)
        {
            double normal_angle = angle + domino_angle / 2;
            double x1 = diameter / 2d * Math.Cos(angle);
            double y1 = diameter / 2d * Math.Sin(angle);
            double x2 = diameter / 2d * Math.Cos(angle + domino_angle);
            double y2 = diameter / 2d * Math.Sin(angle + domino_angle);
            double x4 = diameter / 2d * Math.Cos(angle) + Math.Cos(normal_angle) * domino_height;
            double y4 = diameter / 2d * Math.Sin(angle) + Math.Sin(normal_angle) * domino_height;
            double x3 = diameter / 2d * Math.Cos(angle + domino_angle)+ Math.Cos(normal_angle) * domino_height; 
            double y3 = diameter / 2d * Math.Sin(angle + domino_angle)+ Math.Sin(normal_angle) * domino_height;
            PathDomino d = new PathDomino()
            {
                xCoordinates = new float[] { (float)x1, (float)x2, (float)x3, (float)x4 },
                yCoordinates = new float[] { (float)y1, (float)y2, (float)y3, (float)y4 }
            };
            return d;
            
        }
    }
}
