using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class PaddedStringNumberFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int i)
            {
                return i.ToString().PadRight(5);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string v && int.TryParse(v, out int result))
            {
                return result;
            }
            return 0;
        }
    }
}
