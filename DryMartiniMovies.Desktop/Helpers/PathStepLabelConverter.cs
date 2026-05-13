using System.Globalization;
using System.Windows.Data;
using DryMartiniMovies.Core.DTOs;
using DryMartiniMovies.Core.Enums;

namespace DryMartiniMovies.Desktop.Helpers;

public class PathStepLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not PathStepDto step) return string.Empty;
        if (step.Type == NodeType.Movie) return "Film";
        return step.Role == PersonRole.Director ? "Regissör" : "Skådespelare";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
