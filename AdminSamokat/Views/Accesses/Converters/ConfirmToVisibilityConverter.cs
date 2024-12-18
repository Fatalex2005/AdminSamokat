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
                // Если параметр "Cancel", показываем кнопку отмены при Confirm = 1
                if (parameter?.ToString() == "Cancel")
                {
                    return confirm == 1;
                }
                // Показываем кнопку подтверждения при Confirm = 0
                return confirm == 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
