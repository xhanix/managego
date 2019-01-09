using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class EnabledStateToTextColorConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool v && v)
            {
                // is enabled
                return "#898B8D";
            }
            else
                return "#f4f4f4";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
