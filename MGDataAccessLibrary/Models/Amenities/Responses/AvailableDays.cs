using System;
using System.Collections.Generic;

namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class AvailableDays
    {
        public int MaxBookingLength { get; set; }
        public IEnumerable<AvailableDaysAndTimes> AvailableDaysAndTimes { get; set; }
    }
}
