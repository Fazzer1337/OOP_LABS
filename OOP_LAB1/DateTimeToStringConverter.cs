using System;
using System.Globalization;
using System.Windows.Data;

namespace OOP_LAB1
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime d && d != DateTime.MinValue)
                return $"до {d:HH:mm dd.MM}";
            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
