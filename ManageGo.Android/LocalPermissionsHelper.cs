using System;
using Android;
using Android.Support.Design.Widget;
using ManageGo.Droid;
using Plugin.CurrentActivity;
using Xamarin.Forms;

[assembly: Dependency(typeof(LocalPermissionsHelper))]
namespace ManageGo.Droid
{
    public class LocalPermissionsHelper : ILocalPermissionsHelper
    {
        Action onSuccess;
        Action onFailure;

        public LocalPermissionsHelper()
        {
            MainActivity.Current.FingerPringPermissionsResultReady += (object sender, bool e) =>
            {
                if (e)
                    onSuccess?.Invoke();
                else
                    onFailure?.Invoke();
            };
        }

        public void GetBiometricPermission(Action OnSuccess, Action OnFailure)
        {
            onSuccess = OnSuccess;
            onFailure = OnFailure;
            if (CrossCurrentActivity.Current.Activity.ShouldShowRequestPermissionRationale(Manifest.Permission.UseBiometric)
             || CrossCurrentActivity.Current.Activity.ShouldShowRequestPermissionRationale(Manifest.Permission.UseFingerprint))
            {
                Snackbar.Make(CrossCurrentActivity.Current.Activity.FindViewById(Android.Resource.Id.Content), "Fingerprint access is required to log in with fingerprint.", Snackbar.LengthIndefinite)
                  .SetAction("OK", v =>
                    CrossCurrentActivity.Current.Activity.RequestPermissions
                        (new string[] { Manifest.Permission.UseBiometric, Manifest.Permission.UseFingerprint }, 7890))
                        .Show();
            }
            else
            {
                CrossCurrentActivity.Current.Activity.RequestPermissions
                       (new string[] { Manifest.Permission.UseBiometric, Manifest.Permission.UseFingerprint }, 7890);
            }
        }
    }
}
