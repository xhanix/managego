using System;
namespace ManageGo.Models
{
    public enum PushNotificationType
    {
        TestNotification = -1000,
        TicketCreated = 1,
        TicketReply = 2,
        UnitAwaitingApproval = 7,
        TicketReplyInternal = 8,
        TicketAssigned = 9,
        TenantAwaitingApproval = 10,
        PaymentReceived = 11
    }
}
