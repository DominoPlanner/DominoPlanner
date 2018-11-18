﻿using ColorMine.ColorSpaces.Comparisons;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DominoPlanner.Core
{
    public class CircleParameters : RectangleDominoProvider
    {
        
            int _tangential_width;
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
                    }
                }
            }
            int _normal_width;
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
                    }
                }
            }

            int _tangential_distance;
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
                    }
                }
            }
            int _normal_distance;
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
                    }
                }
            }
            int _rotations;
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
                    }

                }
            }
            int _start_diameter;
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
                    }

                }
            }

        public override int targetCount { set => throw new NotImplementedException(); }

        public CircleParameters(Mat bitmap, int rotations, int normalWidth, int tangentialWidth, int normalDistance, int tangentialDistance, 
            List<DominoColor> colors, IColorSpaceComparison colorMode, AverageMode averageMode, 
            IterationInformation iterationInformation,  bool allowStretch = false) :
            base(bitmap, colors, colorMode, averageMode, allowStretch, iterationInformation)
        {
            this.rotations = rotations;
            this.normalDistance = normalDistance;
            this.normalWidth = normalWidth;
            this.tangentialDistance = tangentialDistance;
            this.tangentialWidth = tangentialWidth;
            hasProcotolDefinition = true;
        }

        protected override void GenerateShapes()
        {
            List<PathDomino> dominos = new List<PathDomino>();
            int circlecount = 0;
            int diameter = start_diameter;
            while (circlecount < rotations)
            {
                diameter += 2 * normalWidth + 2 * normalDistance;
                // get number of dominoes in this spiral
                double domino_angle = Math.Asin((double)tangentialWidth / diameter) * 2;
                double distance_angle = Math.Asin((double)tangentialDistance / diameter) * 2;
                int current_domino_count = (int)Math.Floor(2 * Math.PI / ((double)domino_angle + distance_angle));
                // equally space the distance between all dominoes
                distance_angle = (2 * Math.PI - (domino_angle * current_domino_count)) / current_domino_count;
                // calculate dominoes
                double angle = 0;
                for (int i = 0; i < current_domino_count; i++)
                {
                    PathDomino d = GenerateDomino(diameter, angle, domino_angle);
                    angle += domino_angle + distance_angle;
                    d.position = new ProtocolDefinition() { x = i, y = circlecount };
                    dominos.Add(d);
                }
                circlecount++;
            }
            IDominoShape[] dominoes = dominos.ToArray();
            double x_min = dominoes.Min(x => x.GetContainer().x);
            double y_min = dominoes.Min(x => x.GetContainer().y);
            double x_max = dominoes.Max(x => x.GetContainer().width + x.GetContainer().x);
            double y_max = dominoes.Max(x => x.GetContainer().height + x.GetContainer().y);
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
