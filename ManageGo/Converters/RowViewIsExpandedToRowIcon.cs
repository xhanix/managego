using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class RowViewIsExpandedToRowIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool _val && _val)
                return "chevron_down.png";
            return "chevron_right.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
