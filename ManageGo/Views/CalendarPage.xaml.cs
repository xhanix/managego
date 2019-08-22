using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace ManageGo
{
    public partial class CalendarPage : ContentPage
    {
        public CalendarPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            var cal = new Controls.CalendarView();

            cal.SetBinding(Controls.CalendarView.SelectedDateProperty, "SelectedDate");
            cal.SetBinding(Controls.CalendarView.HighlightedDatesProperty, "HighlightedDates");
            cal.VerticalOptions = LayoutOptions.Start;
            cal.HeightRequest = 310;
            cal.WidthRequest = 490;
            CalContainer.Content = cal;
        }

        public void DataLoaded()
        {
            if (this.BindingContext != null)
            {

                CalEventList.ItemsSource = ((CalendarPageModel)BindingContext).CalendarEvents;
            }
            CalEventList.HasUnevenRows = !CalEventList.HasUnevenRows;
            CalEventList.HasUnevenRows = !CalEventList.HasUnevenRows;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CalEventList.ItemsSource = null;
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            var st = (Frame)sender;
            var container = st.Parent;
            if (container is ViewCell)
            {
                var row = container as ViewCell;
                row.ForceUpdateSize();
            }
            else if (container.Parent != null)
            {
                var p = container.Parent;
                while (p as ViewCell is null)
                {
                    p = p.Parent;
                }
                var row = p as ViewCell;
                row.ForceUpdateSize();
            }

        }


    }
}
