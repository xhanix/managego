using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using ManageGo.Services;
using System.Linq;
using FreshMvvm;
using Xamarin.Essentials;
using PropertyChanged;
using System.Collections.ObjectModel;
using Microsoft.AppCenter.Crashes;

namespace ManageGo
{
    internal class TenantsPageModel : BaseDetailPage
    {
        public bool FilterSelectViewIsShown { get; set; }
        public bool FilterBuildingsExpanded { get; set; }
        public bool FilterUnitsExpanded { get; set; }
        public bool FilterStatusExpanded { get; set; }
        public string SelectedUnitString { get; set; }
        public bool FilterButtonIsEnabled
        {
            get
            {
                var selectedBuilding = Buildings.Any(t => t.IsSelected);
                var statusSelected = SelectedActiveTenantFilter != SelectedInActiveTenantFilter;
                var hasKeywords = !string.IsNullOrWhiteSpace(FilterKeywords);
                return selectedBuilding || statusSelected || hasKeywords;
            }
        }
        [AlsoNotifyFor("FilterButtonIsEnabled")]
        public string FilterKeywords { get; set; }
        public View PopContentView { get; private set; }
        public bool ListIsEnabled { get; set; }
        public bool HasPreExistingFilter { get; private set; }
        public List<Building> Buildings { get; set; }
        public List<Unit> Units { get; set; }
        public ObservableCollection<Tenant> FetchedTenants { get; set; }
        private int LastLoadedItemId { get; set; }
        private int CurrentListPage { get; set; }
        private bool CanGetMorePages { get; set; }
        public Models.TenantRequestItem CurrentFilter { get; private set; }
        public Models.TenantRequestItem ParameterItem { get; set; }
        public string NumberOfAppliedFilters { get; internal set; } = " ";
        [AlsoNotifyFor("FilterButtonIsEnabled")]
        public string SelectedBuildingsString { get; set; }
        [AlsoNotifyFor("FilterButtonIsEnabled")]
        public bool SelectedActiveTenantFilter { get; set; }
        [AlsoNotifyFor("FilterButtonIsEnabled")]
        public bool SelectedInActiveTenantFilter { get; set; }
        public bool BackbuttonIsVisible { get; private set; }
        public string ActiveTenantCheckBoxImage
        {
            get
            {
                return !SelectedActiveTenantFilter ? "unchecked.png" : "checked.png";
            }
        }
        public string InActiveTenantCheckBoxImage
        {

            get
            {
                return !SelectedInActiveTenantFilter ? "unchecked.png" : "checked.png";
            }
        }

        public string SelectedStatusFlagsString
        {
            get
            {
                if (SelectedActiveTenantFilter && SelectedInActiveTenantFilter || !SelectedActiveTenantFilter && !SelectedInActiveTenantFilter)
                    return "All";
                return !SelectedActiveTenantFilter ? "Inactive" : "Active";
            }
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            SelectedActiveTenantFilter = true;
            Buildings = App.Buildings;
            if (initData is Building building)
            {
                HasPreExistingFilter = true;

                foreach (Building b in Buildings.Where(t => t.BuildingId == building.BuildingId))
                {
                    b.IsSelected = true;
                }
                ParameterItem = new Models.TenantRequestItem
                {
                    Buildings = new List<int> { building.BuildingId }
                };
                BackbuttonIsVisible = true;
            }
            else if (initData is int buildingId && buildingId > 0)
            {
                HasPreExistingFilter = true;
                foreach (Building b in Buildings.Where(t => t.BuildingId == buildingId))
                {
                    b.IsSelected = true;
                }
                ParameterItem = new Models.TenantRequestItem
                {
                    Buildings = new List<int> { buildingId }
                };
                BackbuttonIsVisible = true;
            }
        }

        public FreshAwaitCommand OnBackbuttonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    await CoreMethods.PopPageModel(modal: CurrentPage.Navigation.ModalStack.Contains(CurrentPage), animate: false);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            NothingFetched = false;
            var selectedBuildingAddress = Buildings.Count(b => b.IsSelected) == 1 ? Buildings.First(b => b.IsSelected).BuildingName : null;
            try
            {
                if (FetchNextPage && ParameterItem != null)
                {
                    ParameterItem.Page++;
                    var nextPage = await DataAccess.GetTenantsAsync(ParameterItem);

                    if (nextPage != null)
                    {
                        foreach (var item in nextPage)
                        {
                            if (!string.IsNullOrWhiteSpace(selectedBuildingAddress))
                                item.FirstUnitAddress = selectedBuildingAddress;
                            FetchedTenants.Add(item);
                        }
                    }
                    CanGetMorePages = nextPage != null && nextPage.Count == ParameterItem.PageSize;
                    var lastIdx = FetchedTenants.IndexOf(FetchedTenants.Last());
                    var index = Math.Floor(lastIdx / 2d);
                    var markedItem = FetchedTenants.ElementAt((int)index);
                    LastLoadedItemId = markedItem.TenantID;
                }
                else
                {
                    HasLoaded = false;
                    if (ParameterItem is null)
                    {
                        ParameterItem = new Models.TenantRequestItem();
                    }
                    if (refreshData)
                        ParameterItem.Page = 1;
                    List<Tenant> tenantsAsync = await DataAccess.GetTenantsAsync(ParameterItem);
                    if (tenantsAsync != null)
                        FetchedTenants = new ObservableCollection<Tenant>(tenantsAsync);
                    else
                        FetchedTenants = new ObservableCollection<Tenant>();
                    if (FetchedTenants != null && FetchedTenants.Any() && !string.IsNullOrWhiteSpace(selectedBuildingAddress))
                    {
                        foreach (var t in FetchedTenants)
                        {
                            t.FirstUnitAddress = selectedBuildingAddress;
                        }
                    }

                    CanGetMorePages = FetchedTenants != null && FetchedTenants.Count == ParameterItem.PageSize;
                    if (FetchedTenants.Any() && CanGetMorePages)
                    {
                        var lastIdx = FetchedTenants.IndexOf(FetchedTenants.Last());
                        var index = Math.Floor(lastIdx / 2d);
                        var markedItem = FetchedTenants.ElementAt((int)index);
                        LastLoadedItemId = markedItem.TenantID;
                    }
                    if (Buildings.Any(t => t.IsSelected) && Units is null)
                    {
                        var details = await DataAccess.GetBuildingDetails(Buildings.First(t => t.IsSelected).BuildingId);
                        SelectedBuildingsString = details.BuildingName;
                        Units = details.Units;
                    }
                }
                Buildings = App.Buildings;
                ListIsEnabled = true;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                APIhasFailed = true;
                FetchedTenants = null;
                await CoreMethods.DisplayAlert("Something went wrong", $"Unable to get tickets. Message: {ex.Message}", "Try again", "Dismiss");
            }
            finally
            {
                NothingFetched = !FetchedTenants.Any();
                HasLoaded = true;
            }
        }

        public FreshAwaitCommand OnCloseFliterViewTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    PopContentView = (View)null;
                    FilterSelectViewIsShown = false;
                    ListIsEnabled = true;
                    Units = null;
                    FilterUnitsExpanded = false;
                    SelectedUnitString = "Select";
                    if (CurrentFilter != null)
                    {
                        if (Buildings != null && CurrentFilter.Buildings != null)
                        {
                            foreach (var b in Buildings)
                            {
                                if (CurrentFilter.Buildings.Contains(b.BuildingId))
                                    b.IsSelected = true;
                                else
                                    b.IsSelected = false;
                            }
                            if (Buildings.Count(b => b.IsSelected) == 1)
                            {
                                var details = await DataAccess.GetBuildingDetails(Buildings.First(b => b.IsSelected).BuildingId);
                                Units = details.Units;
                            }
                        }
                        if (Units != null && CurrentFilter.Units != null)
                        {
                            foreach (var u in Units)
                            {
                                if (CurrentFilter.Units.Contains(u.UnitId))
                                    u.IsSelected = true;
                                else
                                    u.IsSelected = false;
                            }
                        }
                        switch (CurrentFilter.Status)
                        {
                            case TenantStatus.Active:
                                SelectedActiveTenantFilter = true;
                                SelectedInActiveTenantFilter = false;
                                break;
                            case TenantStatus.Inactive:
                                SelectedActiveTenantFilter = false;
                                SelectedInActiveTenantFilter = true;
                                break;
                            case TenantStatus.All:
                                SelectedActiveTenantFilter = true;
                                SelectedInActiveTenantFilter = true;
                                break;
                        }

                        FilterKeywords = CurrentFilter.Search;
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand SetFilterStatus
        {
            get
            {
                return new FreshAwaitCommand(((parameter, tcs) =>
                {
                    string str = (string)parameter;
                    if (!(str == "Active"))
                    {
                        if (str == "Inactive")
                            SelectedInActiveTenantFilter = !SelectedInActiveTenantFilter;
                    }
                    else
                        SelectedActiveTenantFilter = !SelectedActiveTenantFilter;
                    tcs?.SetResult(true);
                }));
            }
        }
        public FreshAwaitCommand OnApplyFiltersTapped
        {
            get
            {
                async void execute(object parameter, TaskCompletionSource<bool> tcs)
                {
                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    ParameterItem = new Models.TenantRequestItem();
                    if (!string.IsNullOrWhiteSpace(FilterKeywords))
                        ParameterItem.Search = FilterKeywords;
                    else
                    {
                        if (Buildings != null && Buildings.Any(f => f.IsSelected))
                            ParameterItem.Buildings = Buildings.Where(f => f.IsSelected).Select(f => f.BuildingId).ToList();
                        if (Units != null && Units.Any(f => f.IsSelected))
                        {
                            ParameterItem.Units = Units.Where(f => f.IsSelected).Select(f => f.UnitId).ToList();
                        }
                        if (SelectedActiveTenantFilter && SelectedInActiveTenantFilter)
                            ParameterItem.Status = TenantStatus.All;
                        else if (SelectedActiveTenantFilter)
                            ParameterItem.Status = TenantStatus.Active;
                        else if (SelectedInActiveTenantFilter)
                            ParameterItem.Status = TenantStatus.Inactive;
                        else
                            ParameterItem.Status = TenantStatus.All;
                    }

                    NumberOfAppliedFilters = ParameterItem.NumberOfAppliedFilters > 0 ? ParameterItem.NumberOfAppliedFilters.ToString() : string.Empty;
                    await LoadData(refreshData: true);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute, (arg) => FilterButtonIsEnabled);
            }
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (Buildings != null)
            {
                foreach (Building building in Buildings.Where(t => t.IsSelected))
                {
                    building.IsSelected = false;
                }
            }
            Units = null;
            FilterUnitsExpanded = false;
            SelectedUnitString = "Select";
            ParameterItem = null;
            CurrentFilter = null;
            NumberOfAppliedFilters = " ";
        }

        public async Task OnItemAppeared(Tenant tenant)
        {
            if (tenant.TenantID != LastLoadedItemId)
                return;
            CurrentListPage++;
            await LoadData(true, false);
        }

        public FreshAwaitCommand OnFilterTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    if (FilterSelectViewIsShown)
                    {
                        PopContentView = (View)null;
                        ListIsEnabled = true;
                    }
                    else
                    {
                        CurrentFilter = ParameterItem.Clone();
                        PopContentView = new TenantFilterSelectView(this).Content;
                        ListIsEnabled = false;
                    }
                    FilterSelectViewIsShown = !FilterSelectViewIsShown;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnContactTenantTapped
        {
            get
            {
                void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var contactMethod = (string)par;
                    if (contactMethod.Contains("@"))
                    {
                        Device.OpenUri(new Uri($"mailto:{contactMethod}"));
                    }
                    else
                    {
                        Device.OpenUri(new Uri($"tel:{contactMethod}"));
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnShowTenantDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var tenant = (Tenant)par;
                    var alreadyExpandedTenant = FetchedTenants.FirstOrDefault(t => t.DetailsShown && t.TenantID != tenant.TenantID);
                    if (alreadyExpandedTenant != null)
                        alreadyExpandedTenant.DetailsShown = false;
                    tenant.DetailsShown = !tenant.DetailsShown;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnExpandFilterTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    switch ((string)par)
                    {
                        case "Status":
                            FilterStatusExpanded = !FilterStatusExpanded;
                            FilterBuildingsExpanded = false;
                            FilterUnitsExpanded = false;
                            break;
                        case "Buildings":
                            FilterBuildingsExpanded = !FilterBuildingsExpanded;
                            FilterStatusExpanded = false;
                            FilterUnitsExpanded = false;
                            break;
                        case "Units":
                            if (Buildings is null || !Buildings.Any(t => t.IsSelected))
                            {
                                await CoreMethods.DisplayAlert("ManageGo", "Select a building first", "DIMISS");
                            }
                            else
                            {
                                FilterUnitsExpanded = !FilterUnitsExpanded;
                            }
                            FilterBuildingsExpanded = false;
                            FilterStatusExpanded = false;
                            break;
                        default:
                            break;
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnBuildingTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var building = (Building)par;
                    FilterUnitsExpanded = false;
                    Units = null;
                    SelectedUnitString = "Select";
                    try
                    {
                        var details = await DataAccess.GetBuildingDetails(building.BuildingId);
                        foreach (Building b in Buildings.Where(t => t.IsSelected && t.BuildingId != building.BuildingId))
                        {
                            b.IsSelected = false;
                        }
                        building.IsSelected = !building.IsSelected;
                        if (building.IsSelected)
                            SelectedBuildingsString = building.BuildingName;
                        else
                            SelectedBuildingsString = string.Empty;
                        Units = details.Units;
                    }
                    catch (Exception ex)
                    {
                        Crashes.TrackError(ex);
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }
                }
                return new FreshAwaitCommand(execute);
            }

        }

        public FreshAwaitCommand OnUnitTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var unit = (Unit)par;
                    foreach (Unit u in Units)
                    {
                        u.IsSelected = false;
                    }
                    unit.IsSelected = true;
                    SelectedUnitString = unit.UnitName;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnResetFiltersButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    CurrentFilter = null;
                    ParameterItem = null;
                    await LoadData(true, false);
                    NumberOfAppliedFilters = string.Empty;
                    if (Buildings != null)
                    {
                        foreach (var b in Buildings)
                        {
                            b.IsSelected = false;
                        }
                    }
                    if (Units != null)
                    {
                        foreach (var b in Units)
                        {
                            b.IsSelected = false;
                        }
                    }
                    SelectedActiveTenantFilter = true;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }
        public bool NothingFetched { get; private set; }
    }
}
