using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace ManageGo
{
    public partial class MGWebViewPage : ContentPage
    {
        public MGWebViewPage()
        {
            InitializeComponent();
        }

        void Handle_Navigating(object sender, Xamarin.Forms.WebNavigatingEventArgs e)
        {
            MyLoader.IsVisible = true;
            MyLoader.IsRunning = true;
        }

        void Handle_Navigated(object sender, Xamarin.Forms.WebNavigatedEventArgs e)
        {
            MyLoader.IsVisible = false;
            MyLoader.IsRunning = false;
        }
    }
}
