using Avalonia.Data.Converters;
using System;
using System.Collections;
using System.Globalization;

namespace DominoPlanner.Usage
{
    public class FirstSnapshotToTipConverter : IValueConverter
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
                        var first = enumerator.Current;
                        if (first != null)
                        {
                            var t = first.GetType();
                            var rowProp = t.GetProperty("Row");
                            var colProp = t.GetProperty("Column");
                            if (rowProp != null && colProp != null)
                            {
                                var rowVal = rowProp.GetValue(first);
                                var colVal = colProp.GetValue(first);
                                return $"Go to Row {rowVal}, Block {colVal}";
                            }
                        }
                    }
                }
                finally
                {
                    // nothing
                }
            }

            return string.Empty;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
        {
            throw new NotImplementedException();
        }
    }
}
