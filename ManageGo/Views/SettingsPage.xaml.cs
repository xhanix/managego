using System;
using System.Collections.Generic;

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

        void Handle_Tapped(object sender, TappedEventArgs e)
        {
            var name = (string)e.Parameter;
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
                    if (!DisplayNameEntry.IsFocused)
                        DisplayNameEntry.Focus();
                    break;

                default:
                    break;
            }

        }
    }
}
