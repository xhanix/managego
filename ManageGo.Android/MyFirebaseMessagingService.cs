using System;
using System.Collections.Generic;
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
        public override void OnMessageReceived(RemoteMessage message)
        {
            var not = message.GetNotification();
            if (message.Data.TryGetValue("message", out string _message))
            {
                var msg = JsonConvert.DeserializeObject<Models.PushNotificationMessage>(_message);

                var _type = (Models.PushNotificationType)msg.Type;
                var _notificationObject = msg.NotificationObject;

                SendNotification(msg.Title, msg.Body, msg);
            }

        }

        private void SendNotification(string messageTitle, string messageBody, Models.PushNotificationMessage data)
        {
            var intent = new Intent(this, typeof(MainActivity));

            intent.AddFlags(ActivityFlags.ClearTop);
            if (data != null)
            {
                intent.PutExtra("Type", data.Type);
                intent.PutExtra("NotificationObject", data.NotificationObject);
            }
            var pendingIntent = PendingIntent.GetActivity(this, MainActivity.NOTIFICATION_ID, intent, PendingIntentFlags.OneShot);
            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                .SetSmallIcon(Resource.Drawable.notification_icon)
                .SetContentTitle(messageTitle).SetContentText(messageBody)
                .SetStyle(new NotificationCompat.BigTextStyle())
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent);
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(MainActivity.NOTIFICATION_ID, notificationBuilder.Build());
        }
    }
}
