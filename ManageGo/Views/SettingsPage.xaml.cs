using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        void Handle_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = (CustomEntry)sender;
            ((SettingsPageModel)this.BindingContext).OnFieldLostFocus.Execute(entry.ClassId);
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



        void Handle_Tapped(object sender, EventArgs e)
        {
            var name = ((Image)sender).ClassId;
            switch (name)
            {
                case "UserName":
                    if (!UserNameEntry.IsFocused)
                        UserNameEntry.Focus();
                    break;
                case "Email":
                    if (!EmailEntry.IsFocused)
                        EmailEntry.Focus();
                    break;
                case "DisplayName":
                    //  if (!DisplayNameEntry.IsFocused)
                    // DisplayNameEntry.Focus();
                    break;

                default:
                    break;
            }

        }


    }
}
