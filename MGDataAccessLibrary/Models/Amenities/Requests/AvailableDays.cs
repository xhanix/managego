using System;
namespace MGDataAccessLibrary.Models.Amenities.Requests
{
    public class AvailableDays
    {
        public int UnitId { get; set; }
        public int BuildingId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
