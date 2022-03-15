
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Media;

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
                if (value > 0 && value < 5000)
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
        [ProtoMember(3)]
        double? _angle_shift_factor = 0.05;
        
        public bool RandomShiftFactor
        {
            get { return _angle_shift_factor == null; }
        }
        
        public double? AngleShiftFactor
        {
            get
            {
                if (_angle_shift_factor != null)
                    return (double)_angle_shift_factor;
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
                    shapesValid = false;
                }
            }
        }

        private bool _TryUsePerfectInnerCircle = false;
        [ProtoMember(5)]
        public bool TryUsePerfectInnerCircle
        {
            get
            {
                return _TryUsePerfectInnerCircle;
            }
            set
            {
                if (_TryUsePerfectInnerCircle != value)
                {
                    _TryUsePerfectInnerCircle = value;
                    shapesValid = false;
                }
            }
        }

        private Random r;

        public CircleParameters(string filepath, string imagepath, int circles,
            string colors, IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode,
            IterationInformation iterationInformation, bool allowStretch = false) :
            base(filepath, imagepath, colors, colorMode, ditherMode, averageMode, iterationInformation, allowStretch)
        {
            Init(circles);
        }
        public CircleParameters(int imageWidth, int imageHeight, Color background, int circles,
            string colors, IColorComparison colorMode, Dithering ditherMode, AverageMode averageMode,
            IterationInformation iterationInformation, bool allowStretch = false) :
            base(imageWidth, imageHeight, background, colors, colorMode, ditherMode, averageMode, iterationInformation, allowStretch)
        {

            Init(circles);
        }
        private void Init(int circles)
        {
            
            Circles = circles;
            HasProtocolDefinition = true;
            r = new Random();
            DominoWidth = 8;
            DominoLength = 24;
            NormalDistance = 8;
            TangentialDistance = 8;
            StartDiameter = 4 * DominoLength;
        }
        private CircleParameters() : base() { r = new Random(); }
        public override void RegenerateShapes()
        {
            PathDomino[][] dominos = new PathDomino[Circles][];

            if (TryUsePerfectInnerCircle)
            {
                _RegenerateShapes_CalcInnerTwoCircles(dominos);
            }
            _RegenerateShapes_CalcOuterCircles(dominos);
            
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
            Last = new DominoTransfer(dominoes, colors);
            shapesValid = true;
        }

        private void _RegenerateShapes_CalcOuterCircles(PathDomino[][] dominos)
        {
            Parallel.For(TryUsePerfectInnerCircle ? 2 : 0, Circles, new ParallelOptions() { MaxDegreeOfParallelism = -1 },
            (circlecount) =>
            {
                int diameter = _CalcDiameter(circlecount);
                double domino_angle = _CalcAngle(DominoLength, diameter);
                double distance_angle = _CalcAngle(TangentialDistance, diameter);
                int current_domino_count = calcDominoAmount(domino_angle, distance_angle);
                // equally space the distance between all dominoes
                distance_angle = (2 * Math.PI - (domino_angle * current_domino_count)) / current_domino_count;
                _RegenerateShapes_CalcDominoPathes(dominos, circlecount, diameter, domino_angle, distance_angle, current_domino_count, (double)AngleShiftFactor);
            });
        }

        private void _RegenerateShapes_CalcInnerTwoCircles(PathDomino[][] dominos)
        {
            double firstStoneAngle = 0;
            int firstCircleStoneAmount = 1;

            for (int circlecount = 0; circlecount < 2; circlecount++)
            {
                int diameter = _CalcDiameter(circlecount);
                double domino_angle = _CalcAngle(DominoLength, diameter);

                double currentTangentialDistance = TangentialDistance;
                double distance_angle = _CalcAngle(TangentialDistance, diameter);
                int current_domino_count = calcDominoAmount(domino_angle, distance_angle);

                while (circlecount == 1 && current_domino_count != firstCircleStoneAmount * 2 && currentTangentialDistance > 2)
                {
                    currentTangentialDistance--;
                    distance_angle = _CalcAngle(currentTangentialDistance, diameter);
                    current_domino_count = calcDominoAmount(domino_angle, distance_angle);
                }

                firstCircleStoneAmount = current_domino_count;
                // equally space the distance between all dominoes
                distance_angle = (2 * Math.PI - (domino_angle * current_domino_count)) / current_domino_count;

                double usedAngleShiftFactor = (double)AngleShiftFactor;
                if (circlecount == 1)
                {
                    usedAngleShiftFactor = (firstStoneAngle - (domino_angle + distance_angle)) / 2;
                }
                // calculate dominoes
                double angle = (double)usedAngleShiftFactor * circlecount;
                angle += domino_angle + distance_angle;
                if (circlecount == 0) firstStoneAngle = angle;

                _RegenerateShapes_CalcDominoPathes(dominos, circlecount, diameter, domino_angle, distance_angle, current_domino_count, usedAngleShiftFactor);
            }
        }

        private void _RegenerateShapes_CalcDominoPathes(PathDomino[][] dominos, int circlecount, int diameter, double domino_angle, double distance_angle, int current_domino_count, double angleShiftFactor)
        {
            double angle = angleShiftFactor * circlecount;
            dominos[circlecount] = new PathDomino[current_domino_count];
            for (int i = 0; i < current_domino_count; i++)
            {
                PathDomino d = GenerateDomino(diameter, angle);
                angle += domino_angle + distance_angle;
                d.position = new ProtocolDefinition() { x = i, y = circlecount };
                dominos[circlecount][i] = d;
            }
        }

        private int _CalcDiameter(int circleCount)
        {
            return StartDiameter + circleCount * (2 * DominoWidth + 2 * NormalDistance);
        }

        private double _CalcAngle(double value, int diameter)
        {
            return Math.Asin(value / diameter) * 2;
        }

        private int calcDominoAmount(double domino_angle, double distance_angle)
        {
            double current_domino_count = Math.Floor(2 * Math.PI / (domino_angle + distance_angle));
            return (int)Math.Floor(current_domino_count / _force_divisibilty) * _force_divisibilty;
        }

        private PathDomino GenerateDomino(int diameter, double angle)
        {
            double x1 = diameter / 2d * Math.Cos(angle);
            double y1 = diameter / 2d * Math.Sin(angle);
            return CreateDominoAtCoordinates(x1, y1, angle, 1, 1);

        }
    }
}
