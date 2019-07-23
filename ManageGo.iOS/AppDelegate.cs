using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firebase.CloudMessaging;
using Firebase.Core;
using Foundation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ObjCRuntime;
using UIKit;
using UserNotifications;

namespace ManageGo.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());
            Firebase.Core.App.Configure();
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {

                UNUserNotificationCenter.Current.Delegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert | UNAuthorizationOptions.Sound | UNAuthorizationOptions.Sound,
                                                                        (granted, error) =>
                                                                        {
                                                                            if (granted)
                                                                                InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
                                                                        });
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                        UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                        new NSSet());
                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                UIRemoteNotificationType notificationTypes = UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Badge;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);
            }
            Messaging.SharedInstance.Delegate = this;
            Messaging.SharedInstance.ShouldEstablishDirectChannel = true;
            Messaging.SharedInstance.AutoInitEnabled = true;
            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        [Export("messaging:didReceiveRegistrationToken:")]
        public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
        {
            Console.WriteLine($"Firebase registration token: {fcmToken}");
        }

        public override void OnActivated(UIApplication uiApplication)
        {
            base.OnActivated(uiApplication);
            Messaging.SharedInstance.ShouldEstablishDirectChannel = true;
        }

        public override void DidEnterBackground(UIApplication uiApplication)
        {
            base.DidEnterBackground(uiApplication);
            Messaging.SharedInstance.ShouldEstablishDirectChannel = false;
        }

        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
        {
            var token = deviceToken.Description.Replace("<", "").Replace(">", "").Replace(" ", "");
        }

        [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
        public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
        {
            //triggered when app is in foreground
            var userInfo = notification.Request.Content.UserInfo;
            //userInfo.NotificationObject
            completionHandler(UNNotificationPresentationOptions.Alert);
        }


        public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
        {
            //triggered when user taps on notification
            var aps = userInfo["aps"] as NSDictionary;
            var type = aps["category"] as NSString;
            var notifObject = userInfo["NotificationObject"] as NSString;
            if (int.TryParse(notifObject, out int objectId))
            {
                var _type = Enum.Parse<Models.PushNotificationType>(type);
                App.NotificationReceived((int)_type, objectId, false).ConfigureAwait(false);
            }
        }

        public static void ShowMessage(string title, string message, UIViewController fromViewController, Action actionForOk = null)
        {
            var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, (obj) => actionForOk?.Invoke()));
            fromViewController.PresentViewController(alert, true, null);
        }
    }
}
