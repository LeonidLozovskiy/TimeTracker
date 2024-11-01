using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace TimeTracker.Helpers
{
    public class DiscountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Format("{0}%", (int)value);
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
