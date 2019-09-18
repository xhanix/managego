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

        public override void OnMessageReceived(RemoteMessage message)
        {
            //notification type is in Message.Notification.ClickAction
            //ObjectId is in Message.Data.NotificationObject
            var notification = message.GetNotification();
            Analytics.TrackEvent($"FCM.notification.body: {notification.Body}, FCM.notification.ClickAction: {notification.ClickAction}");
            //NotificationObject was the id of the item to show. Need to add this.
            Log.Debug(TAG, "From: " + message.From);
            Log.Debug(TAG, "Notification Message Body: " + notification.Body);
            if (message.Data.TryGetValue("NotificationObject", out string notificationObject)
                && int.TryParse(notificationObject, out int objectId)
                && Enum.TryParse(notification.ClickAction, out Models.PushNotificationType _type))
            {
                Models.PushNotificationMessage pushNotification = new Models.PushNotificationMessage
                {
                    Type = (int)_type,
                    NotificationObject = objectId,
                    Body = notification.Body,
                    Title = notification.Title
                };
                SendNotification(notification.Title, notification.Body, pushNotification);
            }
        }

        private void SendNotification(string messageTitle, string messageBody, Models.PushNotificationMessage data)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.SingleTop);
            if (data != null)
            {
                intent.PutExtra("IsGroup", 0);
                intent.PutExtra("Type", data.Type);
                //id of item to show on click
                intent.PutExtra("NotificationObject", data.NotificationObject);
            }
            int uniqueInt = (int)(DateTime.Now.Millisecond & 0xfffffff);
            var pendingIntent = PendingIntent.GetActivity(this, uniqueInt, intent, PendingIntentFlags.UpdateCurrent);
            var _intent = new Intent(this, typeof(MainActivity));
            _intent.SetFlags(ActivityFlags.SingleTop);
            if (data != null)
            {
                _intent.PutExtra("IsGroup", 1);
                _intent.PutExtra("Type", data.Type);
                _intent.PutExtra("NotificationObject", data.NotificationObject);
            }
            uniqueInt = (int)(DateTime.Now.Millisecond & 0xfffffff);
            var _pendingIntent = PendingIntent.GetActivity(this, uniqueInt, _intent, PendingIntentFlags.UpdateCurrent);
            NotificationCompat.Builder groupBuilder =
                new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                    .SetSmallIcon(Resource.Drawable.not_icon_white)
                    .SetGroupSummary(true)
                    .SetGroup($"com.ManageGo.ManageGo.{data.Type}")
                    .SetStyle(new NotificationCompat.InboxStyle())
                    .SetAutoCancel(true)
                    .SetContentIntent(_pendingIntent);

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                .SetSmallIcon(Resource.Drawable.not_icon_white)
                .SetContentTitle(messageTitle).SetContentText(messageBody)
                .SetStyle(new NotificationCompat.BigTextStyle())
                .SetAutoCancel(true)
                .SetGroup($"com.ManageGo.ManageGo.{data.Type}")
                .SetContentIntent(pendingIntent);
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(data.Type, groupBuilder.Build());
            notificationManager.Notify(Guid.NewGuid().GetHashCode(), notificationBuilder.Build());
        }
    }
}
