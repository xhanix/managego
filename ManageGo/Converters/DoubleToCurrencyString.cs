using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class DoubleToCurrencyString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double val)
            {
                return val.ToString("C", CultureInfo.CurrentCulture);
            }
            return "$0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string val)
            {
                if (double.TryParse(val.Replace("$", ""), out double d))
                {
                    return d;
                }
            }
            return 0d;
        }
    }
}
