using System;
using Android.OS;
using ManageGo.Droid;
using Plugin.CurrentActivity;
using Xamarin.Forms;


[assembly: Dependency(typeof(AppStoreOpenerImplementation))]
namespace ManageGo.Droid
{
    public class AppStoreOpenerImplementation : IAppStoreOpener
    {
        public void OpenAppStore()
        {

            try
            {
                CrossCurrentActivity.Current.Activity.StartActivity(new Android.Content.Intent(Android.Content.Intent.ActionView, Android.Net.Uri.Parse("market://details?id=" + Xamarin.Essentials.AppInfo.PackageName)));

            }
            catch (Android.Content.ActivityNotFoundException)
            {
                CrossCurrentActivity.Current.Activity.StartActivity(new Android.Content.Intent(Android.Content.Intent.ActionView, Android.Net.Uri.Parse("https://play.google.com/store/apps/details?id=" + Xamarin.Essentials.AppInfo.PackageName)));
            }
        }
    }
}
