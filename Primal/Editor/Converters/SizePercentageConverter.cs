using System;
using System.Globalization;
using System.Windows.Data;

namespace Editor.Converters
{
    public class SizePercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualSize && parameter is string percentageString && double.TryParse(percentageString, out double percentage))
            {
                return actualSize * (percentage / 100.0);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
