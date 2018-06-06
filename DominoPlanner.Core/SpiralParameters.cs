using ColorMine.ColorSpaces.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace DominoPlanner.Core
{
    /// <summary>
    /// Stellt Methoden und Eigenschaften bereit, um eine Spirale zu erstellen.
    /// Derzeit nur Einfachspiralen, Mehrfachspiralen folgen.
    /// </summary>
    public class SpiralParameters : RectangleDominoProvider
    {
        #region public properties
        private int _tangentialWidth;
        /// <summary>
        /// Breite eines Steins in tangentialer Richtung.
        /// </summary>
        public int tangentialWidth
        {
            get
            {
                return _tangentialWidth;
            }
            set
            {
                _tangentialWidth = value;
                shapesValid = false;
            }
        }

        private int _normalWidth;
        /// <summary>
        /// Breite eines Steins in normaler (senkrecht zur Kurve) Richtung
        /// </summary>
        public int normalWidth
        {
            get
            {
                return _normalWidth;
            }
            set
            {
                _normalWidth = value;
                shapesValid = false;
            }
        }

        private int _tangentialDistance;
        /// <summary>
        /// Abstand zwischen zwei Steinen in tangentialer Richtung
        /// </summary>
        public int tangentialDistance
        {
            get
            {
                return _tangentialDistance;
            }
            set
            {
                _tangentialDistance = value;
                shapesValid = false;
            }
        }

        private int _normalDistance;
        /// <summary>
        /// Abstand zwischen zwei Steinen in normaler Richtung
        /// </summary>
        public int normalDistance
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
        /// <summary>
        /// Die Viertelumdrehungen der Spirale.
        /// </summary>
        public double quarterRotations
        {
            get
            {
                return (int)((theta_max - theta_min) * 2 / Math.PI);
            }
            set
            {
                theta_max = value * Math.PI / 2 + theta_min;
            }
        }
        public override int targetCount
        {
            set
            {
                throw new NotImplementedException();
            }
        }
        #endregion
        #region private properties
        private double theta_min = 2* Math.PI;
        private double theta_max;
        /// <summary>
        /// Der Parameter a in der Spiralformel, a.k.a. Abstand zwischen zwei Umdrehungen
        /// </summary>
        private double a
        {
            get
            {
                return (normalDistance + normalWidth) / (2d * Math.PI);
            }
        }
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
        public SpiralParameters(WriteableBitmap bitmap, double quarterRotations, int normalWidth, int tangentialWidth, int normalDistance, int tangentialDistance, List<DominoColor> colors, IColorSpaceComparison colorMode, bool useOnlyMyColors, AverageMode averageMode, bool allowStretch = false) :
            base(bitmap, colors, colorMode, useOnlyMyColors, null, averageMode, allowStretch)
        {
            this.quarterRotations = quarterRotations;
            this.normalDistance = normalDistance;
            this.normalWidth = normalWidth;
            this.tangentialDistance = tangentialDistance;
            this.tangentialWidth = tangentialWidth;
            hasProcotolDefinition = true;
        }
        #endregion
        #region private methods
        protected override void GenerateShapes()
        {
            List<PathDomino> dominolist = new List<PathDomino>();
            double theta = theta_min;
            int ycounter = 0;
            double pi2 = 1 / (2d * Math.PI); // spart ein paar Gleitkommadivisionen
            while (theta < theta_max)
            {

                PathDomino d = CreateDomino(theta);
                d.position = new ProtocolDefinition();
                d.position.y = (int)Math.Floor((theta - theta_min) *pi2);
                d.position.x = ycounter;
                dominolist.Add(d);
                double start_value = theta;
                double theta_new;
                do
                {
                    start_value += 0.01d;
                    theta_new = newton_archimedean(theta, start_value, tangentialDistance + tangentialWidth);
                }
                while (theta_new < theta);
                ycounter++;
                if ((int)Math.Floor((theta - theta_min) * pi2) != (int)Math.Floor((theta_new - theta_min) * pi2))
                    ycounter = 0;
                theta = theta_new;
            }
            IDominoShape[] dominoes = dominolist.ToArray();
            float x_min = dominoes.Min(x => x.GetContainer().x1);
            float y_min = dominoes.Min(x => x.GetContainer().y1);
            float x_max = dominoes.Max(x => x.GetContainer().x2);
            float y_max = dominoes.Max(x => x.GetContainer().y2);

            for (int i = 0; i < dominoes.Length; i++)
            {
                dominoes[i] = dominoes[i].TransformDomino(-x_min, -y_min, 0, 0, 0, 0);
            }
            GenStructHelper g = new GenStructHelper();
            g.HasProtocolDefinition = true;
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
        private double newton_archimedean(double theta, double startwert, double abstand)
        {
            double cos = Math.Cos(theta - startwert);
            double result = startwert - (Math.Sqrt(a * a * (theta * theta - 2 * theta * startwert * cos + startwert * startwert)) * abstand -
              a * a * (theta * theta - 2 * theta * startwert * cos + startwert * startwert)) /
              (a * a * (theta * startwert * Math.Sin(theta - startwert) + theta * cos - startwert));
            if (Math.Abs((result - startwert)) < 0.000001) return result;
            else return newton_archimedean(theta, result, abstand);
        }
        /// <summary>
        /// Berechnet den Punkt mit dem angegebenen Winkel auf der Spirale.
        /// </summary>
        /// <param name="theta">Ein Winkel.</param>
        /// <returns>Der Punkt, der sich für den angegebenen Winkel aus der Polarfunktion ergibt.</returns>
        private Point getPoint(double theta)
        {
            return new Point() { X = theta * Math.Cos(theta) * a, Y = theta * Math.Sin(theta) * a };
        }
        /// <summary>
        /// Erstellt einen Pathdomino am angegebenen Punkt (Winkel)
        /// </summary>
        /// <param name="theta">Der Winkel des Punktes, der in der Mitte des zu erzeugenden Steins liegen soll.</param>
        /// <returns>Der PathDomino.</returns>
        private PathDomino CreateDomino(double theta)
        {
            double normal_angle = GetNormalAngle(theta);
            double x1 = getPoint(theta).X + 0.5d * tangentialWidth * Math.Cos(0.5d * Math.PI - normal_angle) - 0.5d * normalWidth * Math.Cos(normal_angle);
            double y1 = getPoint(theta).Y - 0.5d * tangentialWidth * Math.Sin(0.5d * Math.PI - normal_angle) - 0.5d * normalWidth * Math.Sin(normal_angle);
            double x2 = x1 - tangentialWidth * Math.Cos(0.5d * Math.PI - normal_angle);
            double y2 = y1 + tangentialWidth * Math.Sin(0.5d * Math.PI - normal_angle);
            double x3 = x2 + Math.Cos(normal_angle) * normalWidth;
            double y3 = y2 + Math.Sin(normal_angle) * normalWidth;
            double x4 = x3 + tangentialWidth * Math.Cos(0.5d * Math.PI - normal_angle);
            double y4 = y3 - tangentialWidth * Math.Sin(0.5d * Math.PI - normal_angle);
            PathDomino d = new PathDomino()
            {
                points = new Point[] { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3), new Point(x4, y4) }
            };
            return d;
        }
        /// <summary>
        /// Berechnet den Winkel normal zur Spirale am angegebenen Punkt.
        /// </summary>
        /// <param name="theta">Der Winkel, der den Punkt charakterisiert.</param>
        /// <returns>Ein Winkel, normal zur Spirale.</returns>
        private double GetNormalAngle(double theta)
        {
            Point point = getPoint(theta);
            Point point2 = getPoint(theta + 0.00001);
            return Math.PI * 0.5d - Math.Atan((point.Y - point2.Y) / (point2.X - point.X));
        }
        #endregion
    }
}