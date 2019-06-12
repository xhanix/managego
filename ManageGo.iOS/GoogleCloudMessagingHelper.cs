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
            try
            {
                var oldSub = Xamarin.Essentials.Preferences.Get("subscribed", string.Empty);
                if (!string.IsNullOrWhiteSpace(oldSub))
                    Messaging.SharedInstance.Unsubscribe("/topics/" + oldSub);
                Messaging.SharedInstance.Subscribe("/topics/" + topic);
                Xamarin.Essentials.Preferences.Set("subscribed", topic);
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
