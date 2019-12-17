using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;

namespace ManageGo
{
    internal class AmenitiesListPageModel : BaseDetailPage
    {
        private const int pageSize = 50;
        private int currentPage = 1;
        private DateRange selectedDateRange;
        public string CompanyUrl { get; private set; }
        public bool CalFilterIsvisible { get; set; }
        public string SelectedDateRangeString => !filter.From.HasValue || !filter.To.HasValue ? "Select Dates" : filter.From?.ToString("MMM dd") + " - " + filter.To?.ToString("MMM dd");
        public DateRange SelectedDateRange { get; set; } = new DateRange(DateTime.Now);


        private MGDataAccessLibrary.Models.Amenities.Requests.BookingsList filter = new MGDataAccessLibrary.Models.Amenities.Requests.BookingsList
        {
            PageSize = pageSize,
            Page = 1
        };

        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking> Bookings { get; private set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking>();
        public string PendingString { get; set; }


        public override void Init(object initData)
        {
            base.Init(initData);
            async void p(object sender, MGDataAccessLibrary.Models.Amenities.Responses.Booking e)
            {
                if (Bookings != null && Bookings.LastOrDefault() == e && CanGetMorePages)
                {
                    await LoadData(refreshData: false, FetchNextPage: true);
                }
            }
            ((AmenitiesListPage)this.CurrentPage).OnBookingAppeared += p;
        }
        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            var pmcInfo = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetPMCInfo();
            CompanyUrl = pmcInfo.Item2?.CompanyUrl;
            ShouldClearFilter = true;
        }
        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            try
            {
                var pmcInfo = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetPMCInfo();
                var url = pmcInfo.Item2?.CompanyUrl;
                if (FetchNextPage)
                    filter.Page++;
                var list = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetBookingList(filter);
                CanGetMorePages = list.List != null && list.List.Any();
                if (list.List is null && !list.List.Any() && !FetchNextPage)
                {
                    Bookings.Clear();
                }
                else
                {
                    var updatedIconList = list.List.Select(t => { t.Icon = url + t.Icon; return t; });
                    Bookings = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking>(updatedIconList);
                }
            ((AmenitiesListPage)CurrentPage).DataLoaded();
                PendingString = list.TotalPending > 0 ? $"({list.TotalPending} pending)" : string.Empty;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null || ex.InnerException.Message != "404")
                    await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "Dismiss");
            }

        }

        public FreshAwaitCommand OnCreateButtonTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    ShouldClearFilter = false;
                    await CoreMethods.PushPageModel<CreateBookingPageModel>();
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnViewCalendarTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    ShouldClearFilter = false;
                    await CoreMethods.PushPageModel<BookingCalendarPageModel>();
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnDoneCalFilterTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    currentPage = 1;
                    filter.From = SelectedDateRange.StartDate;
                    filter.To = SelectedDateRange.EndDate;
                    filter.Page = 1;
                    CalFilterIsvisible = false;
                    RaisePropertyChanged("SelectedDateRangeString");
                    await LoadData();
                    tcs?.SetResult(true);
                }, () => SelectedDateRange != null && SelectedDateRange.EndDate.HasValue);
            }
        }

        public FreshAwaitCommand OnResetCalFilterTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    currentPage = 1;
                    filter.From = null;
                    filter.To = null;
                    filter.Page = 1;
                    CalFilterIsvisible = false;
                    SelectedDateRange = new DateRange(startDate: DateTime.Now);
                    RaisePropertyChanged("SelectedDateRangeString");
                    await LoadData();
                    tcs?.SetResult(true);
                }, () => SelectedDateRange != null && SelectedDateRange.EndDate.HasValue);
            }
        }

        public FreshAwaitCommand OnHideCalFilterTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    CalFilterIsvisible = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnCalFilterButtonTapped => new FreshAwaitCommand((tcs) =>
        {
            CalFilterIsvisible = !CalFilterIsvisible;
            if (CalFilterIsvisible)
            {
                ((AmenitiesListPage)CurrentPage).SetCalContent();
            }
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnBookingTapped
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    try
                    {
                        if (IsBusy)
                            return;
                        IsBusy = true;
                        var booking = (MGDataAccessLibrary.Models.Amenities.Responses.Booking)par;
                        //need to get booking again because no info in this booking from list
                        var detailedbooking = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetBooking(booking.AmenityBookingId);
                        if (detailedbooking != null)
                        {
                            //need to put this together. It's not done from the backend.
                            detailedbooking.Icon = CompanyUrl + detailedbooking.Icon.Replace(".svg", ".png");
                            ShouldClearFilter = false;
                            await CoreMethods.PushPageModel<BookingDetailPageModel>(data: detailedbooking);
                            IsBusy = false;
                        }

                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "Dismiss");
                    }

                    tcs?.SetResult(true);
                });
            }
        }

        public bool CanGetMorePages { get; private set; }
        public bool IsBusy { get; private set; }
        public bool ShouldClearFilter { get; private set; }



        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (ShouldClearFilter)
            {
                filter = new MGDataAccessLibrary.Models.Amenities.Requests.BookingsList
                {
                    Page = 1,
                    PageSize = pageSize
                };
                SelectedDateRange = new DateRange(DateTime.Now);
                RaisePropertyChanged("SelectedDateRangeString");
            }
            CalFilterIsvisible = false;
        }
    }
}

