using System;
using System.Collections.Generic;

namespace ManageGo
{
    internal class TicketRequest
    {
        public int PageSize { get; set; }
        public int Page { get; set; }
        public int? Ticket { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public IList<int> Buildings { get; set; }
        public IList<int> Tags { get; set; }
        public IList<int> Assigned { get; set; }
        public TicketStatus Status { get; set; }
        public IList<TicketPriorities> Priorities { get; set; }
        public string Search { get; set; }
    }


}
