using System;
namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class Booking
    {
        public int AmenityBookingId { get; set; }
        public string AmenityName { get; set; }
        public string BuildingName { get; set; }
        public string TenantName { get; set; }
        public string UnitName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public AuditLog AuditLog { get; set; }
        public string Icon { get; set; }
        public double BookingFee { get; set; }
        public double SecurityDepositFee { get; set; }
        public string ProvidedTenantName { get; set; }
        public int PaymentStatus { get; set; }
        public int? PaymentAccountId { get; set; }
        public bool IsSecurityDepositEnabled { get; set; }
        public bool IsBookingFeeEnabled { get; set; }
        public int Status { get; set; }
    }
}
