using System;
using System.Collections.Generic;

namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class BookingList
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPending { get; set; }
        public IEnumerable<Booking> List { get; set; }
    }
}