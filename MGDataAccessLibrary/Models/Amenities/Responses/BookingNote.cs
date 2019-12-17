using System;
using Newtonsoft.Json;

namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class BookingNote
    {
        public DateTime TimeStamp { get; set; }
        public string Note { get; set; }
        [JsonIgnore]
        public string DisplayName => User?.Name;
        [JsonIgnore]
        public string DisplayDate => TimeStamp.ToString("MMM dd - h:mm tt");
        public AuditLogUser User { get; set; }
    }
}