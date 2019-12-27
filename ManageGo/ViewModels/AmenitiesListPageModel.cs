using System;
using System.Collections.Generic;
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
        public bool FilterDateExpanded { get; set; }
        public bool FilterBuildingsExpanded { get; private set; }
        public bool FilterAmenitiesExpanded { get; private set; }
        public List<Models.PMCBuilding> Buildings { get; set; } = new List<Models.PMCBuilding>();
        public List<Models.BuildingAmenity> Amenities { get; set; } = new List<Models.BuildingAmenity>();
        public string SelectedDateRangeString => !filter.From.HasValue || !filter.To.HasValue ? "Select Dates" : filter.From?.ToString("MMM dd") + " - " + filter.To?.ToString("MMM dd");
        public DateRange SelectedDateRange { get; set; } = new DateRange(DateTime.Now);
        public bool CanGetMorePages { get; private set; }
        public bool IsBusy { get; private set; }
        public bool ShouldClearFilter { get; private set; }
        public bool IsRefreshing { get; private set; }


        private MGDataAccessLibrary.Models.Amenities.Requests.BookingsList filter = new MGDataAccessLibrary.Models.Amenities.Requests.BookingsList
        {
            PageSize = pageSize,
            Page = 1
        };

        public string FilterString
        {
            get => filter.Search;
            set => filter.Search = value;
        }

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
            var pmcBuildings = pmcInfo.Item1.BuildingsAccess;
            if (pmcBuildings != null && pmcBuildings.Any(b => b.Amenities != null && b.Amenities.Any()))
            {
                Buildings = pmcBuildings.Where(t => t.Amenities != null && t.Amenities.Any()).Select(t => new Models.PMCBuilding
                {
                    Amenities = t.Amenities,
                    BuildingDescription = t.BuildingDescription,
                    BuildingId = t.BuildingId,
                    IsSelected = false

                }).ToList();
                var a = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAmenities();
                Amenities = a.Select(t => new Models.BuildingAmenity
                {
                    Id = t.Id,
                    Name = t.Name,
                    Rules = t.Rules,
                    Status = t.Status
                }).ToList();
            }


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
                else if (refreshData)
                {
                    filter.Page = 1;
                }
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
                if (filter.RawStatuses.Count == 1 && filter.RawStatuses.Contains(0))
                    PendingString = "(View all)";
                else
                    PendingString = list.TotalPending > 0 ? $"({list.TotalPending} pending)" : string.Empty;
            }
            catch (Exception ex)
            {
                if (ex.InnerException is null || ex.InnerException.Message != "404")
                    await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "Dismiss");
            }

        }

        public override void ReverseInit(object returnedData)
        {
            base.ReverseInit(returnedData);
            RefreshList.Execute(null);
        }

        public FreshAwaitCommand RefreshList
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    IsRefreshing = true;
                    await LoadData(refreshData: true);
                    IsRefreshing = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnFilterPendingTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    if (PendingString == "(View all)")
                        filter.RawStatuses.Clear();
                    else
                    {
                        filter.RawStatuses.Clear();
                        filter.RawStatuses.Add(0);
                        PendingString = "(View all)";
                    }
                    RefreshList.Execute(null);
                    tcs?.SetResult(true);
                });
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

        public FreshAwaitCommand OnBuildingSelected => new FreshAwaitCommand(async (par, tcs) =>
        {
            var building = par as Models.PMCBuilding;
            if (building is null)
                return;
            building.IsSelected = !building.IsSelected;
            if (Buildings.Any(t => t.IsSelected))
            {
                filter.RawBuildings = Buildings.Where(t => t.IsSelected).Select(t => t.BuildingId).ToList();
            }

            tcs?.SetResult(true);
        });

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


        public FreshAwaitCommand OnExpandFilterTapped
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var par = (string)parameter;
                    switch (par)
                    {
                        case "Date":
                            FilterDateExpanded = !FilterDateExpanded;
                            ((AmenitiesListPage)CurrentPage).setFilterCalContent();
                            FilterBuildingsExpanded = false;
                            FilterAmenitiesExpanded = false;
                            break;
                        case "Buildings":
                            FilterBuildingsExpanded = !FilterBuildingsExpanded;
                            FilterDateExpanded = false;
                            FilterAmenitiesExpanded = false;
                            break;
                        case "Amenities":
                            FilterAmenitiesExpanded = !FilterAmenitiesExpanded;
                            FilterDateExpanded = false;
                            FilterBuildingsExpanded = false;
                            break;
                        default:
                            break;
                    }
                });
            }
        }


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

