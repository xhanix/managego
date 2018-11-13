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

        void Handle_Tapped(object sender, EventArgs e)
        {
            var st = (StackLayout)sender;
            var im = st.Children[0];
            im.Rotation = im.Rotation + 180;
            var item = ((TappedEventArgs)e).Parameter as MaintenanceTicket;
            item.FirstCommentShown = !item.FirstCommentShown;
            var container = st.Parent;
            var row = container.Parent.Parent as ViewCell;
            row.ForceUpdateSize();
        }



        void Handle_Clicked(object sender, System.EventArgs e)
        {
            if (((MaintenanceTicketsPageModel)BindingContext).FetchedTickets != null
                && ((MaintenanceTicketsPageModel)BindingContext).FetchedTickets.Count > 0)
                MyListView.ScrollTo(((MaintenanceTicketsPageModel)BindingContext).FetchedTickets[0], ScrollToPosition.Start, false);
        }
    }
}
