using System;
namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class BookedBy
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProvidedTenantName { get; set; }
        public int TenantId { get; set; }
        public int BookingId { get; set; }
        public int Status { get; set; }
    }
}
