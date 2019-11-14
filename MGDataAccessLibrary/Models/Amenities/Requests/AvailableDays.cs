using System;
namespace MGDataAccessLibrary.Models.Amenities.Requests
{
    public class AvailableDays
    {
        public int UnitId { get; set; }
        public int BuildingId { get; set; }
        //need to convert to string for backend to accept the values (it's not working with datetime at the moment)
        public string From { get; set; }
        public string To { get; set; }
    }
}
