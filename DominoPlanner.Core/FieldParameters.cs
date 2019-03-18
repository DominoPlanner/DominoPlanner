﻿using Emgu.CV.CvEnum;
using ProtoBuf;
using System;
using System.Windows.Media;

namespace DominoPlanner.Core
{
    [ProtoContract]
    public partial class FieldParameters : IDominoProvider, ICountTargetable
    {
        #region properties
        public int TargetCount
        {
            set
            {
                double tempwidth = Math.Sqrt((double)PrimaryImageTreatment.Height * (HorizontalDistance + HorizontalSize) * value * (VerticalSize + VerticalDistance) * PrimaryImageTreatment.Width) / (PrimaryImageTreatment.Height * (HorizontalDistance + HorizontalSize));
                double tempheight = Math.Sqrt((double)PrimaryImageTreatment.Height * (HorizontalDistance + HorizontalSize) * value * (VerticalSize + VerticalDistance) * PrimaryImageTreatment.Width) / (PrimaryImageTreatment.Width * (VerticalSize + VerticalDistance));
                if (tempwidth < tempheight)
                {
                    Length = (int)Math.Round(tempwidth);
                    Height = (int)(value / (double)Length);
                }
                else
                {
                    Height = (int)Math.Round(tempheight);
                    Length = (int)(value / (double)Height);
                }
            }
        }
        private int _horizontalDistance;
        /// <summary>
        /// Der horizontale Abstand zwischen zwei Reihen/Steinen.
        /// </summary>
        [ProtoMember(2)]
        public int HorizontalDistance
        {
            get
            {
                return _horizontalDistance;
            }
            set
            {
                if (value >= 0 && value != _horizontalDistance)
                {
                    _horizontalDistance = value;
                    shapesValid = false;
                }
            }
        }

        private int _horizontalSize;
        /// <summary>
        /// Die horizontale Breite der Steine.
        /// </summary>
        [ProtoMember(3)]
        public int HorizontalSize
        {
            get
            {
                return _horizontalSize;
            }
            set
            {
                if (value > 0 && value != _horizontalSize)
                {
                    _horizontalSize = value;
                    shapesValid = false;
                }
            }
        }

        private int _verticalSize;
        /// <summary>
        /// Die vertikale Breite der Steine.
        /// </summary>
        [ProtoMember(4)]
        public int VerticalSize
        {
            get
            {
                return _verticalSize;
            }
            set
            {
                if (value > 0 && value != _verticalSize)
                {
                    _verticalSize = value;
                    shapesValid = false;
                }
            }
        }

        private int _verticalDistance;
        /// <summary>
        /// Der vertikale Abstand zwischen zwei Steinen/Reihen.
        /// </summary>
        [ProtoMember(5)]
        public int VerticalDistance
        {
            get
            {
                return _verticalDistance;
            }
            set
            {
                if (value >= 0 && value != _verticalDistance)
                {
                    _verticalDistance = value;
                    shapesValid = false;
                }
            }
        }

        
        private int _length;
        /// <summary>
        /// Die horizontale Steineanzahl.
        /// </summary>
        [ProtoMember(8)]
        public int Length
        {
            get
            {
                return _length;
            }
            set
            {
                if (value > 0 && value != _length)
                {
                    _length = value;
                    current_width = value;
                    shapesValid = false;
                }
            }
        }
        private int _height;
        /// <summary>
        /// Die vertikale Steineanzahl.
        /// </summary>
        [ProtoMember(9)]
        public int Height
        {
            get
            {
                return _height;
            }
            set
            {
                if (value > 0 && value != _height)
                {
                    _height = value;
                    shapesValid = false;
                }
            }
        }
        #endregion properties
        #region constructors
        public FieldParameters(string filepath, string imagePath, string colors, int horizontalDistance, int horizontalSize, int verticalSize, int verticalDistance, int width, int height,
            Inter scalingMode, IColorComparison colorMode, Dithering ditherMode, IterationInformation iterationInformation) : base(filepath)
        {
            ColorPath = colors;
            HorizontalDistance = horizontalDistance;
            HorizontalSize = horizontalSize;
            VerticalSize = verticalSize;
            VerticalDistance = verticalDistance;
            Length = width;
            Height = height;
            PrimaryImageTreatment = new FieldReadout(this, imagePath, scalingMode);
            PrimaryCalculation = new FieldCalculation(colorMode, ditherMode, iterationInformation);
            HasProtocolDefinition = true;
        }
        public FieldParameters(string filepath, string imagePath, string colors, int horizontalDistance, int horizontalSize, int verticalSize, int verticalDistance, int targetSize,
            Inter scalingMode, IColorComparison colorMode, Dithering ditherMode, IterationInformation iterationInformation)
            : this(filepath, imagePath, colors, horizontalDistance, horizontalSize, verticalSize, verticalDistance, 1, 1, scalingMode, colorMode, ditherMode, iterationInformation)
        {
            TargetCount = targetSize;
        }
        public FieldParameters(int imageWidth, int imageHeight, Color background, string colors, int horizontalDistance, int horizontalSize, int verticalSize, int verticalDistance, int targetSize,
            Inter scalingMode, IColorComparison colorMode, Dithering ditherMode, IterationInformation iterationInformation)
        {

            ColorPath = colors;
            HorizontalDistance = horizontalDistance;
            HorizontalSize = horizontalSize;
            VerticalSize = verticalSize;
            VerticalDistance = verticalDistance;
            TargetCount = targetSize;
            PrimaryImageTreatment = new FieldReadout(this, imageWidth, imageHeight, scalingMode);
            PrimaryImageTreatment.Background = background;
            PrimaryCalculation = new FieldCalculation(colorMode, ditherMode, iterationInformation);
            HasProtocolDefinition = true;
        }
        private FieldParameters() { }
        #endregion
        #region overrides
        protected override void RegenerateShapes()
        {
            last = new DominoTransfer(getNewShapes(Length, Height), colors);
            shapesValid = true;
        }
        public override int[,] GetBaseField(Orientation o = Orientation.Horizontal)
        {
            if (!lastValid) throw new InvalidOperationException("There are unreflected changes in this field.");
            current_width = last.FieldPlanLength;
            current_height = last.FieldPlanHeight;
            int[,] result = new int[current_width, current_height];
            for (int i = 0; i < current_width; i++)
            {
                for (int j = 0; j < current_height; j++)
                {
                    result[i, j] = last.shapes[j * current_width + i].color;
                }
            }
            if (o == Orientation.Vertical) result = TransposeArray(result);
            return result;
        }
        #endregion
        #region compatibility properties
        
        [ProtoMember(6)]
        private Inter resizeMode
        {
            get
            {
                return ((FieldReadout)PrimaryImageTreatment)?.ResizeMode ?? Inter.Cubic; 
            }
            set
            {
                ((FieldReadout)CreatePrimaryTreatment()).ResizeMode = value;
            }
        }
        
        #endregion
    }

}
