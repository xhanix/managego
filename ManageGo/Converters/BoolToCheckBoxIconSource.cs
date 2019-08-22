using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class BoolToCheckBoxIconSource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool _val && _val)
                return "checked.png";
            return "unchecked.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
