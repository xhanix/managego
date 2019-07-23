using System;
using Foundation;
using ManageGo.iOS;
using UIKit;
using Xamarin.Forms;


[assembly: Dependency(typeof(AppStoreOpenerImplementation))]
namespace ManageGo.iOS
{
    public class AppStoreOpenerImplementation : IAppStoreOpener
    {
        public void OpenAppStore()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UIApplication.SharedApplication.OpenUrl(new NSUrl("https://itunes.apple.com/us/app/ManageGo-LandlordApp/id1459683580?mt=8"));
            });
        }
    }
}

