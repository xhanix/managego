using System;
namespace MGDataAccessLibrary.Models.Amenities.Responses
{
    public class CalculatedBookingFee
    {
        public string ConvenienceFeeDescription { get; set; }
        public string BookingFeeDescription { get; set; }
        public double BookingFeeAmount { get; set; }
        public double TotalAmount { get; set; }
        public double SecurityDepositFeeTotalAmount { get; set; }
    }
}
