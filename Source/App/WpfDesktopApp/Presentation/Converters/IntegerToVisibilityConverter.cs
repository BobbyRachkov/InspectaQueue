using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Converters;

public class IntegerToVisibilityConverter : IValueConverter
{
    public Visibility FalseValue { get; set; } = Visibility.Collapsed;
    public bool Invert { get; set; } = false;
    public Visibility TreatNullAs { get; set; } = Visibility.Collapsed;

    public int Threshold { get; set; } = 1;

    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return TreatNullAs;
        }

        if (value is not int intValue)
        {
            return Visibility.Visible;
        }
        
        if (Invert)
        {
            return intValue < Threshold ? FalseValue : Visibility.Visible;
        }

        return intValue < Threshold ? FalseValue : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}