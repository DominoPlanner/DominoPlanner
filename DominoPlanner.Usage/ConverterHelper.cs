using Avalonia.Data.Converters;
using DominoPlanner.Core;
using Emgu.CV.CvEnum;
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

namespace DominoPlanner.Usage
{
    class ConverterHelper
    {
    }
    public class AmountToColorConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            int anzahl = 0, gesamt = 0;
            if (int.TryParse(values[0].ToString(), out anzahl) && int.TryParse(values[1].ToString(), out gesamt))
            {
                if (anzahl > gesamt)
                {
                    return Brushes.Red;
                }
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
            if (value is Color)
            {
                Color c = (Color)value;
                return c.ToString();
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
            if (value is Color)
            {
                return new SolidColorBrush((Color)value);
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
                switch (val)
                {
                    case 0: return Inter.Nearest;
                    case 1: return Inter.Linear;
                    case 2: return Inter.LinearExact;
                    case 3: return Inter.Area;
                    case 4: return Inter.Cubic;
                    case 5: return Inter.Lanczos4;
                    default: return Inter.Nearest;
                }
            }

            if (value.GetType().IsEnum)
            {
                var val = (Inter)value;
                switch (val)
                {
                    case Inter.Nearest: return 0;
                    case Inter.Linear: return 1;
                    case Inter.LinearExact: return 2;
                    case Inter.Area: return 3;
                    case Inter.Cubic: return 4;
                    case Inter.Lanczos4: return 5;
                    default: return 0;
                }
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
            switch (value)
            {
                case FloydSteinbergDithering d:
                    return "Floyd/Steinberg Dithering";
                case JarvisJudiceNinkeDithering d:
                    return "Jarvis/Judice/Ninke Dithering";
                case StuckiDithering d:
                    return "Stucki Dithering";
                default:
                    return "No Dithering";
            }
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
            switch (value)
            {
                case FloydSteinbergDithering d:
                    return 1;
                case JarvisJudiceNinkeDithering d:
                    return 2;
                case StuckiDithering d:
                    return 3;
                default:
                    return 0;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)(double)value)
            {
                case 1:
                    return new FloydSteinbergDithering();
                case 2:
                    return new JarvisJudiceNinkeDithering();
                case 3:
                    return new StuckiDithering();
                default:
                    return new Dithering();
            }
        }
    }
    public class ColorModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case Cie1976Comparison d:
                    return "CIE-76 (ISO 12647)";
                case CmcComparison d:
                    return "CMC (l:c)";
                case Cie94Comparison d:
                    return "CIE-94 (DIN 99)";
                default:
                    return "CIE-Delta E 2000";
            }
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
            switch (value)
            {
                case Cie1976Comparison d:
                    return 0;
                case CmcComparison d:
                    return 1;
                case Cie94Comparison d:
                    return 2;
                default:
                    return 3;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((int)(double)value)
            {
                case 1:
                    return new CmcComparison();
                case 2:
                    return new Cie94Comparison();
                case 3:
                    return new CieDe2000Comparison();
                default:
                    return new Cie1976Comparison();
            }
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
            string path = "";
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
            string path = "";
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
            string uri = value as string;

            if (uri != null)
            {
                try
                {
                    Bitmap image = new Bitmap(uri);
                    FileStream fs = new FileStream(uri, FileMode.Open);
                    return Bitmap.DecodeToWidth(fs, 40, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);
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
            if (value is string && targetType == typeof(IImage))
            {
                var uri = new Uri((string)value, UriKind.RelativeOrAbsolute);
                var scheme = uri.IsAbsoluteUri ? uri.Scheme : "file";

                switch (scheme)
                {
                    case "file":
                        return new Bitmap((string)value);

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
}
