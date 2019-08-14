using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using CustomCalendar;
using Newtonsoft.Json;

namespace ManageGo
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore), Serializable]
    internal class TicketRequestItem
    {
        public int PageSize { get; set; } = 25;
        public int Page { get; set; } = 1;
        public int? Ticket { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public IList<int> Buildings { get; set; }
        public IList<int> Tags { get; set; }
        public IList<int> Assigned { get; set; }
        public IList<int> Categories { get; set; }
        public TicketStatus? TicketStatus { get; set; }
        public IList<TicketPriorities> Priorities { get; set; }
        public string Search { get; set; }
        [JsonIgnore, IgnoreDataMember]
        DateRange DefaultDateRange => new DateRange(DateTime.Today, DateTime.Today.AddDays(-30));
        [JsonIgnore, IgnoreDataMember]
        public DateTime DefaultStartDate => DefaultDateRange.StartDate;

        [JsonIgnore, IgnoreDataMember]
        public DateTime DefaultToDate => DefaultDateRange.EndDate.Value;

        [JsonIgnore]
        public int NumberOfAppliedFilters
        {
            get
            {
                var n = 0;
                if (Ticket != null)
                    n++;
                if (DueDateFrom != null)
                    n++;
                if (DueDateFrom != null)
                    n++;
                if (Categories != null && Categories.Any())
                    n++;
                if ((DateFrom != null && DateFrom.Value.Date != DefaultStartDate.Date) || (DateTo != null && DateTo.Value.Date != DefaultToDate.Date))
                    n++;
                if (Buildings != null && Buildings.Any())
                    n++;
                if (Tags != null && Tags.Any())
                    n++;
                if (Assigned != null && Assigned.Any())
                    n++;
                if (TicketStatus != ManageGo.TicketStatus.Open)
                    n++;
                if (Priorities != null)
                    n++;
                if (!string.IsNullOrWhiteSpace(Search))
                    n = 1;
                return n;
            }
        }

        public TicketRequestItem Clone()
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, this);
                ms.Position = 0;
                return (TicketRequestItem)formatter.Deserialize(ms);
            }
        }
    }
}
