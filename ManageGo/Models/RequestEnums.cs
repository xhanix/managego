using System;
namespace ManageGo
{

    public enum TicketPriorities
    {
        Open,
        Closed,
        Unread,
    }

    public enum TicketStatus
    {
        All = -1,
        Open = 0,
        Closed = 1
    }
}
