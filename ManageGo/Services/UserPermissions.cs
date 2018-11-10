using System;
namespace ManageGo
{
    [Flags]
    public enum UserPermissions
    {
        None,
        CanAccessPayments,
        CanAccessTickets
    }
}
