using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using MGDataAccessLibrary.Models.Amenities.Responses;
using PropertyChanged;

namespace ManageGo
{
    internal class BookingCalendarPageModel : BaseDetailPage
    {
        private Models.PMCBuilding selectedBuilding;

        [AlsoNotifyFor("SelectedDateText")]
        public DateTime SelectedDate { get; set; } = DateTime.Now;
        public ObservableCollection<Models.BuildingAmenity> Amenities { get; set; } = new ObservableCollection<Models.BuildingAmenity>();
        public List<Models.PMCBuilding> Buildings { get; set; } = new List<Models.PMCBuilding>();
        public string CompanyUrl { get; private set; }
        [AlsoNotifyFor("SelectedAmenityName", "SelectPromptIsVisible")]
        public Models.BuildingAmenity SelectedAmenity { get; set; }
        public bool SelectPromptIsVisible => SelectedAmenity is null;
        public List<TimeRanges> TimeRanges { get; private set; }
        [AlsoNotifyFor("SelectedBuildingName")]
        public Models.PMCBuilding SelectedBuilding
        {
            get => selectedBuilding;
            set
            {
                if (value != null && value.BuildingId == selectedBuilding?.BuildingId)
                    return;
                selectedBuilding = value;
                if (value != null)
                    OnBuildingSelected.Execute(value);

            }
        }
        public bool AmenityPickerIsVisible { get; set; }
        public bool BuildingPickerIsVisible { get; set; }
        public string SelectedDateText => SelectedDate.ToString("ddd, MMM dd, yyyy");
        public string SelectedAmenityName => SelectedAmenity?.Name ?? "Select amenity";
        public string SelectedBuildingName => SelectedBuilding?.BuildingDescription;


        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            /// 1- get all buildings and select first building
            /// 2- populate amenities but don't select any -> prompt user to select an amenity
            /// 3- amenity selected -> show calendar
            /// 4- building selected -> repopulate amenity** list and deselect amenity -> prompt user to select amenity
            ///

            base.ViewIsAppearing(sender, e);
            HamburgerIsVisible = true;
            try
            {
                var pmcInfo = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetPMCInfo();
                CompanyUrl = pmcInfo.Item2?.CompanyUrl;
                var pmcBuildings = pmcInfo.Item1.BuildingsAccess;
                Xamarin.Essentials.Preferences.Set("time_offset", pmcInfo.Item2.TimeZoneOffset);
                if (pmcBuildings != null && pmcBuildings.Any(b => b.Amenities != null && b.Amenities.Any()))
                    Buildings = pmcBuildings.Where(t => t.Amenities != null && t.Amenities.Any()).Select(t => new Models.PMCBuilding
                    {
                        Amenities = t.Amenities,
                        BuildingDescription = t.BuildingDescription,
                        BuildingId = t.BuildingId,
                        IsSelected = false

                    }).ToList();
                if (SelectedBuilding is null)
                {
                    SelectedBuilding = Buildings?.FirstOrDefault(t => t.Amenities != null && t.Amenities.Any());
                    SelectedBuilding.IsSelected = true;
                }

                if (SelectedAmenity is null)
                    SelectedAmenity = null;
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "Dismiss");
            }

        }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            if (SelectedBuilding is null || SelectedAmenity is null)
                return;
            try
            {
                var availableTimes = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAvailableDays(new MGDataAccessLibrary.Models.Amenities.Requests.AvailableDays
                {
                    BuildingId = SelectedBuilding.BuildingId,
                    //  UnitId = selectedUnit.Id,
                    From = SelectedDate.ToString("dd-MMM-yyyy"),
                    To = SelectedDate.ToString("dd-MMM-yyyy"),
                }, SelectedAmenity.Id);
                if (availableTimes.AvailableDaysAndTimes.Any())
                {
                    TimeRanges = availableTimes.AvailableDaysAndTimes?.FirstOrDefault()?.TimeRanges?.ToList();
                    if (CurrentPage != null)
                    {
                        ((BookingCalendarPage)CurrentPage).SetTimeDetails(TimeRanges);
                    }

                }
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("ManageGo", ex.Message, "Dismiss");
            }

        }

        internal async Task GetBookingForStartTime(int startMinutes)
        {
            var tappedTimeRange = TimeRanges.FirstOrDefault(t => t.From == startMinutes);

            if (tappedTimeRange is null || tappedTimeRange.BookedBy is null)
                return;

            try
            {
                var booking = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetBooking(tappedTimeRange.BookedBy.BookingId);
                if (booking != null)
                {
                    booking.Icon = CompanyUrl + booking.Icon;
                    await CoreMethods.PushPageModel<BookingDetailPageModel>(data: booking);
                }
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("ManageGo", ex.Message, "Dismiss");
            }

        }

        public FreshAwaitCommand OnGoNextDayTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    SelectedDate = SelectedDate.AddDays(1);
                    await LoadData();
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnAmenityButtonTapped => new FreshAwaitCommand(async (tcs) =>
               {
                   if (SelectedBuilding is null)
                   {
                       await CoreMethods.DisplayAlert("ManageGo", "Select a building first", "OK");
                       tcs?.SetResult(true);
                       return;
                   }
                   BuildingPickerIsVisible = false;
                   AmenityPickerIsVisible = !AmenityPickerIsVisible;
                   tcs?.SetResult(true);
               });


        public FreshAwaitCommand OnBuildingButtonTapped => new FreshAwaitCommand((tcs) =>
        {
            AmenityPickerIsVisible = false;
            BuildingPickerIsVisible = !BuildingPickerIsVisible;

            tcs?.SetResult(true);
        });


        public FreshAwaitCommand OnAmenitySelected
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var amenity = (Models.BuildingAmenity)par;
                    foreach (var a in Amenities)
                    {
                        a.IsSelected = false;
                    }
                    amenity.IsSelected = !amenity.IsSelected;
                    SelectedAmenity = amenity;
                    AmenityPickerIsVisible = false;
                    await LoadData();
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnBuildingSelected => new FreshAwaitCommand(async (par, tcs) =>
                {
                    if (!(par is Models.PMCBuilding building))
                        return;
                    int selectedAmenityId = default;
                    if (SelectedAmenity != null && SelectedBuilding.Amenities.Select(t => t.Id).Contains(SelectedAmenity.Id))
                        selectedAmenityId = SelectedBuilding.Amenities.First(t => t.Id == SelectedAmenity.Id).Id;
                    else
                        SelectedAmenity = null;
                    if (building.Amenities != null && building.Amenities.Any())
                        Amenities = new ObservableCollection<Models.BuildingAmenity>(building.Amenities.Select(t => new Models.BuildingAmenity
                        {
                            Id = t.Id,
                            Name = t.Name,
                            Rules = t.Rules,
                            Status = t.Status,
                            IsSelected = false
                        }));

                    SelectedAmenity = Amenities?.FirstOrDefault(t => t.Id == selectedAmenityId);
                    if (SelectedAmenity != null)
                        SelectedAmenity.IsSelected = true;
                    foreach (var a in Buildings)
                    {
                        a.IsSelected = false;
                    }
                    building.IsSelected = true;
                    SelectedBuilding = building;
                    BuildingPickerIsVisible = false;
                    if (SelectedAmenity != null)
                    {
                        await LoadData();
                    }
                    tcs?.SetResult(true);
                });



        public FreshAwaitCommand OnGoPreviousDayTapped => new FreshAwaitCommand(async (tcs) =>
                {
                    SelectedDate = SelectedDate.AddDays(-1);
                    await LoadData();
                    tcs?.SetResult(true);
                });

        public FreshAwaitCommand OnViewListTapped => new FreshAwaitCommand(async (tcs) =>
                {
                    SelectedBuilding = null;
                    SelectedAmenity = null;
                    await CoreMethods.PopPageModel();
                    tcs?.SetResult(true);
                });


    }
}

