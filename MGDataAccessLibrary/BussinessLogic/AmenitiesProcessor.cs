using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MGDataAccessLibrary.BussinessLogic
{
    public static class AmenitiesProcessor
    {
        public static async Task<Models.Amenities.Responses.BookingList> GetBookingList(Models.Amenities.Requests.BookingsList parametes)
        {
            var stringPars = parametes.GetQueryString();
            return await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.BookingList>("bookings?" + stringPars);
        }

        public static Models.Amenities.Responses.AvailableDays GetAvailableDays()
        {
            throw new NotImplementedException();
        }

        public static IEnumerable<Models.Amenities.Responses.Amenity> GetAmenities()
        {
            throw new NotImplementedException();
        }

    }
}
