using System;

using FreshMvvm;

namespace ManageGo
{
    public class SettingsPageModel : FreshBasePageModel
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserAlias { get; set; }
        public string UserPassword { get; set; }
        public bool PushNotificationsIsOn { get; set; }
        public bool BiometricLoginIsOn { get; set; }
        public bool PaymentNotificationsIsOn { get; set; }
        public bool MaintenanceNotificationsIsOn { get; set; }
        public bool TenantsNotificationsIsOn { get; set; }
        public bool ApplicationsNotificationsIsOn { get; set; }

    }
}

