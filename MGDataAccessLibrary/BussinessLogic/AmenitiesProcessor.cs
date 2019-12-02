using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MGDataAccessLibrary.BussinessLogic
{
    public static class AmenitiesProcessor
    {
        public static async Task<Models.Amenities.Responses.BookingList> GetBookingList(Models.Amenities.Requests.BookingsList parameters)
        {
            var stringPars = parameters.GetQueryString();
            return await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.BookingList>("bookings?" + stringPars);
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

        public static async Task<IEnumerable<Models.Amenities.Responses.PMCBuilding>> GetPMCBuildings()
        {
            var pmcInfo = await DataAccess.AmenitiesAPI.GetItems<Models.Amenities.Responses.PMCInfo>("/pmc-user-info");
            return pmcInfo.BuildingsAccess;
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

    }
}
