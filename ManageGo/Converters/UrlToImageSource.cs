using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class UrlToImageSource : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string url && !string.IsNullOrWhiteSpace(url))
                return ImageSource.FromUri(new Uri(url));
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return false;
        }
    }
}
