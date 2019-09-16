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
using Microsoft.AppCenter.Analytics;

namespace ManageGo
{
    internal class TenantsPageModel : BaseDetailPage
    {
        public bool FilterSelectViewIsShown { get; set; }
        public bool FilterBuildingsExpanded { get; set; }
        public bool FilterUnitsExpanded { get; set; }
        public bool FilterStatusExpanded { get; set; }
        public string SelectedUnitString { get; set; }
        public bool CanFilter => Buildings.Any(t => t.IsSelected) || !string.IsNullOrWhiteSpace(FilterKeywords);
        [AlsoNotifyFor("CanFilter")]
        public string FilterKeywords { get; set; }
        public View PopContentView { get; private set; }
        public bool ListIsEnabled { get; set; }
        public bool HasPreExistingFilter { get; private set; }
        public List<Building> Buildings { get; set; }
        public List<Unit> Units { get; set; }
        public ObservableCollection<Tenant> FetchedTenants { get; set; }
        private int CurrentListPage { get; set; }
        private bool CanGetMorePages { get; set; }
        public Models.TenantRequestItem CurrentFilter { get; private set; }
        public Models.TenantRequestItem ParameterItem { get; set; }
        public string NumberOfAppliedFilters { get; internal set; } = " ";
        [AlsoNotifyFor("CanFilter")]
        public string SelectedBuildingsString { get; set; }
        public bool SelectedActiveTenantFilter { get; set; }
        public bool SelectedInActiveTenantFilter { get; set; }
        public bool BackbuttonIsVisible { get; private set; }
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
                    await CoreMethods.PopPageModel(modal: CurrentPage.Navigation.ModalStack.Contains(CurrentPage), animate: true);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            NothingFetched = false;
            try
            {
                var selectedBuildingAddress = Buildings?.Count(b => b.IsSelected) == 1 ? Buildings?.FirstOrDefault(b => b.IsSelected)?.BuildingShortAddress : null;
                if (FetchNextPage && ParameterItem != null)
                {
                    ParameterItem.Page++;
                    var nextPage = await DataAccess.GetTenantsAsync(ParameterItem);

                    if (nextPage != null)
                    {
                        foreach (var item in nextPage)
                        {
                            FetchedTenants.Add(item);
                        }
                    }
                    CanGetMorePages = nextPage != null && nextPage?.Count() == ParameterItem.PageSize;
                }
                else
                {
                    HasLoaded = false;
                    if (ParameterItem is null)
                        ParameterItem = new Models.TenantRequestItem();
                    if (refreshData)
                        ParameterItem.Page = 1;
                    List<Tenant> tenantsAsync = (await DataAccess.GetTenantsAsync(ParameterItem)).ToList();
                    if (tenantsAsync != null)
                        FetchedTenants = new ObservableCollection<Tenant>(tenantsAsync);
                    else
                        FetchedTenants = new ObservableCollection<Tenant>();

                    CanGetMorePages = FetchedTenants != null && FetchedTenants.Count == ParameterItem.PageSize;

                    if (Buildings != null && Buildings.Any(t => t.IsSelected) && Units is null)
                    {
                        var details = await DataAccess.GetBuildingDetails(Buildings.First(t => t.IsSelected).BuildingId);
                        SelectedBuildingsString = details?.BuildingShortAddress;
                        Units = details?.Units;
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
                ((TenantsPage)CurrentPage).DataLoaded();
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
                        if (!SelectedInActiveTenantFilter)
                            SelectedActiveTenantFilter = true;
                    }
                    else
                    {
                        SelectedActiveTenantFilter = !SelectedActiveTenantFilter;
                        if (!SelectedActiveTenantFilter)
                            SelectedInActiveTenantFilter = true;
                    }

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
                    if (!CanFilter)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Select a building or type in search bar", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
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
                return new FreshAwaitCommand(execute);
            }
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            App.MasterDetailNav.IsGestureEnabled = false;
            SelectedBuildingsString = "Select";
            SelectedUnitString = "Select";
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

        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (App.MasterDetailNav != null)
                App.MasterDetailNav.IsGestureEnabled = true;
            if (FilterSelectViewIsShown)
            {
                FilterSelectViewIsShown = false;
                PopContentView = null;
            }
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
            SelectedBuildingsString = "Select";
            ParameterItem = null;
            CurrentFilter = null;
            NumberOfAppliedFilters = " ";
        }

        public async Task OnItemAppeared(Tenant tenant)
        {
            if (FetchedTenants != null && FetchedTenants.Last() == tenant && CanGetMorePages)
            {
                await LoadData(false, true);
            }
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
                        Analytics.TrackEvent("Tapped on Tenant Phone Number");
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
                                await CoreMethods.DisplayAlert("ManageGo", "Select a building first", "Dismiss");
                            }
                            else if (Buildings is null || Buildings.Count(t => t.IsSelected) > 1)
                            {
                                await CoreMethods.DisplayAlert("ManageGo", "Select only one building to select a unit", "Dismiss");
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

                    building.IsSelected = !building.IsSelected;
                    var count = Buildings.Count(t => t.IsSelected);
                    if (count > 0)
                    {
                        SelectedBuildingsString = Buildings.First(t => t.IsSelected).BuildingShortAddress;
                        if (count > 1)
                        {
                            SelectedBuildingsString += $" + {count - 1} Buildings";
                        }
                    }
                    if (count > 1)
                    {
                        SelectedUnitString = "Select";
                        tcs?.SetResult(true);
                        return;
                    }
                    try
                    {
                        if (count == 1)
                        {
                            var details = await DataAccess.GetBuildingDetails(Buildings.First(t => t.IsSelected).BuildingId);
                            Units = details.Units;
                        }
                    }
                    catch (Exception ex)
                    {
                        Crashes.TrackError(ex);
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                        if (count == 0)
                        {
                            SelectedBuildingsString = "Select";
                            SelectedUnitString = "Select";
                        }
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

                    unit.IsSelected = !unit.IsSelected;
                    foreach (Unit u in Units)
                    {
                        if (u != unit)
                            u.IsSelected = false;
                    }
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
                    SelectedBuildingsString = "Select";
                    SelectedUnitString = "Select";
                    await LoadData(true, false);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }
        public bool NothingFetched { get; private set; }
    }
}
