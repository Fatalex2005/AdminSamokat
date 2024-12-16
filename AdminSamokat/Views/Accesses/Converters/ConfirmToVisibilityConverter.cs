using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AdminSamokat.Views.Accesses.Converters
{
    public class ConfirmToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int confirm)
            {
                // Кнопка видима, если Confirm == 0
                return confirm == 0;
            }

            return false; // Скрываем кнопку по умолчанию
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
