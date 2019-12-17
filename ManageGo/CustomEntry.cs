using System;
using Xamarin.Forms;

namespace ManageGo
{
    public class CustomEntry : Entry
    {
        public CustomEntry()
        {
            BackgroundColor = Color.White;
            TextColor = Color.Black;
        }


        public bool HasBorder
        {
            get { return (bool)GetValue(HasBorderProperty); }
            set { SetValue(HasBorderProperty, value); }
        }
        public static readonly BindableProperty HasBorderProperty = BindableProperty.Create(nameof(HasBorder),
                                                                                   typeof(bool),
                                                                                   typeof(CustomEntry),
                                                                                                    false,
                                                                                                    propertyChanged: null);

    }
}
