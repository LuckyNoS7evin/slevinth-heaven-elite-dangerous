using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace SlevinthHeavenEliteDangerous.Converters;

/// <summary>
/// Converter to convert a count to Visibility (Visible if > 0, Collapsed if 0)
/// </summary>
public partial class TerraformToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string stringValue)
        {
            return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
