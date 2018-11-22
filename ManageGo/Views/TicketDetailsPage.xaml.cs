using System;
using System.Collections.Generic;

using Xamarin.Forms;
using PropertyChanged;

namespace ManageGo
{
    [AddINotifyPropertyChangedInterface]
    public partial class TicketDetailsPage : ContentPage
    {
        double pageHeight;
        public double pageWidth { get; set; }
        readonly double ReplyBoxHeigh = 150;
        public TicketDetailsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ReplyBox.HeightRequest = ReplyBoxHeigh;

        }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            pageHeight = height;
            pageWidth = width * 0.7;

        }



        async void Handle_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            ReplyBox.HeightRequest = pageHeight * 0.5;
            MyScrollView.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
            await MyScrollView.ScrollToAsync(0, 0, false);
            //MyNavBar.IsVisible = false;
            //TopDetailsView.IsVisible = false;
        }

        void Handle_Unfocused(object sender, FocusEventArgs e)
        {
            ReplyBox.HeightRequest = ReplyBoxHeigh;
            //MyNavBar.IsVisible = true;
            //TopDetailsView.IsVisible = true;
        }
    }
}
