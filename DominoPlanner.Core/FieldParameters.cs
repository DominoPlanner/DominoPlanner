using ProtoBuf;
using System;
using Avalonia.Media;

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
                    ResetColumnHistory(value);
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
                    ResetRowHistory(value);
                    shapesValid = false;
                }
            }
        }
        #endregion properties
        #region constructors
        public FieldParameters(string filepath, string imagePath, string colors, int horizontalDistance, int horizontalSize, int verticalSize, int verticalDistance, int width, int height,
            SkiaSharp.SKFilterQuality scalingQuality, IColorComparison colorMode, Dithering ditherMode, IterationInformation iterationInformation) : base(filepath)
        {
            ColorPath = colors;
            HorizontalDistance = horizontalDistance;
            HorizontalSize = horizontalSize;
            VerticalSize = verticalSize;
            VerticalDistance = verticalDistance;
            Length = width;
            Height = height;
            PrimaryImageTreatment = new FieldReadout(this, imagePath, scalingQuality);
            PrimaryCalculation = new FieldCalculation(colorMode, ditherMode, iterationInformation);
            HasProtocolDefinition = true;
        }
        public FieldParameters(string filepath, string imagePath, string colors, int horizontalDistance, int horizontalSize, int verticalSize, int verticalDistance, int targetSize,
            SkiaSharp.SKFilterQuality scalingQuality, IColorComparison colorMode, Dithering ditherMode, IterationInformation iterationInformation)
            : this(filepath, imagePath, colors, horizontalDistance, horizontalSize, verticalSize, verticalDistance, 1, 1, scalingQuality, colorMode, ditherMode, iterationInformation)
        {
            TargetCount = targetSize;
        }
        public FieldParameters(int imageWidth, int imageHeight, Color background, string colors, int horizontalDistance, int horizontalSize, int verticalSize, int verticalDistance, int targetSize,
            SkiaSharp.SKFilterQuality scalingQuality, IColorComparison colorMode, Dithering ditherMode, IterationInformation iterationInformation)
        {

            ColorPath = colors;
            HorizontalDistance = horizontalDistance;
            HorizontalSize = horizontalSize;
            VerticalSize = verticalSize;
            VerticalDistance = verticalDistance;
            PrimaryImageTreatment = new FieldReadout(this, imageWidth, imageHeight, scalingQuality)
            {
                Background = background
            };
            TargetCount = targetSize;
            PrimaryCalculation = new FieldCalculation(colorMode, ditherMode, iterationInformation);
            HasProtocolDefinition = true;
        }
        private FieldParameters() { }
        #endregion
        #region overrides
        public override void RegenerateShapes()
        {
            Last = new DominoTransfer(getNewShapes(Length, Height), colors);
            shapesValid = true;
        }
        public override int[,] GetBaseField(Orientation o = Orientation.Horizontal, bool MirrorX = false, bool MirrorY = false)
        {
            if (!LastValid) throw new InvalidOperationException("There are unreflected changes in this field.");
            if (current_width != Last.FieldPlanLength)
            {
                //Likely, something went wrong during row/column operations. Discard history
                ResetSize(Last.FieldPlanLength);
            }
            int[,] result = new int[current_width, current_height];
            for (int i = 0; i < current_width; i++)
            {
                for (int j = 0; j < current_height; j++)
                {
                    result[i, j] = Last.shapes[j * current_width + i].Color;
                }
            }
            if (o == Orientation.Vertical) result = TransposeArray(result);
            if (MirrorX == true)
            {
                result = MirrorArrayX(result);
            }
            if (MirrorY == true)
            {
                result = MirrorArrayY(result);
            }
            return result;
        }
        #endregion
        #region compatibility properties
        
        /*[ProtoMember(6)]
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
        }*/
        
        #endregion
    }

}
