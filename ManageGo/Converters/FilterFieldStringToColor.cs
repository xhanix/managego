using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo
{
    public class FilterFieldStringToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null || value is string s && (s.ToLower().Contains("all") || s.ToLower().Contains("select") ||
                string.IsNullOrWhiteSpace(s) || (s.Contains("$") && s.Split('-').FirstOrDefault() == "$0 "
                && s.Split('-').LastOrDefault() == " $5,000+")))
                return "#58595B";
            return "#8ad96b";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
