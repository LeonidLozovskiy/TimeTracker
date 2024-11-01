using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace TimeTracker.Helpers
{
    public class DateTimeToDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToString("dd.MM.yyyy");
        }

        public object ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
