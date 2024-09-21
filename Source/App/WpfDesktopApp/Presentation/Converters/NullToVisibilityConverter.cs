using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Converters;

public class NullToVisibilityConverter : IValueConverter
{
    public Visibility FalseValue { get; set; } = Visibility.Collapsed;
    public bool Invert { get; set; } = false;

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return Invert ? Visibility.Visible : FalseValue;
        }

        return Invert ? FalseValue : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}