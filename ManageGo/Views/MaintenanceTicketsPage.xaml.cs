using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace ManageGo
{
    public partial class MaintenanceTicketsPage : ContentPage
    {
        public MaintenanceTicketsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var st = (StackLayout)sender;
                var im = st.Children[0];
                im.Rotation = im.Rotation + 180;
                var item = ((TappedEventArgs)e).Parameter as MaintenanceTicket;
                item.FirstCommentShown = !item.FirstCommentShown;
                var container = st.Parent;
                var row = container.Parent.Parent as ViewCell;
                row.ForceUpdateSize();
            });
        }



    }
}
