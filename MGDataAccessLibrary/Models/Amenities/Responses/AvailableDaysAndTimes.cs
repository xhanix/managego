using System;
using System.Collections.Generic;

namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class AvailableDaysAndTimes
    {
        public DateTime Date { get; set; }
        /// <summary>
        /// If BookedBy value is null, that time is available
        /// </summary>
        public IEnumerable<TimeRanges> TimeRanges { get; set; }
    }
}
