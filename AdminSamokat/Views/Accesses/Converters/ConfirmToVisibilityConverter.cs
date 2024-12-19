using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace AdminSamokat.Views.Accesses.Converters
{
    public class ConfirmToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Tuple<int, DateTime> confirmAndDate)
            {
                int confirm = confirmAndDate.Item1;
                DateTime date = confirmAndDate.Item2;

                // Если дата раньше сегодняшнего дня, ничего не показываем
                if (date.Date < DateTime.Now.Date)
                {
                    return false;
                }

                // Обрабатываем параметры
                if (parameter is string action)
                {
                    switch (action)
                    {
                        case "Cancel":
                            // Показываем кнопку отмены, если Confirm == 1
                            return confirm == 1;

                        case "PartialConfirm":
                            // Показываем кнопку частичного подтверждения, если Confirm == 0
                            return confirm == 0;

                        case "PartialCancel":
                            // Показываем кнопку частичной отмены, если Confirm == 1
                            return confirm == 1;

                        default:
                            // Показываем кнопку подтверждения, если Confirm == 0
                            return confirm == 0;
                    }
                }
                else
                {
                    // Если параметр отсутствует, показываем кнопку подтверждения, если Confirm == 0
                    return confirm == 0;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}