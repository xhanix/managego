using System;
using System.ComponentModel;

namespace MGDataAccessLibrary.Models.Amenities.Requests
{
    public class CreateBooking
    {
        public int AmenityId { get; set; }
        public int UnitId { get; set; }
        public int BuildingId { get; set; }
        public int? TenantId { get; set; }
        public string ProvidedTenantName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Note { get; set; }
        // public int PaymentAccountId { get; set; }
        // public string GuestList { get; set; }
        public double BookingFee { get; set; }
        public bool Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }

    public enum PaymentStatus
    {
        [Description("Not Applicable")]
        NotApplicable,
        Paid,
        Requested,
        Scheduled,
        Refunded,
        Reversed,
        Voided,
        Failed,
        NotPaid
    }
}
