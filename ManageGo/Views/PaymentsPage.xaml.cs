using System;
using System.Collections.Generic;
using System.Linq;
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



        void Handle_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            OnPaymentAppeared?.Invoke(this, (Payment)e.Item);
        }
    }
}
