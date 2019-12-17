using System;

namespace ManageGo
{
    [Flags]
    public enum UserPermissions
    {
        None = 0,
        CanAccessPayments = 2,
        CanAccessTickets = 4,
        CanAccessTenants = 8,
        [Obsolete("This may be removed in future API versions")]
        CanAccessMailer = 16,
        CanReplyPublicly = 32,
        CanReplyInternally = 64,
        CanApproveNewTenantsUnits = 128,
        CanAddWorkordersAndEvents = 256,
        CanEditTicketDetails = 512,
        CanAccessAmenities = 1024
        //[Obsolete("This may be removed in future API versions")]
        // IsLimitedToAssignedTickets = 1024
    }
}
