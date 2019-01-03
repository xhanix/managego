using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ManageGo
{
    public partial class CalendarPage : ContentPage
    {
        public CalendarPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
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
