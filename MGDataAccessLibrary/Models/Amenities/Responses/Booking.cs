﻿using System;
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
        public string DisplayDateTime => FromDate.ToString("ddd, MMM dd, yyyy") + " • " + FromDate.ToString("h:mmtt") + " to " + ToDate.ToString("h:mmtt");
        public string DisplayAddress => BuildingName + ", " + UnitName;
        public AuditLog AuditLog { get; set; }
        public string Icon { get; set; }
        public string LinkUrl => "https://ploop.dynamo-ny.com" + Icon.Replace(".svg", ".png");
        public double BookingFee { get; set; }
        public double SecurityDepositFee { get; set; }
        public string ProvidedTenantName { get; set; }
        public int PaymentStatus { get; set; }
        public int? PaymentAccountId { get; set; }
        public bool IsSecurityDepositEnabled { get; set; }
        public bool IsBookingFeeEnabled { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusTextColor
        {
            get
            {
                switch (Status)
                {
                    case BookingStatus.Approved:
                        return "#54c723";
                    case BookingStatus.Declined:
                    case BookingStatus.Canceled:
                        return "#e33b3b";
                    default:
                        return "#a8a8a8";
                }
            }
        }

    }

    public enum BookingStatus
    {
        Pending = 0,
        Approved = 1,
        Declined = 2,
        Canceled = 3
    }
}
