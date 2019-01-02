using System;
using System.Collections.Generic;

namespace ManageGo.Models
{
    public class CalendarEvent
    {
        public int EventID { get; set; }
        public int TicketID { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public List<int> SendToUsers { get; set; }
        public List<int> SendToTenant { get; set; }
        public List<int> SendToExternalContacts { get; set; }
        public string SendToEmail { get; set; }

        public Unit Unit { get; set; }

        public Building Building { get; set; }
    }
}
