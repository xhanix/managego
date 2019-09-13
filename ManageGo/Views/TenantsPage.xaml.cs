using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class TenantsPage : ContentPage
    {
        public TenantsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        public async void Hanlde_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (this.BindingContext != null)
            {
                await ((TenantsPageModel)BindingContext).OnItemAppeared((Tenant)e.Item);
            }
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            TenantsListView.HasUnevenRows = !TenantsListView.HasUnevenRows;
            TenantsListView.HasUnevenRows = !TenantsListView.HasUnevenRows;
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
                TenantsListView.ItemsSource = ((TenantsPageModel)BindingContext).FetchedTenants;
            }
            TenantsListView.HasUnevenRows = !TenantsListView.HasUnevenRows;
            TenantsListView.HasUnevenRows = !TenantsListView.HasUnevenRows;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            TenantsListView.ItemsSource = null;
        }
    }
}
