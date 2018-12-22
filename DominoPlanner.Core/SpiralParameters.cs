using Emgu.CV;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Core
{
    [ProtoContract]
    [ProtoInclude(100, typeof(SpiralParameters))]
    [ProtoInclude(101, typeof(CircleParameters))]
    public abstract class CircularStructure : GeneralShapesProvider
    {
        private int _dominoWidth;
        /// <summary>
        /// Breite eines Steins in tangentialer Richtung.
        /// </summary>
        [ProtoMember(1)]
        public int DominoWidth
        {
            get
            {
                return _dominoWidth;
            }
            set
            {
                _dominoWidth = value;
                shapesValid = false;
                
            }
        }
        private int _dominoLength;
        /// <summary>
        /// Breite eines Steins in normaler (senkrecht zur Kurve) Richtung
        /// </summary>
        [ProtoMember(2)]
        public int DominoLength
        {
            get
            {
                return _dominoLength;
            }
            set
            {
                _dominoLength = value;
                shapesValid = false;
                charLength = DominoLength + TangentialDistance;
            }
        }

        private int _tangentialDistance;
        /// <summary>
        /// Abstand zwischen zwei Steinen in tangentialer Richtung
        /// </summary>
        [ProtoMember(3)]
        public int TangentialDistance
        {
            get
            {
                return _tangentialDistance;
            }
            set
            {
                _tangentialDistance = value;
                shapesValid = false;
                charLength = DominoLength + TangentialDistance;
            }
        }

        private int _normalDistance;
        /// <summary>
        /// Abstand zwischen zwei Steinen in normaler Richtung
        /// </summary>
        [ProtoMember(4)]
        public int NormalDistance
        {
            get
            {
                return _normalDistance;
            }
            set
            {
                _normalDistance = value;
                shapesValid = false;
            }
        }
        public CircularStructure(string bitmap, string colors,
            IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, IterationInformation iterationInformation, bool allowStretch = false)
            : base(bitmap, colors, colorMode, ditherMode, averageMode, allowStretch, iterationInformation)
        {

        }
        public CircularStructure(int imageWidth, int imageHeight, Color background, string colors,
           IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, IterationInformation iterationInformation, bool allowStretch = false)
           : base(imageWidth, imageHeight, background, colors, colorMode, ditherMode, averageMode, allowStretch, iterationInformation)
        {

        }
        internal PathDomino CreateDominoAtCoordinates(double x, double y, double angle, int protocol_x, int protocol_y)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);
            double p1_x = x + sin * DominoLength / 2d + cos * DominoWidth / 2d;
            double p1_y = y - cos * DominoLength / 2d + sin * DominoWidth / 2d;
            double p2_x = p1_x - sin * DominoLength;
            double p2_y = p1_y + cos * DominoLength;
            double p3_x = p2_x - cos * DominoWidth;
            double p3_y = p2_y - sin * DominoWidth;
            double p4_x = p3_x + sin * DominoLength;
            double p4_y = p3_y - cos * DominoLength;
            return new PathDomino()
            {
                points = new Point[] { new Point(p1_x, p1_y), new Point(p2_x, p2_y), new Point(p3_x, p3_y), new Point(p4_x, p4_y) },
                position = new ProtocolDefinition() { x = protocol_x, y = protocol_y }
            };
        }
        internal CircularStructure()
        {

        }
    }
    /// <summary>
    /// Stellt Methoden und Eigenschaften bereit, um eine Spirale zu erstellen. 
    /// Mehrfachspiralen sind möglich.
    /// </summary>
    [ProtoContract]
    public class SpiralParameters : CircularStructure 
    {
        #region public properties
        /// <summary>
        /// Die Viertelumdrehungen der Spirale.
        /// </summary>
        public double QuarterRotations
        {
            get
            {
                return (int)((ThetaMax - ThetaMin) * 2 / Math.PI);
            }
            set
            {
                ThetaMax = value * Math.PI / 2 + ThetaMin;
            }
        }
        private double _theta_min = 3 * Math.PI;
        [ProtoMember(1)]
        public double ThetaMin 
        {
            get
            {
                return _theta_min;
            }
            set
            {
                shapesValid = false;
                _theta_min = value;
            }
        }
        private double _theta_max;
        [ProtoMember(2)]
        public double ThetaMax
        {
            get
            {
                return _theta_max;
            }
            set
            {
                _theta_max = value;
                shapesValid = false;
            }
        }
        [ProtoMember(3)]
        public int NormalGroupDistance
        {
            get
            {
                return _normalGroupDistance;
            }
            set
            {
                shapesValid = false;
                _normalGroupDistance = value;
            }
        }
        private int nGroup = 1;
        [ProtoMember(4)]
        public int NumberOfGroups
        {
            get
            {
                return nGroup;
            }
            set
            {
                nGroup = value;
                shapesValid = false;
            }
        }
        private int nArms = 1;
        [ProtoMember(5)]
        public int NumberOfArms
        {
            get
            {
                return nArms;
            }
            set
            {
                nArms = value;
                shapesValid = false;
            }
        }
        private bool closeEnds = true;
        [ProtoMember(6)]
        public bool CloseEnds
        {
            get
            {
                return closeEnds;
            }
            set
            {
                closeEnds = value;
                shapesValid = false;
            }
        }
        private double shiftfactor = 0;
        [ProtoMember(7)]
        public double ShiftFactor
        {
            get
            {
                return shiftfactor;
            }
            set
            {
                shiftfactor = value;
                shapesValid = false;
            }
        }
        #endregion
        #region private properties
        /// <summary>
        /// Der Parameter a in der Spiralformel, a.k.a. Abstand zwischen zwei Umdrehungen
        /// </summary>
        private double a
        {
            get
            {
                return ((DominoLength + NormalDistance) * nGroup + NormalGroupDistance) * nArms / (2d * Math.PI);
            }
        }
        private int _normalGroupDistance = 8;
        
        #endregion
        #region constructors

        /// <summary>
        /// Generiert eine Spirale mit der angegebenen Zahl Viertelumdrehungen.
        /// </summary>
        /// <param name="bitmap">Das der Spirale zugrunde liegende Bild.</param>
        /// <param name="quarterRotations">Die Viertelumdrehungen der Spirale.</param>
        /// <param name="normalWidth">Breite eines Steins in tangentialer Richtung</param>
        /// <param name="tangentialWidth">Breite eines Steins in normaler Richtung</param>
        /// <param name="normalDistance">Abstand zwischen zwei Steinen in normaler Richtung</param>
        /// <param name="tangentialDistance">Abstand zwischen zwei Steinen in tangentialer Richtung</param>
        /// <param name="colors">Die Farben, die für dieses Objekt verwendet werden sollen.</param>
        /// <param name="colorMode">Der Interpolationsmodus, der zur Farberkennung verwendet wird.</param>
        /// <param name="averageMode">Gibt an, ob nur ein Punkt des Dominos (linke obere Ecke) oder ein Durchschnittswert aller Pixel unter dem Pfad verwendet werden soll, um die Farbe auszuwählen.</param>
        /// <param name="allowStretch">Gibt an, ob beim Berechnen die Struktur an das Bild angepasst werden darf.</param>
        /// <param name="useOnlyMyColors">Gibt an, ob die Farben nur in der angegebenen Menge verwendet werden sollen. 
        /// Ist diese Eigenschaft aktiviert, kann das optische Ergebnis schlechter sein, das Objekt ist aber mit den angegeben Steinen erbaubar.</param>
        public SpiralParameters(string bitmap, int rotations, string colors, 
            IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, IterationInformation iterationInformation, bool allowStretch = false) :
            base(bitmap, colors, colorMode, ditherMode, averageMode, iterationInformation, allowStretch)
        {
            hasProcotolDefinition = false;
            init(rotations);
        }
        public SpiralParameters(int imageWidth, int imageHeight, Color background, int rotations, string colors, 
            IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode, IterationInformation iterationInformation, bool allowStretch = false)
            : base(imageWidth, imageHeight, background, colors, colorMode, ditherMode, averageMode, iterationInformation,allowStretch )
        {
            hasProcotolDefinition = false;
            init(rotations);
        }
        private void init(int rotations)
        {
            ThetaMax = rotations * 2 * Math.PI + ThetaMin;
            DominoWidth = 8;
            DominoLength = 24;
            TangentialDistance = 8;
            DominoWidth = 8;
        }
        private SpiralParameters() : base() { }
        #endregion
        #region private methods
        internal override void GenerateShapes()
        {
            Point endpoint = getPoint(ThetaMax, nGroup * (DominoLength + NormalDistance) / a);
            double end_radius = Math.Sqrt(endpoint.X * endpoint.X + endpoint.Y * endpoint.Y);
            List<PathDomino> dominolist = new List<PathDomino>();
            double pi2 = 1 / (2d * Math.PI); // spart ein paar Gleitkommadivisionen
            for (int i = 0; i < nGroup; i++)
            {
                double shift = i * (DominoLength + NormalDistance) / a;
                shift = shift - shiftfactor * nGroup * (DominoLength + NormalDistance) / a;
                double theta = ThetaMin;
                int ycounter = 0;
                double current_radius = 0;
                while (closeEnds ? current_radius < end_radius : theta < ThetaMax) 
                {
                    Point current_point = getPoint(theta, i * (DominoLength + NormalDistance) / a);
                    current_radius = Math.Sqrt(current_point.X * current_point.X + current_point.Y * current_point.Y);
                    for (int k = 0; k < nArms; k++)
                    {
                        PathDomino d = CreateDomino(theta, shift, 2.0 * Math.PI / nArms * k);
                        //d.position = new ProtocolDefinition();
                        //d.position.y = (int)Math.Floor((theta - theta_min) * pi2);
                        //d.position.x = ycounter;
                        dominolist.Add(d);
                    }
                    double start_value = theta;
                    double theta_new;
                    do
                    {
                        start_value += 0.01d;
                        theta_new = approximate_archimedean(theta, start_value, TangentialDistance + DominoWidth, shift);
                    }
                    while (theta_new < theta);
                    ycounter++;
                    if ((int)Math.Floor((theta - ThetaMin) * pi2) != (int)Math.Floor((theta_new - ThetaMin) * pi2))
                        ycounter = 0;
                    theta = theta_new;
                }
            }
            IDominoShape[] dominoes = dominolist.ToArray();
            DominoRectangle[] containers = dominoes.AsParallel().Select(x => x.GetContainer()).ToArray();

            double x_min = containers.Min(x => x.x);
            double y_min = containers.Min(x => x.y);
            double x_max = containers.Max(x => x.width + x.x);
            double y_max = containers.Max(x => x.height + x.y);
            Parallel.For(0, dominoes.Length, (i) =>
            {
                dominoes[i] = dominoes[i].TransformDomino(-x_min, -y_min, 0, 0, 0, 0);

            });
            GenStructHelper g = new GenStructHelper();
            g.HasProtocolDefinition = false;
            g.dominoes = dominoes;
            g.width = x_max - x_min;
            g.height = y_max - y_min;
            shapes = g;
            shapesValid = true;
        }
        /// <summary>
        /// Berechnet iterativ den nächsten Winkel mit dem Newton-Verfahren.
        /// </summary>
        /// <param name="theta">Der alte Winkel.</param>
        /// <param name="startwert">Der letzte Wert</param>
        /// <param name="abstand">Zu erzielender Abstand zwischen dem Punkt mit dem alten und dem Punkt mit dem neuen Winkel.</param>
        /// <returns>Der neue Winkel.</returns>
        private double newton_archimedean(double theta, double startwert, double abstand, double shift)
        {
            
            double cos = Math.Cos(theta - startwert);
            double result2 = startwert - (Math.Sqrt(a * a * (theta * theta - 2 * theta * startwert * cos + startwert * startwert)) 
                * abstand -
              a * a * (theta * theta - 2 * theta * startwert * cos + startwert * startwert)) /
              (a * a * (theta * startwert * Math.Sin(theta - startwert) + theta * cos - startwert));
            // alte Implementierung ohne Shift
            /*double result = startwert -
                (Math.Sqrt(-2 * theta * cos * (startwert + shift) + theta * theta + (startwert + shift) * (startwert + shift)) * abstand
                    - a * (-2 * theta * cos * (startwert + shift) + theta * theta + (startwert + shift) * (startwert + shift)))
                    / (a * (cos * theta + (theta * Math.Sin(theta - startwert) - 1) * (startwert + shift)));
                    */
            double wurzel = -(2 * (theta + shift)) * (startwert + shift) * cos
                + 2 * shift * shift + (2 * theta + 2 * startwert) * shift + startwert * startwert + theta * theta;
            double result = startwert - (Math.Sqrt(wurzel) * abstand - a * wurzel)
                / (a * ((theta + shift) * cos + (startwert + shift) * (-1 + (theta + shift) * Math.Sin(theta - startwert))));
            if (Math.Abs((result - startwert)) < 0.001) return result;
            else return newton_archimedean(theta, result, abstand, shift);
        }
        // berechnet die Ableitung nur approximativ. Scheint nicht wirklich Performance-Unterschiede zu machen, 
        // aber ist einfacher zu verändern
        private double approximate_archimedean(double theta, double startwert, double abstand, double shift)
        {
            double dist = distance(startwert, theta, shift);
            double ableitung = (distance(startwert + 0.0001d, theta, shift) -dist) / 0.0001d;
            double result = startwert - (dist - abstand) / ableitung;
            if (Math.Abs((result - startwert)) < 0.001) return result;
            else return approximate_archimedean(theta, result, abstand, shift);
        }
        private double distance(double theta1, double theta2, double shift)
        {
            var point1 = getPoint(theta1, shift);
            var point2 = getPoint(theta2, shift);
            return Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y));
        }
        /// <summary>
        /// Berechnet den Punkt mit dem angegebenen Winkel auf der Spirale.
        /// </summary>
        /// <param name="theta">Ein Winkel.</param>
        /// <returns>Der Punkt, der sich für den angegebenen Winkel aus der Polarfunktion ergibt.</returns>
        private Point getPoint(double theta, double shift, double rotate = 0)
        {
            Point result = new Point((theta+shift) * Math.Cos(theta) * a, (theta+shift) * Math.Sin(theta) * a);
            if (rotate == 0) return result;
            Point gedreht = new Point(result.X * Math.Cos(rotate) - result.Y * Math.Sin(rotate),
                result.X * Math.Sin(rotate) + result.Y * Math.Cos(rotate));
            return gedreht;
        }
        /// <summary>
        /// Erstellt einen Pathdomino am angegebenen Punkt (Winkel)
        /// </summary>
        /// <param name="theta">Der Winkel des Punktes, der in der Mitte des zu erzeugenden Steins liegen soll.</param>
        /// <returns>Der PathDomino.</returns>
        private PathDomino CreateDomino(double theta, double shift, double rotate)
        {
            double normal_angle = GetTangentialAngle(theta, shift, rotate);
            double x1 = getPoint(theta, shift, rotate).X;
            double y1 = getPoint(theta, shift, rotate).Y;
            return CreateDominoAtCoordinates(x1, y1, normal_angle, 1, 1);
        }
        /// <summary>
        /// Berechnet den Winkel normal zur Spirale am angegebenen Punkt.
        /// </summary>
        /// <param name="theta">Der Winkel, der den Punkt charakterisiert.</param>
        /// <returns>Ein Winkel, normal zur Spirale.</returns>
        private double GetTangentialAngle(double theta, double shift, double rotate = 0)
        {
            Point point = getPoint(theta, shift, rotate);
            Point point2 = getPoint(theta + 0.00001, shift, rotate);
            return -Math.Atan((point.Y - point2.Y) / (point2.X - point.X));
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}