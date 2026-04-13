using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UQuiz.models.enums;

namespace UQuiz.converters
{
    public class UserTypeToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is UserType userType && parameter is string paramStr)
            {
                if (Enum.TryParse<UserType>(paramStr, out var paramType))
                {
                    return userType == paramType;
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value && parameter is string paramStr)
            {
                if (Enum.TryParse<UserType>(paramStr, out var paramType))
                {
                    return paramType;
                }
            }
            return Binding.DoNothing;
        }
    }
}
