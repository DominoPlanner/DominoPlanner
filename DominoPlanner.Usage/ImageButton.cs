using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace DominoPlanner.Usage
{
    public class ImageButton : Button
    {
        public IImage Image
        {
            get { return GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly StyledProperty<IImage> ImageProperty = AvaloniaProperty.Register<ImageButton, IImage>(nameof(Image));

        public ImageButton() { }
    }
    public class IsNotEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            return !(value is string s && string.IsNullOrEmpty(s));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
