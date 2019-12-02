using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using PropertyChanged;

namespace ManageGo
{
    internal class BookingCalendarPageModel : BaseDetailPage
    {
        [AlsoNotifyFor("SelectedDateText")]
        public DateTime SelectedDate { get; set; } = DateTime.Now;
        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Amenity> Amenities { get; set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Amenity>();
        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Building> Buildings { get; set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Building>();
        [AlsoNotifyFor("SelectedAmenityName")]
        public MGDataAccessLibrary.Models.Amenities.Responses.Amenity SelectedAmenity { get; set; }
        [AlsoNotifyFor("SelectedBuildingName")]
        public MGDataAccessLibrary.Models.Amenities.Responses.Building SelectedBuilding { get; set; }
        public bool AmenityPickerIsVisible { get; set; }
        public bool BuildingPickerIsVisible { get; set; }
        public string SelectedDateText => SelectedDate.ToString("ddd, MMM dd, yyyy");
        public string SelectedAmenityName => SelectedAmenity?.Name;
        public string SelectedBuildingName => SelectedBuilding?.BuildingDescription;

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            //get all ameneties

            var amenities = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAmenities();
            if (amenities != null && amenities.Any())
            {
                Amenities = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Amenity>(amenities);

            }

            if (SelectedAmenity is null)
                SelectedAmenity = Amenities.FirstOrDefault();


            var selectedAmenityId = SelectedAmenity?.Id;
            //get available days for first amenity
            var parameters = new MGDataAccessLibrary.Models.Amenities.Requests.AvailableDays
            {
                BuildingId = SelectedBuilding?.BuildingId ?? default,
                From = SelectedDate.ToString("yyyy-MM-dd"),
                To = SelectedDate.ToString("yyyy-MM-dd")
            };

            try
            {
                var availableDate = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAvailableDays(parameters, selectedAmenityId ?? default);
                ((BookingCalendarPage)CurrentPage).SetTimeDetails(availableDate?.AvailableDaysAndTimes?.FirstOrDefault().TimeRanges);
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

        public FreshAwaitCommand OnAmenityButtonTapped => new FreshAwaitCommand((tcs) =>
               {
                   BuildingPickerIsVisible = false;
                   AmenityPickerIsVisible = true;
                   tcs?.SetResult(true);
               });


        public FreshAwaitCommand OnBuildingButtonTapped => new FreshAwaitCommand((tcs) =>
        {
            AmenityPickerIsVisible = false;
            BuildingPickerIsVisible = true;

            tcs?.SetResult(true);
        });


        public FreshAwaitCommand OnAmenitySelected
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var amenity = (MGDataAccessLibrary.Models.Amenities.Responses.Amenity)par;
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

        public FreshAwaitCommand OnBuildingSelected
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var building = (MGDataAccessLibrary.Models.Amenities.Responses.Building)par;
                    foreach (var a in Buildings)
                    {
                        a.IsSelected = false;
                    }
                    building.IsSelected = !building.IsSelected;
                    SelectedBuilding = building;
                    BuildingPickerIsVisible = false;
                    await LoadData();
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnGoPreviousDayTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    SelectedDate = SelectedDate.AddDays(-1);
                    await LoadData();
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnViewListTapped
        {
            get
            {
                return new FreshAwaitCommand(async (tcs) =>
                {
                    await CoreMethods.PopPageModel();
                    tcs?.SetResult(true);
                });
            }
        }
    }
}

