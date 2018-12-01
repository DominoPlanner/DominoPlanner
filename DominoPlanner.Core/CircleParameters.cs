
using Emgu.CV;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DominoPlanner.Core
{
    [ProtoContract]
    public class CircleParameters : RectangleDominoProvider
    {

        int _tangential_width;
        [ProtoMember(1)]
        public int tangentialWidth
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
        int _normal_width;
        [ProtoMember(2)]
        public int normalWidth
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

        int _tangential_distance;
        [ProtoMember(3)]
        public int tangentialDistance
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
        int _normal_distance;
        [ProtoMember(4)]
        public int normalDistance
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
        int _rotations;
        [ProtoMember(5)]
        public int rotations
        {
            get
            {
                return _rotations;
            }
            set
            {
                if (value > 4 && value < 5000)
                {
                    _rotations = value;
                    shapesValid = false;
                }

            }
        }
        int _start_diameter;
        [ProtoMember(6)]
        public int start_diameter
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
        double? _angle_shift_factor;
        
        [ProtoMember(7)]
        public double? angle_shift_factor
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
        private Random r;
        public override int targetCount { set => throw new NotImplementedException(); }

        public CircleParameters(Mat bitmap, int rotations, int normalWidth, int tangentialWidth, int normalDistance, int tangentialDistance,
            string colors, IColorComparison colorMode, AverageMode averageMode,
            IterationInformation iterationInformation, bool allowStretch = false) :
            base(bitmap, colors, colorMode, averageMode, allowStretch, iterationInformation)
        {
            this.rotations = rotations;
            this.normalDistance = normalDistance;
            this.normalWidth = normalWidth;
            this.tangentialDistance = tangentialDistance;
            this.tangentialWidth = tangentialWidth;
            this.start_diameter = 4 * tangentialWidth;
            hasProcotolDefinition = true;
            r = new Random();
        }
        private CircleParameters() : base() { }
        protected override void GenerateShapes()
        {
            PathDomino[][] dominos = new PathDomino[rotations][];
            Parallel.For(0,  rotations,  new ParallelOptions() { MaxDegreeOfParallelism = -1 },
            (circlecount) =>
            {
                
                int diameter = start_diameter + circlecount * (2 * normalWidth + 2 * normalDistance);

                double domino_angle = Math.Asin((double)tangentialWidth / diameter) * 2;
                double distance_angle = Math.Asin((double)tangentialDistance / diameter) * 2;
                int current_domino_count = (int)Math.Floor(2 * Math.PI / ((double)domino_angle + distance_angle));
                // equally space the distance between all dominoes
                distance_angle = (2 * Math.PI - (domino_angle * current_domino_count)) / current_domino_count;
                // calculate dominoes
                double angle = (double)angle_shift_factor*circlecount;
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
            double x4 = diameter / 2d * Math.Cos(angle) + Math.Cos(normal_angle) * normalWidth;
            double y4 = diameter / 2d * Math.Sin(angle) + Math.Sin(normal_angle) * normalWidth;
            double x3 = diameter / 2d * Math.Cos(angle + domino_angle) + Math.Cos(normal_angle) * normalWidth;
            double y3 = diameter / 2d * Math.Sin(angle + domino_angle) + Math.Sin(normal_angle) * normalWidth;
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
