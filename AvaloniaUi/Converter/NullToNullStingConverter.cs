using System.Globalization;
using Avalonia.Data.Converters;

namespace aSql.Converter;

public class NullToNullStringConverter : IValueConverter
{
  public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    return value is null or DBNull ? "<NULL>" : value.ToString();
  }

  public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is "<NULL>") return DBNull.Value;
    return value;
  }
}