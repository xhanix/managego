using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;

namespace ManageGo
{
    internal class BookingCalendarPageModel : BaseDetailPage
    {
        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            //get all ameneties

            var amenities = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAmenities();
            var firstAmenityId = amenities.First().Id;
            //get available days for first amenity
            var parameters = new MGDataAccessLibrary.Models.Amenities.Requests.AvailableDays
            {
                BuildingId = amenities.First().Buildings.First().BuildingId,
                From = DateTime.Now.ToString("yyyy-MM-dd"),
                To = DateTime.Now.ToString("yyyy-MM-dd")
            };
            var availableDate = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAvailableDays(parameters, firstAmenityId);
            Console.WriteLine(availableDate.AvailableDaysAndTimes);
        }
    }
}

