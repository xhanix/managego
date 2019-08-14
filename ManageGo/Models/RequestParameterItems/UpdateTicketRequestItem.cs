using System;
using System.Collections.Generic;

namespace ManageGo.Models
{
    public class UpdateTicketRequestItem
    {
        public int TicketID { get; set; }
        public int BuildingID { get; set; }
        public int? UnitID { get; set; }
        public int TenantID { get; set; }
        public IEnumerable<int> Categories { get; set; }
        public TicketStatus Status { get; set; }
        public TicketPriorities Priority { get; set; }
        public IEnumerable<int> Tags { get; set; }
        public DateTime? DueDate { get; set; }
        public IEnumerable<int> Assigned { get; set; }
        public string Comment { get; set; }
        public string FileName { get; set; }
        public string File { get; set; }
    }
}
