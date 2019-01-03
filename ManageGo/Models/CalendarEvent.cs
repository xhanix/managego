using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
        public List<User> SendToUsers { get; set; }
        public List<Tenant> SendToTenant { get; set; }
        public List<ExternalContact> SendToExternalContacts { get; set; }
        public string SendToEmail { get; set; }

        public Unit Unit { get; set; }

        public Building Building { get; set; }

        [JsonIgnore]
        public string HeaderTime
        {
            get => TimeFrom + " to " + TimeTo;
        }
        [JsonIgnore]
        public string HeaderDate
        {
            get => Date.ToString("MMM dd, yy");
        }
        [JsonIgnore]
        public string HeaderAddress
        {
            get
            {
                if (Building is null)
                    return "No address";
                return Building.BuildingShortAddress + ", " + Unit?.UnitName;
            }
        }
    }
}
