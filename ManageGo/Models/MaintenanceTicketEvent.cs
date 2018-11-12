using System;
using System.Collections.Generic;

namespace ManageGo
{
    public class MaintenanceTicketEvent
    {
        public int? EventId { get; set; }
        public int TicketId { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public List<User> SendToUsers { get; set; }
        public List<Tenant> SendToTenant { get; set; }
        public List<MaintenanceExternalContact> SendToExternalContacts { get; set; }
        public string SendToEmail { get; set; }
        public Unit Unit { get; set; }
        public Building Building { get; set; }
    }
}
