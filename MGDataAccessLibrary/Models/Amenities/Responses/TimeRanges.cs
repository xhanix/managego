using System;
namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class TimeRanges
    {
        public int From { get; set; }
        public int To { get; set; }
        public BookedBy BookedBy { get; set; }
    }
}
