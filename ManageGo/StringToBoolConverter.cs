using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class StringToBoolConverter : IValueConverter
    {
        public StringToBoolConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string && !string.IsNullOrWhiteSpace((string)value))
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
