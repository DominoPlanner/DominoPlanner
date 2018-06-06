using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using ImageProcessor.Imaging.Filters.Photo;
using ImageProcessor.Processors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DominoPlanner
{
    [Serializable]
    public abstract class Filter : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler tempHandler = PropertyChanged;
            if (tempHandler != null)
                tempHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        public abstract void Apply(ImageFactory fs);

        public void Edit()
        {
            
        }

        public abstract string PropertiesToString();

        public String name
        {
            get; set;
        }
        public String tooltip { get; set; }
    }
    [Serializable]
    public class AlphaFilter : Filter
    {
        public int percentage
        {
            get; set;
        }

        public AlphaFilter()
        {
            this.name = "Alpha";
            tooltip = "Changes the opacity of the current image";
            percentage = 30;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Alpha(percentage);
        }

        public override string PropertiesToString()
        {
            return "Percentage: " + percentage;
        }
    }
    [Serializable]
    public class BackgroundColorFilter : Filter
    {
        private System.Drawing.Color _color;
        public System.Drawing.Color color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("color");
            }
        }

        public BackgroundColorFilter()
        {
            this.name = "Background Color";
            tooltip = "Changes the background color (i.e. opaque areas) of the current image";
            color = System.Drawing.Color.Gray;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.BackgroundColor(color);
        }

        public override string PropertiesToString()
        {
            return "Background Color: " + color.ToString();
        }
    }
    [Serializable]
    public class BrightnessFilter : Filter
    {
        public int percentage
        {
            get; set;
        }

        public BrightnessFilter()
        {
            this.name = "Brightness";
            tooltip = "Changes the brightness of the current image";
            percentage = 50;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Brightness(percentage);
        }

        public override string PropertiesToString()
        {
            return "Percentage: " + percentage;
        }
    }
    [Serializable]
    public class ContrastFilter : Filter
    {
        public int percentage
        {
            get; set;
        }

        public ContrastFilter()
        {
            this.name = "Contrast";
            tooltip = "Changes the contrast of the current image";
            percentage = 50;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Contrast(percentage);
        }

        public override string PropertiesToString()
        {
            return "Percentage: " + percentage;
        }
    }
    [Serializable]
    public class CropFilter : Filter
    {
        private float _left;
        private float _right;
        private float _bottom;
        private float _top;
        public float left
        {
            get
            {
                return _left;
            }
            set
            {
                
                _left = value;

                OnPropertyChanged("");
            }
        }
        public float right
        {
            get
            {
                return _right;
            }
            set
            {
                _right = value;
                OnPropertyChanged("");
            }
        }
        public float top
        {
            get
            {
                return _top;
            }
            set
            {
                _top = value;
                OnPropertyChanged("");
            }
        }
        public float bottom
        {
            get
            {
                return _bottom;
            }
            set
            {
                _bottom = value;
                OnPropertyChanged("");
            }
        }
        public CropFilter()
        {
            this.name = "Crop";
            tooltip = "Crops the image to the given location and size, i.e. removing left, top, right and bottom margin";
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Crop(new CropLayer(_left, _top, _right, _bottom, CropMode.Percentage));
        }

        public override string PropertiesToString()
        {
                return "Left: " + _left + "%, Top: " + _top + "%, Right: " + _right + "%, Bottom: " + _bottom + "%";
        }
    }
    [Serializable]
    public class EdgeDetectionFilter : Filter
    {
        public int index
        {
            get
            {
                if (filter is KayyaliEdgeFilter) return 0;
                else if (filter is KirschEdgeFilter) return 1;
                else if (filter is Laplacian3X3EdgeFilter) return 2;
                else if (filter is Laplacian5X5EdgeFilter) return 3;
                else if (filter is LaplacianOfGaussianEdgeFilter) return 4;
                else if (filter is PrewittEdgeFilter) return 5;
                else if (filter is RobertsCrossEdgeFilter) return 6;
                else if (filter is ScharrEdgeFilter) return 7;
                else if (filter is SobelEdgeFilter) return 8;
                return -1;
            }
            set
            {
                switch (value)
                {
                    case 0: filter = new KayyaliEdgeFilter(); break;
                    case 1: filter = new KirschEdgeFilter(); break;
                    case 2: filter = new Laplacian3X3EdgeFilter(); break;
                    case 3: filter = new Laplacian5X5EdgeFilter(); break;
                    case 4: filter = new LaplacianOfGaussianEdgeFilter(); break;
                    case 5: filter = new PrewittEdgeFilter(); break;
                    case 6: filter = new RobertsCrossEdgeFilter(); break;
                    case 7: filter = new ScharrEdgeFilter(); break;
                    case 8: filter = new SobelEdgeFilter(); break;
                    
                }
                OnPropertyChanged(null);
            }

        }
        [NonSerialized]
        private IEdgeFilter _filter;
        public IEdgeFilter filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
            }
        }
        public bool greyscale
        { get; set; }
        public EdgeDetectionFilter()
        {
            this.name = "Detect Edges";
            tooltip = "Detects the edges in the current image using various algorithms. \nEverything but the edges will be black, \nedge colors will remain if greyscale is false.";
            filter = new LaplacianOfGaussianEdgeFilter();
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.DetectEdges(filter, greyscale);
        }

        public override string PropertiesToString()
        {
            return "Filter: " + filter.GetType().Name.Replace("EdgeFilter", "") + ", " + (greyscale ? "greyscale" : "colored");
        }
    }
    [Serializable]
    public class EntropyCropFilter : Filter
    {
        public byte threshold
        {
            get; set;
        }

        public EntropyCropFilter()
        {
            this.name = "Entropy Crop";
            tooltip = "Crops an image to the area of greatest entropy. \n Removes e.g. black or white spaces around the image";
            threshold = 128;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.EntropyCrop(threshold);
        }

        public override string PropertiesToString()
        {
            return "Threshold: " + threshold;
        }
    }
    [Serializable]
    public class MatrixFilter : Filter
    {
        private int _index;
        public int index
        {
            get
            {
                return _index;
            }
            set
            {
                switch (value)
                {
                    case 0:
                        id = "Black White";
                        matrix = MatrixFilters.BlackWhite;
                        break;
                    case 1:
                        id = "Comic";
                        matrix = MatrixFilters.Comic;
                        break;
                    case 2:
                        id = "Gotham";
                        matrix = MatrixFilters.Gotham;
                        break;
                    case 3:
                        id = "Greyscale";
                        matrix = MatrixFilters.GreyScale;
                        break;
                    case 4:
                        id = "HiSatch";
                        matrix = MatrixFilters.HiSatch;
                        break;
                    case 5:
                        id = "Invert";
                        matrix = MatrixFilters.Invert;
                        break;
                    case 6:
                        id = "Lomograph";
                        matrix = MatrixFilters.Lomograph;
                        break;
                    case 7:
                        id = "LoSatch";
                        matrix = MatrixFilters.LoSatch;
                        break;
                    case 8:
                        id = "Polaroid";
                        matrix = MatrixFilters.Polaroid;
                        break;
                    case 9:
                        id = "Sepia";
                        matrix = MatrixFilters.Sepia;
                        break;
                }
                OnPropertyChanged("");
                _index = value;
            }
        }
        [NonSerialized]
        private IMatrixFilter matrix;
        private String id;
        public MatrixFilter()
        {
            this.name = "Filter";
            tooltip = "Applies a filter to the current image";
            index = 2;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Filter(matrix);
        }

        public override string PropertiesToString()
        {
            return "Filter: " +  id;
        }

    }
    [Serializable]
    public class FlipFilter : Filter
    {
        public bool vertical
        {
            get; set;
        }

        public FlipFilter()
        {
            this.name = "Flip";
            tooltip = "Flips the current image horizontally or vertically";
            vertical = false;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Flip(vertical);
        }

        public override string PropertiesToString()
        {
            return "Direction: " + (vertical? "Vertical" : "Horizontal");
        }
    }
    [Serializable]
    public class GaussianBlurFilter : Filter
    {
        public int size
        {
            get; set;
        }
        public double sigma
        {
            get; set;
        }
        public int threshold
        {
            get; set;
        }

        public GaussianBlurFilter()
        {
            this.name = "Gaussian Blur";
            tooltip = "Uses a gaussian Kernel to blur the current image";
            size = 5;
            sigma = 1.4;
            threshold = 0;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.GaussianBlur(new GaussianLayer(size, sigma, threshold));
        }

        public override string PropertiesToString()
        {
            return "Kernel Size: " + size + (sigma != 1.4 ? ", Sigma: " + sigma : "") + (threshold != 0 ? ", Threshold: " + threshold: "");
        }
    }
    [Serializable]
    public class GaussianSharpenFilter : Filter
    {
        public int size
        {
            get; set;
        }
        public double sigma
        {
            get; set;
        }
        public int threshold
        {
            get; set;
        }

        public GaussianSharpenFilter()
        {
            this.name = "Gaussian Sharpen";
            tooltip = "Uses a gaussian Kernel to sharpen the current image";
            size = 5;
            sigma = 1.4;
            threshold = 0;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.GaussianSharpen(new GaussianLayer(size, sigma, threshold));
        }

        public override string PropertiesToString()
        {
            return "Kernel Size: " + size + (sigma != 1.4 ? ", Sigma: " + sigma : "") + (threshold != 0 ? ", Threshold: " + threshold : "");
        }
    }
    [Serializable]
    public class HueFilter : Filter
    {
        public bool rotate
        {
            get; set;
        }
        public int degrees {
            get; set; }

        public HueFilter()
        {
            this.name = "Hue";
            tooltip = "Alters the hue of the current image changing the overall color";
            rotate = true;
            degrees = 180;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Hue(degrees, rotate);
        }

        public override string PropertiesToString()
        {
            return "Angle: " + degrees + ", Rotate: " + rotate;
        }
    }
    [Serializable]
    public class ReplaceFilter : Filter
    {
        System.Drawing.Color _source;
        public System.Drawing.Color source
        {
            get { return _source; }
            set { _source = value; OnPropertyChanged("source"); }
        }
        System.Drawing.Color _target;
        public System.Drawing.Color target
        {
            get { return _target; }
            set { _target = value; OnPropertyChanged("target"); }
        }
        public int threshold
        {
            get; set;
        }

        public ReplaceFilter()
        {
            this.name = "Replace Color";
            tooltip = "Replaces a color within the current image";
            threshold = 64;
            source = System.Drawing.Color.White;
            target = System.Drawing.Color.Black;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.ReplaceColor(source, target, threshold);
        }

        public override string PropertiesToString()
        {
            return "Replace " + System.Drawing.ColorTranslator.ToHtml(source)  + " by " + System.Drawing.ColorTranslator.ToHtml(target) + ", Accuracy: " + threshold;
        }
    }
    [Serializable]
    public class RotateFilter : Filter
    {
        public int degrees
        {
            get; set;
        }
        public RotateFilter()
        {
            this.name = "Rotate";
            tooltip = "Rotates the current image by the given angle without clipping";
            degrees = 45;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Rotate(degrees);
        }

        public override string PropertiesToString()
        {
            return "Degrees: " + degrees;
        }
    }
    [Serializable]
    public class SaturationFilter : Filter
    {
        public int percentage
        {
            get; set;
        }
        public SaturationFilter()
        {
            this.name = "Saturation";
            tooltip = "Changes the saturation of the current image.";
            percentage = 50;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Saturation(percentage);
        }

        public override string PropertiesToString()
        {
            return "Percentage: " + percentage;
        }
    }
    [Serializable]
    public class TintFilter : Filter
    {
        private System.Drawing.Color _color;
        public System.Drawing.Color color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("color");
            }
        }
        public TintFilter()
        {
            this.name = "Tint";
            tooltip = "Tints the image with the given color.";
            color = System.Drawing.Color.LightBlue;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Tint(color);
        }

        public override string PropertiesToString()
        {
            return "Color: " + System.Drawing.ColorTranslator.ToHtml(color);
        }
    }
    [Serializable]
    public class VignetteFilter : Filter
    {
        private System.Drawing.Color _color;
        public System.Drawing.Color color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("color");
            }
        }
        public VignetteFilter()
        {
            this.name = "Vignette";
            tooltip = "Adds a vignette image effect to the current image.";
            color = System.Drawing.Color.LightBlue;
        }

        public override void Apply(ImageFactory fs)
        {
            fs = fs.Vignette(color);
        }

        public override string PropertiesToString()
        {
            return "Color: " + System.Drawing.ColorTranslator.ToHtml(color);
        }
    }
}
