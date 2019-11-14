using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;

namespace ManageGo
{
    internal class AmenitiesListPageModel : BaseDetailPage
    {
        private const int pageSize = 50;
        private int currentPage = 1;
        private MGDataAccessLibrary.Models.Amenities.Requests.BookingsList filter = new MGDataAccessLibrary.Models.Amenities.Requests.BookingsList
        {
            PageSize = pageSize,
            Page = 1
        };

        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking> Bookings { get; private set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking>();
        public string PendingString { get; set; }
        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            var list = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetBookingList(filter);

            if (list.List is null && !list.List.Any())
            {
                Bookings.Clear();
            }
            else
            {
                Bookings = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking>(list.List);
            }
            ((AmenitiesListPage)CurrentPage).DataLoaded();
            PendingString = list.TotalPending > 0 ? $"({list.TotalPending} pending)" : string.Empty;
        }

        public FreshAwaitCommand OnViewCalendarTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    await CoreMethods.PushPageModel<BookingCalendarPageModel>();
                    tcs?.SetResult(true);
                });
            }
        }

    }
}

