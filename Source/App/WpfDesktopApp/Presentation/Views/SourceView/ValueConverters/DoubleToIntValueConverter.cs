using System.Globalization;
using System.Windows.Data;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.Views.SourceView.ValueConverters;

public class DoubleToIntValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return (int)0;
        }

        return System.Convert.ToInt32(value);
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return (double)0;
        }

        return (double)value;
    }
}