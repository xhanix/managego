using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class ToggleToChevronRotationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool expanded && expanded)
                return 90;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
