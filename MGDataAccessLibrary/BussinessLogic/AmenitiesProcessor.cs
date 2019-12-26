using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HashidsNet;
using MGDataAccessLibrary.Models.Amenities.Requests;
using MGDataAccessLibrary.Models.Amenities.Responses;

namespace MGDataAccessLibrary.BussinessLogic
{
    public static class AmenitiesProcessor
    {
        static readonly string alphabet = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        static readonly string seps = "mg@#cplPMC";
        static Hashids hashids = new Hashids("", 0, alphabet, seps);


        public static async Task<Models.Amenities.Responses.BookingList> GetBookingList(BookingsList parameters)
        {
            if (parameters.RawStatuses.Any())
            {
                parameters.Statuses = hashids.Encode(parameters.RawStatuses);
            }
            else
            {
                parameters.Statuses = null;
            }
            var stringPars = parameters.GetQueryString();
            return await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.BookingList>("bookings?" + stringPars);
        }

        public static async Task<Models.Amenities.Responses.Booking> GetBooking(int id)
        {
            return await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.Booking>($"bookings/{id}");
        }



        public static async Task<Models.Amenities.Responses.AvailableDays> GetAvailableDays(Models.Amenities.Requests.AvailableDays parameters, int amenityId)
        {
            var stringPars = parameters.GetQueryString();
            return await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.AvailableDays>($"amenities/{amenityId}/available-days?" + stringPars);
        }

        public static async Task<IEnumerable<Models.Amenities.Responses.Amenity>> GetAmenities()
        {
            return await DataAccess.AmenitiesAPI.GetItems<IEnumerable<Models.Amenities.Responses.Amenity>>("amenities");
        }

        public static async Task<(Models.Amenities.Responses.PMCUserInfo, Models.Amenities.Responses.PMCInfo)> GetPMCInfo()
        {
            var item1 = await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.PMCUserInfo>("/pmc-user-info");
            var item2 = await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.PMCInfo>("/pmc-info");
            return (item1, item2);
        }

        public static async Task<IEnumerable<Models.Amenities.Responses.BuildingUnit>> GetBuildingUnits(int buildingId)
        {
            return await DataAccess.AmenitiesAPI.GetItems<IEnumerable<Models.Amenities.Responses.BuildingUnit>>($"/units?buildingId={buildingId}");

        }

        public static async Task<IEnumerable<Models.Amenities.Responses.UnitTenant>> GetUnitTenants(int unitId)
        {
            return await DataAccess.AmenitiesAPI.GetItems<IEnumerable<Models.Amenities.Responses.UnitTenant>>($"/tenants/{unitId}");

        }

        public static async Task<Models.Amenities.Responses.Amenity> GetAmenity(int id, int buildingId)
        {
            return await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.Amenity>($"amenities/{id}?buildingId={buildingId}");
        }

        public static async Task CreateBooking(CreateBooking request)
        {
            await DataAccess.AmenitiesAPI.PostItem<CreateBooking>($"bookings", request);
        }

        public static async Task<CalculatedBookingFee> CalculateBookingFee(int amenityId, int unitId, int bookingTotalMinutes)
        {
            return await DataAccess.AmenitiesAPI.GetItems<CalculatedBookingFee>($"amenities/{amenityId}/calculate-booking-fee?unitId={unitId}&bookingLength={bookingTotalMinutes}");
        }

        public static async Task SetBookingStatus(int id, string notes, BookingStatus status)
        {
            var path = $"bookings/status?ids={id}&status={(int)status}&note={notes}";
            await DataAccess.AmenitiesAPI.PatchItem(path);
        }
    }
}
