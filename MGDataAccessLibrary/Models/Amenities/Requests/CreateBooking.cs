using System;
namespace MGDataAccessLibrary.Models.Amenities.Requests
{
    public class CreateBooking
    {
        public int AmenityId { get; set; }
        public int UnitId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Note { get; set; }
        public int PaymentAccountId { get; set; }
        public string GuestList { get; set; }
    }
}
