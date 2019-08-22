using System;
namespace ManageGo.Models
{
    public class PushNotificationMessage
    {
        public int Type { get; set; }
        public int NotificationObject { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }


    }
}
