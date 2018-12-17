
using Emgu.CV;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DominoPlanner.Core
{
    [ProtoContract]
    public class CircleParameters : RectangleDominoProvider
    {

        int _tangential_width = 24;
        [ProtoMember(1)]
        public int TangentialWidth
        {
            get
            {
                return _tangential_width;
            }
            set
            {
                if (value > 1)
                {
                    _tangential_width = value;
                    shapesValid = false;
                }
            }
        }
        int _normal_width = 8;
        [ProtoMember(2)]
        public int NormalWidth
        {
            get
            {
                return _normal_width;
            }
            set
            {
                if (value > 1)
                {
                    _normal_width = value;
                    shapesValid = false;
                }
            }
        }

        int _tangential_distance = 8;
        [ProtoMember(3)]
        public int TangentialDistance
        {
            get
            {
                return _tangential_distance;

            }
            set
            {
                if (value > 1)
                {
                    _tangential_distance = value;
                    shapesValid = false;
                }
            }
        }
        int _normal_distance = 8;
        [ProtoMember(4)]
        public int NormalDistance
        {
            get
            {
                return _normal_distance;
            }
            set
            {
                if (value > 1)
                {
                    _normal_distance = value;
                    shapesValid = false;
                }
            }
        }
        int _circles;
        [ProtoMember(5)]
        public int Circles
        {
            get
            {
                return _circles;
            }
            set
            {
                if (value > 4 && value < 5000)
                {
                    _circles = value;
                    shapesValid = false;
                }

            }
        }
        int _start_diameter = 0;
        [ProtoMember(6)]
        public int StartDiameter
        {
            get
            {
                return _start_diameter;
            }
            set
            {
                if (value > _normal_width * 2)
                {
                    _start_diameter = value;
                    shapesValid = false;
                }

            }
        }
        double? _angle_shift_factor = 0.05;
        
        [ProtoMember(7)]
        public double? AngleShiftFactor
        {
            get
            {
                if (_angle_shift_factor != null)
                    return _angle_shift_factor;
                else
                    return r.NextDouble() * 3.141 * 2;
            }
            set
            {
                _angle_shift_factor = value;
                shapesValid = false;

            }
        }
        private int _force_divisibilty = 1;
        [ProtoMember(8)]
        public int ForceDivisibility
        {
            get
            {
                return _force_divisibilty;
            }
            set
            {
                if (value >= 1)
                {
                    _force_divisibilty = value;
                }
            }
        }
        private Random r;

        public CircleParameters(string imagepath, int circles,
            string colors, IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode,
            IterationInformation iterationInformation, bool allowStretch = false) :
            base(imagepath, colors, colorMode, ditherMode, averageMode, allowStretch, iterationInformation)
        {

            this.StartDiameter = 4 * TangentialWidth;
            Circles = circles;
            hasProcotolDefinition = true;
            r = new Random();
        }
        public CircleParameters(int imageWidth, int imageHeight, Color background, int circles,
            string colors, IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode,
            IterationInformation iterationInformation, bool allowStretch = false) :
            base(imageWidth, imageHeight, background, colors, colorMode, ditherMode, averageMode, allowStretch, iterationInformation)
        {

            this.StartDiameter = 4 * TangentialWidth;
            Circles = circles;
            hasProcotolDefinition = true;
            r = new Random();
        }
        private CircleParameters() : base() { r = new Random(); }
        internal override void GenerateShapes()
        {
            PathDomino[][] dominos = new PathDomino[Circles][];
            Parallel.For(0,  Circles,  new ParallelOptions() { MaxDegreeOfParallelism = -1 },
            (circlecount) =>
            {
                
                int diameter = StartDiameter + circlecount * (2 * NormalWidth + 2 * NormalDistance);

                double domino_angle = Math.Asin((double)TangentialWidth / diameter) * 2;
                double distance_angle = Math.Asin((double)TangentialDistance / diameter) * 2;
                int current_domino_count = (int)Math.Floor(2 * Math.PI / ((double)domino_angle + distance_angle));
                current_domino_count = (int)Math.Round((double)current_domino_count / _force_divisibilty) * _force_divisibilty;
                // equally space the distance between all dominoes
                distance_angle = (2 * Math.PI - (domino_angle * current_domino_count)) / current_domino_count;
                // calculate dominoes
                double angle = (double)AngleShiftFactor*circlecount;
                dominos[circlecount] = new PathDomino[current_domino_count];
                for (int i = 0; i < current_domino_count; i++)
                {
                    PathDomino d = GenerateDomino(diameter, angle, domino_angle);
                    angle += domino_angle + distance_angle;
                    d.position = new ProtocolDefinition() { x = i, y = circlecount };
                    dominos[circlecount][i] = d;
                }
            });
            IDominoShape[] dominoes = dominos.SelectMany(x => x).ToArray();
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
            g.HasProtocolDefinition = true;
            g.dominoes = dominoes;
            g.width = x_max - x_min;
            g.height = y_max - y_min;
            shapes = g;
            shapesValid = true;
        }



        private PathDomino GenerateDomino(int diameter, double angle, double domino_angle)
        {
            double normal_angle = angle + domino_angle / 2;
            double x1 = diameter / 2d * Math.Cos(angle);
            double y1 = diameter / 2d * Math.Sin(angle);
            double x2 = diameter / 2d * Math.Cos(angle + domino_angle);
            double y2 = diameter / 2d * Math.Sin(angle + domino_angle);
            double x4 = diameter / 2d * Math.Cos(angle) + Math.Cos(normal_angle) * NormalWidth;
            double y4 = diameter / 2d * Math.Sin(angle) + Math.Sin(normal_angle) * NormalWidth;
            double x3 = diameter / 2d * Math.Cos(angle + domino_angle) + Math.Cos(normal_angle) * NormalWidth;
            double y3 = diameter / 2d * Math.Sin(angle + domino_angle) + Math.Sin(normal_angle) * NormalWidth;
            PathDomino d = new PathDomino()
            {
                points = new Point[] { new Point(x1, y1), new Point(x2, y2), new Point(x3, y3), new Point(x4, y4) },
                position = new ProtocolDefinition() { x = 1, y = 1 }

            };
            return d;

        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
