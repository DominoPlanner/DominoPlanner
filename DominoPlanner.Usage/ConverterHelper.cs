using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DominoPlanner.Usage
{
    class ConverterHelper
    {
    }

    public class ColorToHTMLConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Windows.Media.Color)
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
            if (value is System.Windows.Media.Color)
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

    public class BoolVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            
            return System.Windows.Visibility.Collapsed;
            if(value is bool visibility)
            {
                if (visibility)
                {
                    return System.Windows.Visibility.Visible;
                }
                else
                {
                    return System.Windows.Visibility.Hidden;
                }
            }
            return System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
