using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class TimeSpanToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan _val)
            {
                var dateTime = new DateTime(_val.Ticks);
                return dateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
            }

            return "unchecked.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
