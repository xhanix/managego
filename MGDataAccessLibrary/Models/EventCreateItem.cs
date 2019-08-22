using System;
using System.Collections.Generic;

namespace MGDataAccessLibrary.Models
{
    public class EventCreateItem
    {
        public int TicketID { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
        public DateTime EventDate { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public IEnumerable<int> SendToUsers { get; set; }
        public IEnumerable<int> SendToTenant { get; set; }
        public IEnumerable<int> SendToExternalContacts { get; set; }
        public string SendToEmail { get; set; }
    }
}