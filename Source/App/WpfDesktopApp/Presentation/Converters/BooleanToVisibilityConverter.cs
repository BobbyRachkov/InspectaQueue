using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Converters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public Visibility FalseValue { get; set; } = Visibility.Collapsed;
    public bool Invert { get; set; } = false;
    public Visibility TreatNullAs { get; set; } = Visibility.Collapsed;

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return TreatNullAs;
        }

        if (value is not bool isVisible)
        {
            return Visibility.Visible;
        }

        if (Invert)
        {
            return isVisible ? FalseValue : Visibility.Visible;
        }

        return isVisible ? Visibility.Visible : FalseValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}