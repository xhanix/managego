using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class BookingCalendarPage : ContentPage
    {
        readonly Color defaultColor = Color.FromHex("#f8f9fa");
        TapGestureRecognizer recognizer = new TapGestureRecognizer();
        public BookingCalendarPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            recognizer.Tapped += Recognizer_Tapped;
        }

        private async void Recognizer_Tapped(object sender, EventArgs e)
        {
            //on time slot tapped
            var container = (StackLayout)sender;
            var name = container.ClassId.Replace("#", "");
            if (int.TryParse(name, out int timeFrom))
            {
                var min = timeFrom;
                if (BindingContext != null)
                {
                    await ((BookingCalendarPageModel)BindingContext).GetBookingForStartTime(min);
                }
            }
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

            for (int i = 30; i <= 1410; i += 30)
            {
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
            var fontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));
            if (timeRanges is null)
                return;
            foreach (var range in timeRanges)
            {
                var minTime = range.From;
                var maxTime = range.To;
                string prevString = string.Empty;
                for (int i = minTime; i <= maxTime; i += 30)
                {
                    var calSlot = this.FindByName<StackLayout>($"A{i}");

                    if (calSlot != null)
                    {
                        calSlot.ClassId = $"#{range.From}";
                        if (calSlot.GestureRecognizers is null || !calSlot.GestureRecognizers.Any())
                        {
                            calSlot.GestureRecognizers.Add(recognizer);
                        }
                        calSlot.BackgroundColor = defaultColor;
                        if (calSlot.Children.Any(t => t is Label))
                            calSlot.Children.Remove(calSlot.Children.First(t => t is Label));

                        if (range.BookedBy is null)
                        {
                            calSlot.BackgroundColor = Color.White;

                        }

                        else
                        {
                            calSlot.BackgroundColor = range.BookedBy.Status == MGDataAccessLibrary.Models.Amenities.Responses.BookingStatus.Approved ? Color.FromHex("#eef9e6") : Color.FromHex("#dedede");
                            var fromTimeSpan = new TimeSpan(0, range.From, 0);
                            var fromDate = new DateTime(fromTimeSpan.Ticks).ToString("hh:mm tt");
                            var toTimeSpan = new TimeSpan(0, range.To, 0);
                            var toDate = new DateTime(toTimeSpan.Ticks).ToString("hh:mm tt");
                            var displayStatus = range.BookedBy.Status == MGDataAccessLibrary.Models.Amenities.Responses.BookingStatus.Approved ? "Booked" : range.BookedBy.Status.ToString();
                            var currentString = $"{displayStatus} - {fromDate} to {toDate}";
                            if (currentString != prevString)
                            {
                                calSlot.Children.Add(new Label { Text = currentString, FontSize = fontSize, TextColor = Color.FromHex("#5e5e5e"), InputTransparent = true });
                                prevString = currentString;
                            }

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
