using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using System.Threading.Tasks;
using Android.Content;
using System.Linq;
using System.IO;
using ManageGo.Services;
using Android.Webkit;
using Android.Provider;
using Firebase.Messaging;
using Firebase.Iid;
using Android.Util;
using Android.Gms.Common;

namespace ManageGo.Droid
{
    [Activity(Label = "ManageGo", Icon = "@mipmap/ic_launcher", LaunchMode = LaunchMode.SingleTask, RoundIcon = "@mipmap/ic_launcher_round", Theme = "@style/MainTheme", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        // Field, properties, and method for Video Picker
        public static MainActivity Current { private set; get; }
        public event EventHandler<bool> FingerPringPermissionsResultReady;
        public static readonly int PickImageId = 1000;
        // static readonly string TAG = "MainActivity";
        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 199;
        public TaskCompletionSource<string> PickImageTaskCompletionSource { set; get; }
        public TaskCompletionSource<Tuple<Stream, string, MGFileType>> PickMediaTaskCompletionSource { set; get; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            Current = this;
            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            var result = IsPlayServicesAvailable();
            CreateNotificationChannel();
        }

    

        protected override async void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            if (intent.Extras != null)
            {
                bool isGroup = intent.Extras.GetInt("IsGroup", 0) != 0;
                await App.NotificationReceived(intent.Extras.GetInt("Type"), intent.Extras.GetInt("NotificationObject"), isGroup);
            }
        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }
            var channel = new NotificationChannel(CHANNEL_ID, "FCM Notifications", NotificationImportance.Default)
            {
                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == PickImageId)
            {
                if ((resultCode == Result.Ok) && (data != null))
                {
                    // Set the filename as the completion of the Task
                    PickImageTaskCompletionSource?.SetResult(data.DataString);
                    Android.Net.Uri uri = data.Data;
                    var mime = this.ContentResolver.GetType(uri);
                    Android.Database.ICursor cr = ContentResolver.Query(uri, null, null, null, null);
                    cr.MoveToFirst();
                    var displayName = cr.GetString(cr.GetColumnIndex(OpenableColumns.DisplayName));

                    Stream stream = ContentResolver.OpenInputStream(uri);
                    // Set the Stream as the completion of the Task
                    PickMediaTaskCompletionSource?.SetResult(new Tuple<Stream, string, MGFileType>(stream, displayName,
                                    mime.Contains("video") ? MGFileType.Video : MGFileType.Photo));
                }
                else
                {
                    PickImageTaskCompletionSource?.SetResult(null);
                }
            }

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {

            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 7890)
                FingerPringPermissionsResultReady?.Invoke(this, grantResults.Any(t => t == Permission.Granted));
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                {
                    // var msg = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                }
                else
                {
                    // var msg = "This device is not supported";
                    Finish();
                }
                return false;
            }
            else
            {
                // var msg = "Google Play Services is available.";
                return true;
            }
        }
    }



}