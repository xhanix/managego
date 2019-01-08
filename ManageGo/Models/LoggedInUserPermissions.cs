using System;
using Newtonsoft.Json;

namespace ManageGo.Models
{
    public class LoggedInUserPermissions
    {
        [Obsolete("This may be removed in future versions of the backend service")]
        public bool ListOfBuildingsUserHasAccess { get; set; }

        [JsonProperty("AccessToPayments")]
        public bool CanAccessPayments { get; set; }

        [JsonProperty("AccessToTenants")]
        public bool CanAccessTenants { get; set; }

        [JsonProperty("AccessToMaintenance")]
        public bool CanAccessMaintenanceTickets { get; set; }

        [JsonProperty("AccessToMailer")]
        public bool CanAccessMailer { get; set; }


        public bool CanReplyPublicly { get; set; }

        [JsonProperty("LimitToAssignedTicketsOnly")]
        public bool IsLimitedToAssignedTickets { get; set; }
    }
}
