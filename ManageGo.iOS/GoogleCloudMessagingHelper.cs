using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.CloudMessaging;
using Foundation;
using ManageGo.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(GoogleCloudMessagingHelper))]
namespace ManageGo.iOS
{
    public class GoogleCloudMessagingHelper : IGoogleCloudMessagingHelper
    {
        string _topic;

        public void SubscribeToTopic(string topic)
        {
            _topic = topic;
            if (Xamarin.Essentials.Preferences.Get(topic, false))
                return;
            try
            {
                Messaging.SharedInstance.Subscribe("/topics/" + topic);
                Xamarin.Essentials.Preferences.Set(topic, true);
                Console.WriteLine($"Subscribed to {topic}");
            }
            catch (NSErrorException ex)
            {
                throw new Exception(ex.Message);
            }
        }



        public void UnSubscribeFromTopics()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_topic))
                    return;
                Messaging.SharedInstance.Unsubscribe("/topics/" + _topic);
                Xamarin.Essentials.Preferences.Set(_topic, false);
                Console.WriteLine($"Subscribed to {_topic}");
            }
            catch (NSErrorException ex)
            {
                throw new Exception(ex.Message);
            }
        }


    }
}
