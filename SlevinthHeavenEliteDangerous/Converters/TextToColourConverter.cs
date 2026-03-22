using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace SlevinthHeavenEliteDangerous.Converters;

public partial class TextToColourConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string signalText)
        {
            if (signalText.Contains("Biological", StringComparison.OrdinalIgnoreCase) && signalText.Contains("Geological", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Colors.Pink);
            }
            else if (signalText.Contains("Biological", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Colors.LightGreen);
            }
            else if (signalText.Contains("Geological", StringComparison.OrdinalIgnoreCase))
            {
                return new SolidColorBrush(Colors.Orange);
            }
        }
        return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
