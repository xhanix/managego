using System;
using System.Collections.Generic;

namespace MGDataAccessLibrary.Models
{
    public class CreateTicketRequestItem
    {
        public int BuildingID { get; set; }
        public int UnitID { get; set; }
        public int TenantID { get; set; }
        public IEnumerable<int> Categories { get; set; }
        public int Status { get; set; }
        public int Priority { get; set; }
        public IEnumerable<int> Tags { get; set; }
        public DateTime? DueDate { get; set; }
        public IEnumerable<int> Assigned { get; set; }
        public string Subject { get; set; }
        public string Comment { get; set; }
        public string FileName { get; set; }
        public byte[] File { get; set; }
    }
}
