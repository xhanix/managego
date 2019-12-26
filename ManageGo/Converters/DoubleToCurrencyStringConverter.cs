using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class DoubleToCurrencyStringConverter : IValueConverter
    {
        const string formatString = "C0";
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double num)
            {
                return num.ToString(formatString);
            }
            return 0.ToString(formatString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && !string.IsNullOrWhiteSpace(s) && double.TryParse(s.Replace("$", ""), out double num))
                return num;
            return 0;
        }
    }
}
