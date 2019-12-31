using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;
using PropertyChanged;

namespace ManageGo
{
    internal class AmenitiesListPageModel : BaseDetailPage
    {
        private const int pageSize = 50;
        private int currentPage = 1;
        public string CompanyUrl { get; private set; }
        public bool CalFilterIsvisible { get; set; }
        public bool FilterDateExpanded { get; set; }
        public string url { get; private set; }
        public bool FilterBuildingsExpanded { get; private set; }
        public bool FilterAmenitiesExpanded { get; private set; }
        public bool FilterStatusesExpanded { get; private set; }
        public List<Models.PMCBuilding> Buildings { get; set; } = new List<Models.PMCBuilding>();
        public List<Models.BuildingAmenity> Amenities { get; set; } = new List<Models.BuildingAmenity>();
        public List<Models.AmenityStatus> Statuses { get; set; }
        public string SelectedStatusesString => Statuses is null || !Statuses.Any(t => t.IsSelected) ? "All" : (Statuses.Count(t => t.IsSelected) == 1 ? Statuses.First(t => t.IsSelected).Name : $"{Statuses.First(t => t.IsSelected).Name}, +{Statuses.Count(t => t.IsSelected) - 1} more");
        public string SelectedDateRangeString => SelectedDateRange is null || !SelectedDateRange.EndDate.HasValue ? "All dates" : SelectedDateRange.StartDate.ToString("MMM dd") + " - " + SelectedDateRange.EndDate?.ToString("MMM dd");
        [AlsoNotifyFor("SelectedDateRangeString")]
        public DateRange SelectedDateRange { get; set; }
        public bool CanGetMorePages { get; private set; }
        public bool IsBusy { get; private set; }
        public bool ShouldClearFilter { get; private set; }
        public bool NothingFound { get; private set; }
        public bool IsRefreshing { get; private set; }
        public string SelectedAmenitiesString => Amenities is null || !Amenities.Any(t => t.IsSelected) ? "All amenities" : (Amenities.Count(t => t.IsSelected) == 1 ? Amenities.First(t => t.IsSelected).Name : $"{Amenities.First(t => t.IsSelected).Name}, +{Amenities.Count(t => t.IsSelected) - 1} more");
        public string SelectedBuildingsString => Buildings is null || !Buildings.Any(t => t.IsSelected) ? "All buildings" : (Buildings.Count(t => t.IsSelected) == 1 ? Buildings.First(t => t.IsSelected).BuildingDescription : $"{Buildings.First(t => t.IsSelected).BuildingDescription }, +{Buildings.Count(t => t.IsSelected) - 1} more");


        private MGDataAccessLibrary.Models.Amenities.Requests.BookingsList filter = new MGDataAccessLibrary.Models.Amenities.Requests.BookingsList
        {
            PageSize = pageSize,
            Page = 1
        };

        public string FilterString { get; set; }

        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking> Bookings { get; private set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking>();
        public string PendingString { get; set; }


        public override void Init(object initData)
        {
            base.Init(initData);
            //Pending = 0, Approved = 1, Declined = 2, Canceled = 3
            Statuses = new List<Models.AmenityStatus>
            {
                new Models.AmenityStatus { Id = 0, Name = "Pending"},
                new Models.AmenityStatus { Id = 1, Name = "Approved"},
                new Models.AmenityStatus { Id = 2, Name = "Declined"},
                new Models.AmenityStatus { Id = 3, Name = "Canceled"}
            };
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
            FilterAmenitiesExpanded = false;
            FilterBuildingsExpanded = false;
            FilterDateExpanded = false;
            FilterStatusesExpanded = false;
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
                NothingFound = false;
                IsRefreshing = true;
                if (url is null)
                {
                    var pmcInfo = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetPMCInfo();
                    url = pmcInfo.Item2?.CompanyUrl;
                }
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
                if (ex.InnerException.Message == "404")
                {
                    if (filter.Page == 1)
                    {
                        CanGetMorePages = false;
                        Bookings = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Booking>();
                        NothingFound = true;
                    }
                    else
                    {
                        CanGetMorePages = false;
                        //go back 1 page if this page did not return any results
                        filter.Page--;
                    }

                }
                else
                    await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "Dismiss");

            }
            IsRefreshing = false;
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
                    CanGetMorePages = true;
                    CalFilterIsvisible = false;
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
            if (!(par is Models.PMCBuilding building))
                return;
            building.IsSelected = !building.IsSelected;
            if (Buildings.Any(t => t.IsSelected))
            {
                filter.RawBuildings = Buildings.Where(t => t.IsSelected).Select(t => t.BuildingId).ToList();
            }
            else
            {
                filter.RawBuildings = new List<int>();
            }
            RaisePropertyChanged("SelectedBuildingsString");
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnAmenitySelected => new FreshAwaitCommand(async (par, tcs) =>
        {
            if (!(par is Models.BuildingAmenity amenity))
                return;
            amenity.IsSelected = !amenity.IsSelected;
            if (Amenities.Any(t => t.IsSelected))
            {
                filter.RawAmenities = Amenities.Where(t => t.IsSelected).Select(t => t.Id).ToList();
            }
            else
            {
                filter.RawAmenities = new List<int>();
            }
            RaisePropertyChanged("SelectedAmenitiesString");

            tcs?.SetResult(true);
        });


        public FreshAwaitCommand OnStatusSelected => new FreshAwaitCommand(async (par, tcs) =>
        {
            if (!(par is Models.AmenityStatus status))
                return;
            status.IsSelected = !status.IsSelected;
            if (Statuses.Any(t => t.IsSelected))
            {
                filter.RawStatuses = Statuses.Where(t => t.IsSelected).Select(t => t.Id).ToList();
            }
            else
            {
                filter.RawStatuses = new List<int>();
            }
            RaisePropertyChanged("SelectedStatusesString");

            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnCalFilterButtonTapped => new FreshAwaitCommand((tcs) =>
        {
            CalFilterIsvisible = !CalFilterIsvisible;
            /*
            if (CalFilterIsvisible)
            {
                ((AmenitiesListPage)CurrentPage).SetCalContent();
            }*/
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnApplyFiltersTapped => new FreshAwaitCommand(async (tcs) =>
        {
            filter.RawAmenities = Amenities.Where(t => t.IsSelected).Select(t => t.Id).ToList();
            filter.RawBuildings = Buildings.Where(t => t.IsSelected).Select(t => t.BuildingId).ToList();
            filter.RawStatuses = Statuses.Where(t => t.IsSelected).Select(t => t.Id).ToList();
            filter.From = SelectedDateRange?.StartDate;
            filter.To = SelectedDateRange?.EndDate;
            filter.Search = FilterString;
            filter.Page = 1;
            currentPage = 1;
            CanGetMorePages = true;
            CalFilterIsvisible = false;
            await LoadData();
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnCloseFilterViewTapped => new FreshAwaitCommand((tcs) =>
        {
            CalFilterIsvisible = false;
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

        public FreshAwaitCommand OnResetFiltersTapped => new FreshAwaitCommand(async (tcs) =>
        {
            ResetFilter();
            CalFilterIsvisible = false;

            await LoadData();
            tcs?.SetResult(true);
        });


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
                            //  ((AmenitiesListPage)CurrentPage).setFilterCalContent();
                            FilterBuildingsExpanded = false;
                            FilterAmenitiesExpanded = false;
                            FilterStatusesExpanded = false;
                            break;
                        case "Buildings":
                            FilterBuildingsExpanded = !FilterBuildingsExpanded;
                            FilterDateExpanded = false;
                            FilterAmenitiesExpanded = false;
                            FilterStatusesExpanded = false;
                            break;
                        case "Amenities":
                            FilterAmenitiesExpanded = !FilterAmenitiesExpanded;
                            FilterDateExpanded = false;
                            FilterBuildingsExpanded = false;
                            FilterStatusesExpanded = false;
                            break;
                        case "Statuses":
                            FilterStatusesExpanded = !FilterStatusesExpanded;
                            FilterAmenitiesExpanded = false;
                            FilterDateExpanded = false;
                            FilterBuildingsExpanded = false;
                            break;
                        default:
                            break;
                    }
                });
            }
        }



        private void ResetFilter()
        {
            filter = new MGDataAccessLibrary.Models.Amenities.Requests.BookingsList
            {
                Page = 1,
                PageSize = pageSize
            };
            SelectedDateRange = new DateRange(DateTime.Today, null);
            CanGetMorePages = true;
            if (Buildings != null)
                Buildings = Buildings.Select(b => { b.IsSelected = false; return b; }).ToList();
            if (Amenities != null)
                Amenities = Amenities.Select(a => { a.IsSelected = false; return a; }).ToList();
            if (Statuses != null)
                Statuses = Statuses.Select(a => { a.IsSelected = false; return a; }).ToList();
            FilterString = null;
            FilterAmenitiesExpanded = false;
            FilterBuildingsExpanded = false;
            FilterDateExpanded = false;
            FilterStatusesExpanded = false;
            RaisePropertyChanged("SelectedAmenitiesString");
            RaisePropertyChanged("SelectedBuildingsString");
            RaisePropertyChanged("SelectedStatusesString");
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (ShouldClearFilter)
            {
                ResetFilter();
            }
            CalFilterIsvisible = false;
        }
    }
}

