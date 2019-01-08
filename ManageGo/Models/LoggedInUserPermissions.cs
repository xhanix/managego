using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ManageGo.Models
{
    public class LoggedInUserPermissions
    {
        [Obsolete("This may be removed in future versions of the backend service")]
        public IList<int> ListOfBuildingsUserHasAccess { get; set; }

        [JsonProperty("AccessToPayments")]
        public bool CanAccessPayments { get; set; }

        [JsonProperty("AccessToTenants")]
        public bool CanAccessTenants { get; set; }

        [JsonProperty("AccessToMaintenance")]
        public bool CanAccessMaintenanceTickets { get; set; }

        [Obsolete("This may be removed in future versions of the backend service")]
        [JsonProperty("AccessToMailer")]
        public bool CanAccessMailer { get; set; }


        public bool CanAddWorkordersAndEvents { get; set; }

        public bool CanApproveNewTenantsUnits { get; set; }

        public bool CanReplyInternally { get; set; }

        public bool CanReplyPublicly { get; set; }


        public bool CanEditTicket { get; set; }

        [Obsolete("This may be removed in future versions of the backend service")]
        [JsonProperty("LimitToAssignedTicketsOnly")]
        public bool IsLimitedToAssignedTickets { get; set; }
    }
}
