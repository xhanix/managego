using System;
using System.Collections.Generic;

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
            var im = (Image)sender;
            im.Rotation = im.Rotation + 180;
            var item = ((TappedEventArgs)e).Parameter as MaintenanceTicket;
            item.FirstCommentShown = !item.FirstCommentShown;

        }



    }
}
