using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class BookingCalendarPage : ContentPage
    {
        public BookingCalendarPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override bool OnBackButtonPressed()
        {
            if (Navigation.ModalStack.Contains(this))
            {
                Navigation.PopModalAsync();
            }
            else if (Navigation.NavigationStack.Contains(this))
            {
                Navigation.PopAsync();
            }
            else
            {
                App.MasterDetailNav.SwitchSelectedRootPageModel<WelcomePageModel>();
            }
            return true;
        }

        private void resetCellColors()
        {
            var defaultColor = Color.FromHex("#f8f9fa");
            for (int i = 30; i <= 1410; i += 30)
            {
                Console.WriteLine($"A{i}");
                var calSlot = this.FindByName<StackLayout>($"A{i}");
                if (calSlot != null)
                {
                    calSlot.BackgroundColor = defaultColor;
                }
            }
        }

        public void SetTimeDetails(IEnumerable<MGDataAccessLibrary.Models.Amenities.Responses.TimeRanges> timeRanges)
        {
            resetCellColors();
            if (timeRanges is null)
                return;
            foreach (var range in timeRanges)
            {
                var minTime = range.From;
                var maxTime = range.To;
                for (int i = minTime; i <= maxTime; i += 30)
                {
                    var calSlot = this.FindByName<StackLayout>($"A{i}");
                    if (calSlot != null)
                    {
                        if (range.BookedBy is null)
                            calSlot.BackgroundColor = Color.White;
                        else
                        {
                            calSlot.BackgroundColor = Color.FromHex("#eef9e6");
                            calSlot.Children.Add(new Label { Text = $"{range.BookedBy.Status} - {range.From} to {range.To}" });
                        }
                    }
                }
            }
        }


        public void DataLoaded()
        {
            if (this.BindingContext != null)
            {

                //  MyListView.ItemsSource = ((AmenitiesListPageModel)BindingContext).Bookings;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // MyListView.ItemsSource = null;
        }
    }
}
