using System;
using System.Globalization;

namespace AdminSamokat.Views.Accesses.Converters
{
    public class ConfirmToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Проверка на null и приведение значения
            if (value is int confirm)
            {
                return confirm == 1 ? "Подтверждено" : "Не подтверждено";
            }

            return "Нет";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
