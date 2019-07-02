using System;
using Xamarin.Forms;

namespace ManageGo
{
    public class MGLabel : Label
    {
        public static readonly BindableProperty LineSpacingProperty =
          BindableProperty.Create(nameof(LineSpacing),
              typeof(double), typeof(double), defaultValue: 1d);

        public double LineSpacing
        {
            get { return (double)GetValue(LineSpacingProperty); }
            set { SetValue(LineSpacingProperty, value); }
        }
    }
}
