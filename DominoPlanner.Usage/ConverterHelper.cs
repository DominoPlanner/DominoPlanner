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

    public class InterToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Inter)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public sealed class DiffusionModeToIntConverter : IValueConverter
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
                    0 => Inter.Nearest,
                    1 => Inter.Linear,
                    3 => Inter.Area,
                    4 => Inter.Cubic,
                    _ => Inter.Nearest,
                };
            }

            if (value.GetType().IsEnum)
            {
                var val = (Inter)value;
                return val switch
                {
                    Inter.Nearest => 0,
                    Inter.Linear => 1,
                    Inter.Area => 3,
                    Inter.Cubic => 4,
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
                FloydSteinbergDithering _ => "Floyd/Steinberg Dithering",
                JarvisJudiceNinkeDithering _ => "Jarvis/Judice/Ninke Dithering",
                StuckiDithering _ => "Stucki Dithering",
                _ => "No Dithering",
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
                Cie1976Comparison _ => "CIE-76 (ISO 12647)",
                CmcComparison _ => "CMC (l:c)",
                Cie94Comparison _ => "CIE-94 (DIN 99)",
                _ => "CIE-Delta E 2000",
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
            if ((bool)value)
            {
                return new IterativeColorRestriction(2, 0.1);
            }
            else
            {
                return new NoColorRestriction();
            }
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
                        return new Bitmap(vstr);

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
    public class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

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
}
