using System;
using System.Threading.Tasks;
using Firebase.Iid;
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
            var oldSub = Xamarin.Essentials.Preferences.Get("subscribed", string.Empty);
            if (!string.IsNullOrWhiteSpace(oldSub))
                FirebaseMessaging.Instance.UnsubscribeFromTopic("/topics/" + oldSub);
            FirebaseMessaging.Instance.SubscribeToTopic("/topics/" + topic);
            //Console.WriteLine($"FCM token: {FirebaseInstanceId.Instance.Token}");
            FirebaseMessaging.Instance.SubscribeToTopic("dev_0505_dev");
            Xamarin.Essentials.Preferences.Set("subscribed", topic);
            Console.WriteLine($"Subscribed to {topic}");
            _topic = topic;
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
