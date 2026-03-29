using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace SlevinthHeavenEliteDangerous.Converters;

/// <summary>
/// Converts a string to Visibility — Visible if the string is non-null and non-empty, Collapsed otherwise.
/// </summary>
public partial class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
