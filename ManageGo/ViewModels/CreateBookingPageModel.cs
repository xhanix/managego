using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using PropertyChanged;

namespace ManageGo
{
    internal class CreateBookingPageModel : BaseDetailPage
    {
        private MGDataAccessLibrary.Models.Amenities.Responses.PMCBuilding selectedBuilding;
        private MGDataAccessLibrary.Models.Amenities.Responses.BuildingUnit selectedUnit;
        private MGDataAccessLibrary.Models.Amenities.Responses.Amenity selectedAmenity { get; set; }
        private DateTime? selectedDate;
        private string totalAmount;

        public bool OtherTextFieldIsVisible { get; set; }
        public string OtherNameText { get; set; }
        public string NoteToTenant { get; set; }
        public bool ListViewIsVisible { get; set; }
        public string TotalAmount
        {
            get => totalAmount;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    totalAmount = "$0.00";
                else if (!value.StartsWith("$", StringComparison.InvariantCulture))
                    totalAmount = "$" + value;

                else
                    totalAmount = value;
            }
        }

        public double SecurityDeposit { get; set; }
        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Amenity> Amenities { get; set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Amenity>();
        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.PMCBuilding> Buildings { get; set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.PMCBuilding>();
        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.BuildingUnit> Units { get; set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.BuildingUnit>();
        public ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.UnitTenant> Tenants { get; set; } = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.UnitTenant>();

        [AlsoNotifyFor("SelectedDateString")]
        public DateTime? SelectedDate
        {
            get => selectedDate;
            set
            {
                selectedDate = value;
                //close the date picker after selection is made
                DatePickerIsVisible = false;
            }
        }
        public string SelectedDateString => SelectedDate.HasValue ? SelectedDate.Value.ToString("MMM d, yyyy") : "Select date";
        public bool DatePickerIsVisible { get; set; }

        [AlsoNotifyFor("SelectedAmenityName")]
        public MGDataAccessLibrary.Models.Amenities.Responses.Amenity SelectedAmenity
        {
            get => selectedAmenity;
            set
            {
                selectedAmenity = value;
                if (value != null)
                    OnAmenitySelected.Execute(value);
            }
        }
        [AlsoNotifyFor("SelectedBuildingName")]
        public MGDataAccessLibrary.Models.Amenities.Responses.PMCBuilding SelectedBuilding
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
        [AlsoNotifyFor("SelectedUnitName")]
        public MGDataAccessLibrary.Models.Amenities.Responses.BuildingUnit SelectedUnit
        {
            get => selectedUnit;
            set
            {
                selectedUnit = value;
                if (value?.Id == selectedUnit?.Id)
                    return;
                if (value != null)
                    OnUnitSelected.Execute(value);

            }
        }
        public string SelectedAmenityName => SelectedAmenity?.Name;
        public string SelectedBuildingName => SelectedBuilding?.BuildingDescription;
        public string SelectedUnitName => SelectedUnit?.Unit;

        public bool BuildingPickerIsVisible { get; set; }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            var pmcBuildings = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetPMCBuildings();
            if (pmcBuildings != null && pmcBuildings.Any(b => b.Amenities != null && b.Amenities.Any()))
                Buildings = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.PMCBuilding>(pmcBuildings.Where(t => t.Amenities != null && t.Amenities.Any()));
            SelectedBuilding = Buildings?.FirstOrDefault(t => t.Amenities != null && t.Amenities.Any());
        }
        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            return;
        }

        public FreshAwaitCommand OnBuildingButtonTapped => new FreshAwaitCommand((tcs) =>
        {
            BuildingPickerIsVisible = true;
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnAmenitySelected => new FreshAwaitCommand(async (par, tcs) =>
        {
            var amenity = (MGDataAccessLibrary.Models.Amenities.Responses.Amenity)par;
            var amenityWithDetails = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetAmenity(amenity.Id, SelectedBuilding.BuildingId);
            amenity.Rules = amenityWithDetails.Rules;
            SecurityDeposit = amenity.Rules.SecurityDepositAmount;
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnBuildingSelected
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var building = (MGDataAccessLibrary.Models.Amenities.Responses.PMCBuilding)par;
                    SelectedUnit = null;

                    int selectedAmenityId = default;
                    if (SelectedAmenity != null && SelectedBuilding.Amenities.Select(t => t.Id).Contains(SelectedAmenity.Id))
                        selectedAmenityId = SelectedBuilding.Amenities.First(t => t.Id == SelectedAmenity.Id).Id;
                    else
                        SelectedAmenity = null;
                    if (building.Amenities != null && building.Amenities.Any())
                        Amenities = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.Amenity>(building.Amenities);
                    if (SelectedBuilding is null)
                        return;
                    if (SelectedBuilding != null)
                    {
                        var units = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetBuildingUnits(SelectedBuilding.BuildingId);
                        if (units != null && units.Any())
                            Units = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.BuildingUnit>(units);
                    }

                    SelectedAmenity = Amenities?.FirstOrDefault(t => t.Id == selectedAmenityId);
                    foreach (var a in Buildings)
                    {
                        a.IsSelected = false;
                    }
                    building.IsSelected = !building.IsSelected;

                    BuildingPickerIsVisible = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnUnitSelected
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var unit = (MGDataAccessLibrary.Models.Amenities.Responses.BuildingUnit)par;

                    var tenants = await MGDataAccessLibrary.BussinessLogic.AmenitiesProcessor.GetUnitTenants(unit.Id);
                    Tenants = new ObservableCollection<MGDataAccessLibrary.Models.Amenities.Responses.UnitTenant>();
                    foreach (var tenant in tenants)
                        Tenants.Add(tenant);
                    Tenants.Add(new MGDataAccessLibrary.Models.Amenities.Responses.UnitTenant { FirstName = "Other", Id = default });
                    ListViewIsVisible = true;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnTenantTapped
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var tenant = (MGDataAccessLibrary.Models.Amenities.Responses.UnitTenant)par;
                    OtherTextFieldIsVisible = tenant.Id == default;
                    foreach (var _tenant in Tenants)
                        _tenant.IsSelected = false;
                    tenant.IsSelected = true;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnSelectDateTapped
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    DatePickerIsVisible = !DatePickerIsVisible;
                });
            }
        }

    }
}
