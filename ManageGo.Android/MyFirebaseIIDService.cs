using System;
using Android.App;
using Firebase.Iid;
using Android.Util;
using Firebase.Messaging;

namespace ManageGo.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseMessagingService
    {
       
        public override void OnNewToken(string p0)
        {
            base.OnNewToken(p0);

            var refreshedToken = p0;
            Console.WriteLine("Refreshed token: " + refreshedToken);
            SendRegistrationToServer(refreshedToken);
        }
        void SendRegistrationToServer(string token)
        {
            // Add custom implementation, as needed. 
        }

    }
}

