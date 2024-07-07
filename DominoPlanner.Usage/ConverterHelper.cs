using Avalonia.Data.Converters;
using DominoPlanner.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using Avalonia.Controls;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Data;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Controls.Shapes;
using ThemeEditor.Controls.ColorPicker;

namespace DominoPlanner.Usage
{
    using static Localizer;
    class ConverterHelper
    {
    }
    public class AmountToColorConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[2] == null || values[1] == null)
                return Brushes.Black;
            if (int.TryParse(values[0].ToString(), out int anzahl) && int.TryParse(values[2].ToString(), out int gesamt))
            {
                if (anzahl > gesamt)
                {
                    return Brushes.Red;
                }
            }
            if (values[1] is bool b && b)
            {
                return Brushes.Gray;
            }
            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToHTMLConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";
            if (value is Color c)
            {
                return string.Format("#{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
            }
            System.Drawing.Color c2 = (System.Drawing.Color)value;
            return System.Drawing.ColorTranslator.ToHtml(c2);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            else
            {
                System.Drawing.Color c = (System.Drawing.Color)value;
                return new SolidColorBrush(Color.FromArgb(c.A, c.R, c.G, c.B));

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PathToImageConverter : IValueConverter
    {
        public static Bitmap GetIcon(string iconpath)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var bitmap = new Bitmap(assets.Open(new Uri("avares://DominoPlanner.Usage" + iconpath)));
            return bitmap;

        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string path = ImageHelper.GetImageOfFile(value.ToString());
                if (path.StartsWith("/Icons/"))
                {
                    return new Image()
                    {
                        Source = GetIcon(path)
                    };
                }
                else
                {
                    return new Image() { Source = new Bitmap(ImageHelper.GetImageOfFile(value.ToString())) };
                }
            }
            catch (IOException)
            {
                // we're probably still writing the file - not a good idea.
                return null;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FilterQualityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SkiaSharp.SKFilterQuality)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class FilterQualityToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (targetType.IsEnum)
            {
                var val = (int)(double)value;
                return val switch
                {
                    0 => SkiaSharp.SKFilterQuality.Low,
                    1 => SkiaSharp.SKFilterQuality.Medium,
                    2 => SkiaSharp.SKFilterQuality.High,
                    _ => SkiaSharp.SKFilterQuality.Low,
                };
            }

            if (value.GetType().IsEnum)
            {
                var val = (SkiaSharp.SKFilterQuality)value;
                return val switch
                {
                    SkiaSharp.SKFilterQuality.Low => 0,
                    SkiaSharp.SKFilterQuality.Medium => 1,
                    SkiaSharp.SKFilterQuality.High => 2,
                    _ => 0,
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // perform the same conversion in both directions
            return Convert(value, targetType, parameter, culture);
        }
    }
    public struct DictHelper
    {
        public int index;
        public Type type;
        public string name;
    }
    public class DitheringToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                FloydSteinbergDithering _ => _("Floyd/Steinberg Dithering"),
                JarvisJudiceNinkeDithering _ => _("Jarvis/Judice/Ninke Dithering"),
                StuckiDithering _ => _("Stucki Dithering"),
                _ => _("No Dithering"),
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DitheringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                FloydSteinbergDithering _ => 1,
                JarvisJudiceNinkeDithering _ => 2,
                StuckiDithering _ => 3,
                _ => 0,
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)(double)value) switch
            {
                1 => new FloydSteinbergDithering(),
                2 => new JarvisJudiceNinkeDithering(),
                3 => new StuckiDithering(),
                _ => new Dithering(),
            };
        }
    }
    public class ColorModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                Cie1976Comparison _ => _("CIE-76 (ISO 12647)"),
                CmcComparison _ => _("CMC (l:c)"),
                Cie94Comparison _ => _("CIE-94 (DIN 99)"),
                _ => _("CIE-Delta E 2000"),
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ColorModeToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                Cie1976Comparison _ => 0,
                CmcComparison _ => 1,
                Cie94Comparison _ => 2,
                _ => 3,
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)(double)value) switch
            {
                1 => new CmcComparison(),
                2 => new Cie94Comparison(),
                3 => new CieDe2000Comparison(),
                _ => new Cie1976Comparison(),
            };
        }
    }
    public class EnumBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((Enum)value).Equals((Enum)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.Equals(true) ? parameter : BindingOperations.DoNothing;
        }
    }
    public class IterationInformationToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is IterativeColorRestriction;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path;
            if ((bool)value)
                path = "/Icons/ok.ico";
            else
                path = "/Icons/closewindow.ico";
            return PathToImageConverter.GetIcon(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LockedToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path;
            if ((bool)value)
                path = "/Icons/lock.ico";
            else
                path = "/Icons/unlock.ico";
            return PathToImageConverter.GetIcon(path);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }
    }
    public class FilenameToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string uri)
            {
                try
                {
                    if (uri.StartsWith("/Icons/"))
                    {
                        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                        Uri test = new Uri($"avares://DominoPlanner.Usage/Icons/image.ico");
                        return new Bitmap(assets.Open(test));
                    }
                    else
                    {
                        FileStream fs = new FileStream(uri, FileMode.Open);
                        return Bitmap.DecodeToWidth(fs, 40, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);
                    }
                }
                catch { }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class EnumToButtonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return parameter;
            else
                return Enum.GetValues(targetType).GetValue(0);
        }
    }
    public class BitmapValueConverter : IValueConverter
    {
        public static BitmapValueConverter Instance = new BitmapValueConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string vstr && targetType == typeof(IImage))
            {
                var uri = new Uri(vstr, UriKind.RelativeOrAbsolute);
                var scheme = uri.IsAbsoluteUri ? uri.Scheme : "file";

                switch (scheme)
                {
                    case "file":
                        {
                            try
                            {
                                return new Bitmap(vstr);
                            }
                            catch
                            {
                                return null;
                            }
                        }

                    default:
                        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                        return new Bitmap(assets.Open(uri));
                }
            }
            if (value == null)
                return null;

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IsNormalColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ColorListEntry e && e.DominoColor is DominoColor dc)
            {
                return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";
            if (value is int i && i == int.MaxValue)
                return _("(infinite)");

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PopupColorPicker : ColorPicker
    {
        public PopupColorPicker() { }
    }

    public class ImgSizeConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 3 && values[0] is Bitmap bitmap && values[1] is double width && values[2] is double height && parameter is string parameterValue)
            {
                if (width > 0 && height > 0)
                {
                    double newHeight;
                    double newWidth;

                    double widthToHeight_Image = bitmap.Size.Height / bitmap.Size.Width;
                    double widthToHeight_Space = height / width;

                    if ((widthToHeight_Image > 1 && widthToHeight_Image > widthToHeight_Space) || (width * widthToHeight_Image) > height)
                    {
                        newHeight = height;
                        newWidth = height / widthToHeight_Image;
                    }
                    else
                    {
                        newWidth = width;
                        newHeight = width * widthToHeight_Image;
                    }

                    if (parameterValue.Equals("Height"))
                    {
                        return newHeight;
                    }
                    else if (parameterValue.Equals("Width"))
                    {
                        return newWidth;
                    }
                }
            }
            return true;
        }
    }
    public class FieldPlanArrowsGridConverter : IMultiValueConverter
    {
        public static string RowToolTip = _("Order of the first line in the protocol");
        public static string ColToolTip = _("Order of separate lines in the protocol");
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 4 && values[0] is bool orientation && values[1] is bool mirrorX && values[2] is bool mirrorY && parameter is string parameterString && values[3] is NaturalFieldPlanOrientation naturalOrientation)
            {
                NaturalFieldPlanOrientation currentOrientation = new NaturalFieldPlanOrientation(mirrorX, mirrorY, orientation);
                bool top;
                bool left;
                bool result = false;
                if (naturalOrientation.x ^ naturalOrientation.y) // in this case, transposition works 90° rotated
                {
                    top = orientation ? (currentOrientation.x != naturalOrientation.x) : (currentOrientation.y == naturalOrientation.y);
                    left = orientation ? (currentOrientation.y != naturalOrientation.y) : (currentOrientation.x == naturalOrientation.x);
                }
                else
                {
                    top = orientation ? (currentOrientation.x == naturalOrientation.x) : (currentOrientation.y == naturalOrientation.y);
                    left = orientation ? (currentOrientation.y == naturalOrientation.y) : (currentOrientation.x == naturalOrientation.x);
                }
                if (parameterString.Equals("HorizontalRow"))
                    return top ? 0 : 2;

                if (parameterString.Equals("HorizontalColor"))
                    return (!(currentOrientation.orientation ^ naturalOrientation.orientation)) ? Colors.Blue : Colors.LightBlue;
                if (parameterString.Equals("HorizontalToolTip"))
                    return (!(currentOrientation.orientation ^ naturalOrientation.orientation)) ? RowToolTip : ColToolTip;

                if (parameterString.Equals("RightHorizontalVisibility"))
                    return left;

                if (parameterString.Equals("LeftHorizontalVisibility"))
                    return !left;

                if (parameterString.Equals("VerticalColumn"))
                    return left ? 0 : 2;

                if (parameterString.Equals("VerticalColor"))
                    return (currentOrientation.orientation ^ naturalOrientation.orientation) ? Colors.Blue : Colors.LightBlue;
                if (parameterString.Equals("VerticalToolTip"))
                    return (currentOrientation.orientation ^ naturalOrientation.orientation) ? RowToolTip : ColToolTip;

                if (parameterString.Equals("BottomVerticalVisibility"))
                    return top;
                if (parameterString.Equals("TopVerticalVisibility"))
                    return !top;

                else if (parameterString.Equals("TopLeft"))
                    result = top && left;
                else if (parameterString.Equals("TopRight"))
                    result = top && !left;
                else if (parameterString.Equals("BottomLeft"))
                    result = !top && left;
                else if (parameterString.Equals("BottomRight"))
                    result = !top && !left;
                if (targetType == typeof(Avalonia.Media.IBrush))
                {
                    if (result)
                        return Brushes.Red;
                    else
                        return Brushes.Transparent;
                }
            }
            return null;
        }
    }
    public class BorderWidthConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 4)
            {
                if (values[0] is int colorAmount && values[1] is int amount && values[2] is double width && values[3] is double pixelDensity && pixelDensity > 0)
                {
					double realWidth = Math.Ceiling(width * pixelDensity);
					double realStoneWidth = Math.Floor(realWidth / amount);
					double realBlockSize = realStoneWidth * colorAmount;
					return realBlockSize / pixelDensity;
                }
            }
            return 20;
        }
    }

    public class StoneWidthConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 3)
            {
                if (values[0] is double pixelDensity && pixelDensity > 0 && values[1] is int amount && values[2] is double width)
                {
                    double realWidth = Math.Ceiling(width * pixelDensity);
                    double realStoneWidth = Math.Floor(realWidth / amount);
                    realStoneWidth -= Math.Floor(4 * pixelDensity);
                    return realStoneWidth / pixelDensity;
                }
            }
            return 20;
        }
    }

    public class BlockConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count == 4)
            {
                if (values[0] is int amount && values[1] is double width && values[2] is int blockSize && values[3] is double pixelDensity && pixelDensity > 0)
                {
					double realWidth = Math.Ceiling(width * pixelDensity);
					double realStoneWidth = Math.Floor(realWidth / amount);

                    double realBlockSize = realStoneWidth * blockSize;

                    realBlockSize -= Math.Floor(4 * pixelDensity);

                    return realBlockSize / pixelDensity;
                }
            }
			return 20;
        }
    }

    public class ShowBlockWarning : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values != null && values.Count == 2 && values[0] is int index && values[1] is List<BlockData> blockData && blockData.Count > (index - 1) && (index - 1) >= 0 && !blockData[index - 1].UseBlock)
            {
                return Brushes.Red;
            }
            return Brushes.Transparent;
        }
    }
}
