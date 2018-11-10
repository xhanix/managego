using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ManageGo
{
    public partial class MasterMenuPage : ContentPage
    {
        public MasterMenuPage()
        {
            InitializeComponent();
            Title = "Menu";
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            (this.BindingContext as MasterMenuPageModel)?.OnSupportEmailTapped.Execute(null);
        }

        void Handle_Tapped_1(object sender, System.EventArgs e)
        {
            (this.BindingContext as MasterMenuPageModel)?.OnSupportPhoneTapped.Execute(null);
        }
    }
}
