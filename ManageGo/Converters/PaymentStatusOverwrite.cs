using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class PaymentStatusOverwrite : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                if (status.ToLower() == "passed")
                    return "Received";
                return (status);
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
