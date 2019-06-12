using System;
namespace MGDataAccessLibrary.Models
{
    public class SignedInUserInfo
    {
        public string AccessToken { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmailAddress { get; set; }
        public int UserID { get; set; }
        public bool PaymentPushNotification { get; set; }
        public bool MaintenancePushNotification { get; set; }
        public bool TenantPushNotification { get; set; }
        public bool PushNotification { get; set; }
    }
}
