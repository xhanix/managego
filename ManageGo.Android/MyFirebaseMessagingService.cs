using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.App;
using Android.Util;
using Firebase.Messaging;
using Microsoft.AppCenter.Analytics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Android.App.Notification;

namespace ManageGo.Droid
{

    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class MyFirebaseMessagingService : FirebaseMessagingService
    {
        const string TAG = "MyFirebaseMsgService";

        public override void OnNewToken(string p0)
        {
            Analytics.TrackEvent($"FCM Token: {p0}");
            base.OnNewToken(p0);
        }

        public override void OnMessageReceived(RemoteMessage p0)
        {
            //notification type is in Message.Notification.ClickAction
            //ObjectId is in Message.Data.NotificationObject
            var notification = p0.GetNotification();
            Analytics.TrackEvent($"FCM.notification.body: {notification.Body}, FCM.notification.ClickAction: {notification.ClickAction}");
            //NotificationObject was the id of the item to show. Need to add this.
            if (int.TryParse(p0.Data["NotificationObject"], out int objectId))
            {
                Models.PushNotificationMessage pushNotification = new Models.PushNotificationMessage
                {
                    NotificationObject = objectId,
                    Body = notification.Body,
                    Title = notification.Title
                };
                var _type = !string.IsNullOrWhiteSpace(notification.ClickAction) ? notification.ClickAction : p0.Data["GroupId"];
                SendNotification(notification.Title, notification.Body, pushNotification, _type);
            }
        }

        private void SendNotification(string messageTitle, string messageBody, Models.PushNotificationMessage data, string groupType)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.SingleTop);
            var _type = ManageGo.Models.PushNotificationType.TicketCreated;
            Enum.TryParse(groupType, out _type);
            if (data != null)
            {
                intent.PutExtra("IsGroup", $"{0}");
                intent.PutExtra("Type", groupType);
                //id of item to show on click
                intent.PutExtra("NotificationObject", $"{data.NotificationObject}");
            }
            int uniqueInt = (int)(DateTime.Now.Millisecond & 0xfffffff);
            var pendingIntent = PendingIntent.GetActivity(this, uniqueInt, intent, PendingIntentFlags.UpdateCurrent);
            var _intent = new Intent(this, typeof(MainActivity));
            _intent.SetFlags(ActivityFlags.SingleTop);
            if (data != null)
            {
                _intent.PutExtra("IsGroup", $"{1}");
                _intent.PutExtra("Type",groupType);
                _intent.PutExtra("NotificationObject", $"{data.NotificationObject}");
            }
            uniqueInt = (int)(DateTime.Now.Millisecond & 0xfffffff);
            var _pendingIntent = PendingIntent.GetActivity(this, uniqueInt, _intent, PendingIntentFlags.UpdateCurrent);
            NotificationCompat.Builder groupBuilder =
                new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                    .SetSmallIcon(Resource.Drawable.not_icon_white)
                    .SetGroupSummary(true)
                    .SetGroup($"com.ManageGo.ManageGo.{_type}")
                    .SetStyle(new NotificationCompat.InboxStyle())
                    .SetAutoCancel(true)
                    .SetContentIntent(_pendingIntent);

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                .SetSmallIcon(Resource.Drawable.not_icon_white)
                .SetContentTitle(messageTitle)
                .SetContentText(messageBody)
                .SetAutoCancel(true)
                .SetGroup($"com.ManageGo.ManageGo.{_type}")
                .SetContentIntent(pendingIntent);



            var notificationManager = NotificationManagerCompat.From(this);
          
            notificationManager.Notify((int)_type, groupBuilder.Build());
            notificationManager.Notify(Guid.NewGuid().GetHashCode(), notificationBuilder.Build());
        }
    }
}
