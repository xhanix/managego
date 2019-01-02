﻿using System;
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
            var container = st.Parent;
            var row = container.Parent.Parent as ViewCell;
            row.ForceUpdateSize();
        }

        protected override bool OnBackButtonPressed()
        {
            var model = (MaintenanceTicketsPageModel)BindingContext;
            if (model.PopContentView != null)
            {
                model.PopContentView = null;
                model.ListIsEnabled = true;
                model.CalendarIsShown = false;
                model.FilterSelectViewIsShown = false;
                return true;
            }
            App.MasterDetailNav.SwitchSelectedRootPageModel<WelcomePageModel>();
            return true;
        }

        void Handle_Clicked(object sender, EventArgs e)
        {
            if (((MaintenanceTicketsPageModel)BindingContext).FetchedTickets != null
                && ((MaintenanceTicketsPageModel)BindingContext).FetchedTickets.Count > 0)
                MyListView.ScrollTo(((MaintenanceTicketsPageModel)BindingContext).FetchedTickets[0], ScrollToPosition.Start, false);
        }




        void Handle_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            ((MaintenanceTicketsPageModel)this.BindingContext).OnItemAppeared((MaintenanceTicket)e.Item);
        }
    }
}
