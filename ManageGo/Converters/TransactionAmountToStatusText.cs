using System;
using System.Globalization;
using Xamarin.Forms;

namespace ManageGo
{
    public class TransactionAmountToStatusText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal _val && _val > 0)
                return "Credit";
            return "Debit";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
