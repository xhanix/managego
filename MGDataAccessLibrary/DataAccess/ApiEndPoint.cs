namespace MGDataAccessLibrary.DataAccess
{
    public enum ApiEndPoint
    {
        dashboard,
        authorize,
        tickets,
        buildings,
        BuildingDetails,
        MaintenanceObjects, //provides list of Categories,Tags, external contacts
        Users,
        CreateTicket,
        TicketsDetails,
        TicketNewComment,
        CommentNewFile,
        CommentFilesCompleted,
        GetTicketFile,
        CreateWorkOrder,
        CreateEvent,
        UpdateTicket,
        Tenants,
        Payments,
        BankTransactions,
        BankAccounts,
        UserSettings,
        EventsListDates,
        EventList,
        PendingApprovals,
        PendingApprovalAction,
        ResetPassword
    }
}