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
