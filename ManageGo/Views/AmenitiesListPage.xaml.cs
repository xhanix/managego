using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class AmenitiesListPage : ContentPage
    {
        public event EventHandler<MGDataAccessLibrary.Models.Amenities.Responses.Booking> OnBookingAppeared;
        public Thickness DotMargin => new Thickness(0, -1.35 * Device.GetNamedSize(NamedSize.Default, typeof(Label)), 0, 0);

        public AmenitiesListPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

        }

        public void SetCalContent()
        {
            var cal = new Controls.CalendarView();
            cal.SetBinding(Controls.CalendarView.SelectedDatesProperty, new Binding("SelectedDateRange"));

            cal.AllowMultipleSelection = true;
            CalContainer.Content = cal;
        }

        public void setFilterCalContent()
        {
            var cal = new Controls.CalendarView();
            cal.SetBinding(Controls.CalendarView.SelectedDatesProperty, new Binding("SelectedDateRange"));

            cal.AllowMultipleSelection = true;
            FilterCalContainer.Content = cal;
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


        private void HandleItemTapped(object sender, ItemTappedEventArgs tappedEventArgs)
        {
            if (this.BindingContext != null)
            {
                ((AmenitiesListPageModel)BindingContext).OnBookingTapped.Execute(tappedEventArgs.Item);
            }
        }

        void Handle_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            OnBookingAppeared?.Invoke(this, (MGDataAccessLibrary.Models.Amenities.Responses.Booking)e.Item);
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
