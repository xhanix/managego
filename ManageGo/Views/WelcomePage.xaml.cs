using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ManageGo
{
    public partial class WelcomePage : ContentPage
    {
        public WelcomePage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
        protected override bool OnBackButtonPressed()
        {
            ((WelcomePageModel)BindingContext).OnMasterMenuTapped.Execute(null);
            return true;
        }
    }
}
