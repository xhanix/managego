using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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

        [JsonIgnore]
        public int NumberOfAppliedFilters
        {
            get
            {
                var n = 0;
                if (Ticket != null)
                    n++;
                if (DateFrom != null)
                    n++;
                if (DateTo != null)
                    n++;
                if (DueDateFrom != null)
                    n++;
                if (DueDateTo != null)
                    n++;
                if (Buildings.Count > 0)
                    n++;
                if (Tags.Count > 0)
                    n++;
                if (Assigned.Count > 0)
                    n++;
                if (Status != TicketStatus.All)
                    n++;
                if (Priorities != null)
                    n++;
                if (!string.IsNullOrWhiteSpace(Search))
                    n++;
                return n;
            }
        }
    }
}
