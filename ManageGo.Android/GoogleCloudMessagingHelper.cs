using System;
using System.Threading.Tasks;
using Firebase.Messaging;
using ManageGo.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(GoogleCloudMessagingHelper))]
namespace ManageGo.Droid
{
    public class GoogleCloudMessagingHelper : IGoogleCloudMessagingHelper
    {
        string _topic;

        public void SubscribeToTopic(string topic)
        {
            _topic = topic;
            if (Xamarin.Essentials.Preferences.Get(topic, false))
                return;
            FirebaseMessaging.Instance.SubscribeToTopic("/topics/" + topic);
            Xamarin.Essentials.Preferences.Set(topic, true);
            //  FirebaseMessaging.Instance.Send(msg.Build());



        }

        public void UnSubscribeFromTopics()
        {

            if (string.IsNullOrWhiteSpace(_topic))
                return;
            FirebaseMessaging.Instance.UnsubscribeFromTopic("/topics/" + _topic);
            Xamarin.Essentials.Preferences.Set(_topic, false);
            Console.WriteLine($"Subscribed to {_topic}");

        }
    }
}
