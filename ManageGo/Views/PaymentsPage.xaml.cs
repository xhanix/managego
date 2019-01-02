using System;
using System.Collections.Generic;
using System.Linq;
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
            var st = (StackLayout)sender;
            var container = st.Parent;
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
