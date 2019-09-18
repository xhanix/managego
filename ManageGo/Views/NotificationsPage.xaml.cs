using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class NotificationsPage : ContentPage
    {
        public NotificationsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            MyListView.ItemsSource = null;
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
            else
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

        public void DataLoaded()
        {
            if (this.BindingContext != null)
            {
                MyListView.ItemsSource = ((NotificationsPageModel)BindingContext).FetchedNotifications;
            }
            MyListView.HasUnevenRows = !MyListView.HasUnevenRows;
            MyListView.HasUnevenRows = !MyListView.HasUnevenRows;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MyListView.ItemsSource = null;
        }

        protected override bool OnBackButtonPressed()
        {
            if (Navigation.ModalStack.Contains(this))
            {
                Navigation.PopModalAsync();
            }
            else
            {
                App.MasterDetailNav.SwitchSelectedRootPageModel<WelcomePageModel>();
            }
            return true;
        }

    }
}
