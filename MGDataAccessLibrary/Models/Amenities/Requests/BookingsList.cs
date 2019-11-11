using System;
namespace MGDataAccessLibrary.Models.Amenities.Requests
{
    public class BookingsList
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        /// <summary>
        /// Comma-separated building ids
        /// </summary>
        public string Buildings { get; set; }
        /// <summary>
        /// Comma-separated Amenity ids
        /// </summary>
        public string Amenities { get; set; }
        /// <summary>
        /// Comma-separated Status codes.
        /// Status IDs: Pending = 0, Approved = 1, Declined = 2, Canceled = 3
        /// </summary>
        public string Statuses { get; set; }
        /// <summary>
        /// Search by tenant name, unit name, booking ID
        /// </summary>
        public string Search { get; set; }
        /// <summary>
        /// Code for sort style.
        /// Available codes:
        /// DateFrom = 0, DateFromDesc = 1, AmenityName = 2,
        /// AmenityNameDesc = 3, BuildingName = 4,
        /// BuildingNameDesc = 5, Status = 6, StatusDesc = 7,
        /// BookingId = 8, BookingIdDesc = 9, TenantName = 10,
        /// TenantNameDesc = 11
        /// </summary>
        public int Sort { get; set; }
        public int PageSize { get; set; }
        public int Page { get; set; }

    }
}
