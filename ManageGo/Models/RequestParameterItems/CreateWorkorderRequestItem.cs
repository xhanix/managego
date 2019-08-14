using System;
using System.Collections.Generic;

namespace ManageGo.Models
{
    public class CreateWorkorderRequestItem
    {
        public int TicketID { get; set; }
        public string Summary { get; set; }
        public string Details { get; set; }
        public IEnumerable<int> SendToUsers { get; set; }
        public IEnumerable<int> SendToExternalContacts { get; set; }
        public string SendToEmail { get; set; }
    }
}
