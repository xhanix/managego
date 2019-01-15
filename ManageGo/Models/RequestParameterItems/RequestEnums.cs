using System;

namespace ManageGo
{
    /// <summary>
    /// Ticket priorities.
    /// Values specified in API documentation
    /// </summary>
    public enum TicketPriorities
    {
        Low,
        Medium,
        High,
    }
    /// <summary>
    /// Ticket status.
    /// Values specified in API documentation
    /// </summary>
    public enum TicketStatus
    {
        All = -1,
        Open = 0,
        Closed = 1
    }
    /// <summary>
    /// Tenant status.
    /// Values specified in API documentation
    /// </summary>
    public enum TenantStatus
    {
        Active,
        Inactive,
        All
    }
}
