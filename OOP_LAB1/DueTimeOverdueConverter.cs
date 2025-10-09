using System;
using System.Globalization;
using System.Windows.Data;

namespace OOP_LAB1
{
    public class DueTimeOverdueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // value должен быть TaskItem!
            var task = value as TaskItem;

            if (task == null || task.DueTime == DateTime.MinValue)
                return false;

            // Если задача завершена — она не просрочена!
            return !task.IsCompleted && task.DueTime < DateTime.Now;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
