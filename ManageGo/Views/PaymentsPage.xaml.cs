using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManageGo.Models;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class PaymentsPage : ContentPage
    {
        public PaymentsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

        }

        public event EventHandler<Payment> OnPaymentAppeared;

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

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            PaymentsList.HasUnevenRows = !PaymentsList.HasUnevenRows;
            PaymentsList.HasUnevenRows = !PaymentsList.HasUnevenRows;
        }

        void Handle_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            OnPaymentAppeared?.Invoke(this, (Payment)e.Item);
        }



        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            PaymentsList.ItemsSource = null;
        }

        public void DataLoaded()
        {
            if (this.BindingContext != null)
            {

                PaymentsList.ItemsSource = ((PaymentsPageModel)BindingContext).FetchedPayments;
            }

            PaymentsList.HasUnevenRows = !PaymentsList.HasUnevenRows;
            PaymentsList.HasUnevenRows = !PaymentsList.HasUnevenRows;
        }


        internal void ScrollToFirst(Object item)
        {
            PaymentsList.ScrollTo(item, ScrollToPosition.MakeVisible, false);
        }
    }
}
