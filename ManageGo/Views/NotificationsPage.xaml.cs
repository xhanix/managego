using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ManageGo
{
    public partial class NotificationsPage : ContentPage
    {
        public NotificationsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
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
            else
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

    }
}
