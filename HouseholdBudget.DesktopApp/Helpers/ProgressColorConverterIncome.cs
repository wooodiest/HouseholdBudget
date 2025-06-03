using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HouseholdBudget.DesktopApp.Helpers
{
    public class ProgressColorConverterExpense : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return progress switch
                {
                    < 0.5 => new SolidColorBrush(Colors.Green),
                    < 1.0 => new SolidColorBrush(Colors.Orange),
                    _ => new SolidColorBrush(Colors.Red)
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ProgressColorConverterIncome : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                return progress switch
                {
                    < 0.5 => new SolidColorBrush(Colors.Red),
                    < 1.0 => new SolidColorBrush(Colors.Orange),
                    _ => new SolidColorBrush(Colors.Green)
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
