using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace DominoPlanner.Usage
{
    public class FirstOrDefaultConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            if (value is IEnumerable enumerable)
            {
                var enumerator = enumerable.GetEnumerator();
                try
                {
                    if (enumerator.MoveNext())
                    {
                        return enumerator.Current;
                    }
                }
                finally
                {
                    // nothing
                }
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
