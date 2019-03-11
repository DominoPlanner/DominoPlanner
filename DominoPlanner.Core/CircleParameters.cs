
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
    public class CircleParameters : CircularStructure
    {
        int _circles;
        [ProtoMember(1)]
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
        int _start_diameter = 20;
        [ProtoMember(2)]
        public int StartDiameter
        {
            get
            {
                return _start_diameter;
            }
            set
            {
                if (value > DominoLength * 2)
                {
                    _start_diameter = value;
                    shapesValid = false;
                }

            }
        }
        double? _angle_shift_factor = 0.05;
        
        [ProtoMember(3)]
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
        [ProtoMember(4)]
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

        public CircleParameters(string filepath, string imagepath, int circles,
            string colors, IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode,
            IterationInformation iterationInformation, bool allowStretch = false) :
            base(filepath, imagepath, colors, colorMode, ditherMode, averageMode, iterationInformation, allowStretch)
        {
            init(circles);
        }
        public CircleParameters(int imageWidth, int imageHeight, Color background, int circles,
            string colors, IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode,
            IterationInformation iterationInformation, bool allowStretch = false) :
            base(imageWidth, imageHeight, background, colors, colorMode, ditherMode, averageMode, iterationInformation, allowStretch)
        {

            init(circles);
        }
        private void init(int circles)
        {
            
            Circles = circles;
            HasProtocolDefinition = true;
            r = new Random();
            DominoWidth = 8;
            DominoLength = 24;
            NormalDistance = 8;
            TangentialDistance = 8;
            this.StartDiameter = 4 * DominoLength;
        }
        private CircleParameters() : base() { r = new Random(); }
        protected override void RegenerateShapes()
        {
            PathDomino[][] dominos = new PathDomino[Circles][];
            Parallel.For(0,  Circles,  new ParallelOptions() { MaxDegreeOfParallelism = -1 },
            (circlecount) =>
            {
                int diameter = StartDiameter + circlecount * (2 * DominoWidth + 2 * NormalDistance);
                double domino_angle = Math.Asin((double)DominoLength / diameter) * 2;
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
                    PathDomino d = GenerateDomino(diameter, angle);
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
            last = new DominoTransfer(dominoes, colors);
            shapesValid = true;
        }
        private PathDomino GenerateDomino(int diameter, double angle)
        {
            double x1 = diameter / 2d * Math.Cos(angle);
            double y1 = diameter / 2d * Math.Sin(angle);
            return CreateDominoAtCoordinates(x1, y1, angle, 1, 1);

        }
    }
}
