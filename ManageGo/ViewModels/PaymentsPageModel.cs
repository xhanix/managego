using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;
using ManageGo.Models;
using ManageGo.Services;
using Microsoft.AppCenter.Crashes;
using PropertyChanged;
using Xamarin.Forms;

namespace ManageGo
{
    //todo add next page loading
    internal class PaymentsPageModel : BaseDetailPage
    {
        public View RangePickerView { get; set; }
        int CurrentListPage { get; set; } = 1;
        DateRange dateRange;
        public bool CanGetMorePages { get; private set; }
        public int LastLoadedItemId { get; private set; }
        public bool IsRefreshingList { get; set; }
        [AlsoNotifyFor("NumberOfAppliedFilters")]
        public PaymentsRequestItem CurrentFilter { get; private set; }
        public string NumberOfAppliedFilters => CurrentFilter is null || CurrentFilter.NumberOfAppliedFilters == 0 ? " " : $"{CurrentFilter.NumberOfAppliedFilters}";

        PaymentsRequestItem ParameterItem { get; set; }
        public bool FilterSelectViewIsShown { get; set; }
        public bool RangeSelectorIsShown { get; private set; }
        ObservableCollection<Payment> fetchedPayments;
        public ObservableCollection<Payment> FetchedPayments
        {
            get { return fetchedPayments; }
            set
            {
                fetchedPayments = value;
                if (fetchedPayments is null || !fetchedPayments.Any())
                    NothingFetched = true;
                else
                    NothingFetched = false;
            }
        }
        public List<Building> Buildings { get; private set; }
        public List<Tenant> Tenants { get; private set; }
        public List<Unit> Units { get; private set; }
        public string SelectedBuildingsString { get; set; }
        public string SelectedUnitString { get; set; }
        public string SelectedTenantString { get; set; }
        public string FilterAmountString
        {
            get
            {
                if (FilteredAmountRange is null ||
                    (FilteredAmountRange.Item1 == 0 && FilteredAmountRange.Item2 == 5000))
                    return "All";

                var minVal = FilteredAmountRange.Item1 ?? 0;
                var maxVal = FilteredAmountRange.Item2 ?? 5000;
                return minVal.ToString("C0") + " - " +
                   (maxVal == 5000 ? maxVal.ToString("C0") + "+" : FilteredAmountRange.Item2?.ToString("C0"));
            }
        }
        public string FilterKeywords { get; set; }
        public bool FilterBuildingsExpanded { get; set; }
        public bool FilterUnitsExpanded { get; set; }
        public bool FilterTenantsExpanded { get; set; }
        public bool FilterStatusExpanded { get; set; }
        public bool FilterDueDateExpanded { get; private set; }
        [AlsoNotifyFor("SentPaymentsCheckBoxImage", "SelectedStatusFlagsString")]
        public bool SelectedSentPaymentsFilter { get; private set; }
        [AlsoNotifyFor("ReceivedPaymentsCheckBoxImage", "SelectedStatusFlagsString")]
        public bool SelectedReceivedPaymentsFilter { get; private set; }
        [AlsoNotifyFor("ReversedPaymentsCheckBoxImage", "SelectedStatusFlagsString")]
        public bool SelectedReversedPaymentsFilter { get; private set; }
        [AlsoNotifyFor("RefundedPaymentsCheckBoxImage", "SelectedStatusFlagsString")]
        public bool SelectedRefundedPaymentsFilter { get; private set; }

        public Tuple<int?, int?> SelectedAmountRange { get; set; } = new Tuple<int?, int?>(0, 5000);

        [AlsoNotifyFor("FilterAmountString")]
        private Tuple<int?, int?> FilteredAmountRange { get; set; }


        [AlsoNotifyFor("CalendarButtonText")]
        public DateRange FilterDueDate
        {
            get
            {
                return dateRange is null ? new DateRange(DateTime.Today, DateTime.Today.AddDays(-30)) : dateRange;
            }
            set
            {
                dateRange = value;
            }
        }

        [AlsoNotifyFor("SelectedDateRangeString")]
        public DateRange SelectedDateRange { get; set; }

        public string SelectedDateRangeString
        {
            get
            {
                return SelectedDateRange != null ?
                    SelectedDateRange.EndDate.HasValue ? SelectedDateRange.StartDate.ToShortDateString() + " - " + SelectedDateRange.EndDate.Value.ToShortDateString()
                                     :
                    SelectedDateRange.StartDate.ToShortDateString() : "All";
            }
        }

        public string SelectedStatusFlagsString
        {
            get
            {
                List<string> strings = new List<string>();
                if (SelectedSentPaymentsFilter)
                    strings.Add("Submitted");
                if (SelectedReceivedPaymentsFilter)
                    strings.Add("Received");
                if (SelectedReversedPaymentsFilter)
                    strings.Add("Reversed");
                if (SelectedRefundedPaymentsFilter)
                    strings.Add("Refunded");
                return string.Join(", ", strings);
            }
        }
        public string RangeSelectorMin => SelectedAmountRange != null ? SelectedAmountRange.Item1?.ToString("C0") : "";
        public string RangeSelectorMax
        {
            get
            {
                if (SelectedAmountRange is null)
                    return string.Empty;
                else if (SelectedAmountRange.Item2 >= 5000)
                    return "$5,000+";
                return SelectedAmountRange.Item2?.ToString("C0");
            }
        }

        public bool BackbuttonIsVisible { get; set; }
        public View PopContentView { get; private set; }

        public string CalendarButtonText
        {
            get
            {
                if (FilterDueDate.EndDate.HasValue)
                {
                    if (FilterDueDate.StartDate == FilterDueDate.EndDate)
                        return FilterDueDate.StartDate.ToString("MMM-dd");
                    return FilterDueDate.StartDate.ToString("MMM dd") + " - " + FilterDueDate.EndDate.Value.ToString("MMM dd");
                }
                return FilterDueDate.StartDate.ToString("MMM-dd");
            }
        }


        public string SentPaymentsCheckBoxImage
        {
            get
            {
                return SelectedSentPaymentsFilter ? "checked.png" : "unchecked.png";
            }
        }
        public string ReceivedPaymentsCheckBoxImage
        {
            get
            {
                return SelectedReceivedPaymentsFilter ? "checked.png" : "unchecked.png";
            }
        }
        public string ReversedPaymentsCheckBoxImage
        {
            get
            {
                return SelectedReversedPaymentsFilter ? "checked.png" : "unchecked.png";
            }
        }

        public string RefundedPaymentsCheckBoxImage
        {
            get
            {
                return SelectedRefundedPaymentsFilter ? "checked.png" : "unchecked.png";
            }
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            Buildings = App.Buildings;
            SelectedBuildingsString = "Select";
            SelectedUnitString = "Select";
            SelectedTenantString = "Select";
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
            if (Tenants != null)
            {
                foreach (var b in Tenants)
                {
                    b.IsSelected = false;
                }
            }
        }

        public bool NothingFetched { get; private set; }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            NothingFetched = false;
            try
            {
                if (refreshData && ParameterItem != null)
                {
                    HasLoaded = false;
                    ParameterItem.Page = 1;
                    ParameterItem.DateTo = FilterDueDate.EndDate ?? FilterDueDate.StartDate.AddDays(1);
                    var fetchedTickets = await DataAccess.GetPaymentsAsync(ParameterItem);
                    if (fetchedTickets != null)
                        FetchedPayments = new ObservableCollection<Payment>(fetchedTickets);
                    else
                        FetchedPayments = new ObservableCollection<Payment>();
                    CanGetMorePages = FetchedPayments != null && FetchedPayments.Count == ParameterItem.PageSize;


                }
                else if (FetchNextPage && ParameterItem != null)
                {
                    ParameterItem.Page++;
                    var nextPage = await Services.DataAccess.GetPaymentsAsync(ParameterItem);
                    CanGetMorePages = nextPage != null && nextPage.Count() == ParameterItem.PageSize;
                    foreach (var item in nextPage)
                    {
                        FetchedPayments.Add(item);
                    }
                }
                else
                {
                    HasLoaded = false;
                    ParameterItem = new PaymentsRequestItem
                    {
                        DateFrom = FilterDueDate.StartDate
                    };
                    ParameterItem.DateTo = FilterDueDate.EndDate;
                    var fetchedTickets = await DataAccess.GetPaymentsAsync(ParameterItem);
                    if (fetchedTickets != null)
                    {
                        FetchedPayments = new ObservableCollection<Payment>(fetchedTickets);
                    }
                    else
                        FetchedPayments = new ObservableCollection<Payment>();
                    foreach (var payment in fetchedPayments)
                    {
                        Console.WriteLine(payment.TransactionDate);
                    }

                    CanGetMorePages = FetchedPayments != null && FetchedPayments.Count == ParameterItem.PageSize;

                }
                if (CanGetMorePages)
                {
                    var lastIdx = FetchedPayments.IndexOf(FetchedPayments.Last());
                    var index = Math.Floor(lastIdx / 2d);
                    var markedItem = FetchedPayments.ElementAt((int)index);
                    LastLoadedItemId = markedItem.PaymentId;
                }
                ((PaymentsPage)CurrentPage).DataLoaded();
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                APIhasFailed = true;
                FetchedPayments = null;
                await CoreMethods.DisplayAlert("Something went wrong", $"Unable to get payment records. Message from server: {ex.Message}", "Try again", "Dismiss");
            }
            finally
            {
                HasLoaded = true;
                if (FetchedPayments is null || !FetchedPayments.Any())
                {
                    NothingFetched = true;
                }
            }
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (FilterSelectViewIsShown)
            {
                FilterSelectViewIsShown = false;
                PopContentView = null;
                if (App.MasterDetailNav != null)
                    App.MasterDetailNav.IsGestureEnabled = true;
            }
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            if (initData is bool _isModal)
            {
                IsModal = _isModal;
                HamburgerIsVisible = !IsModal;
            }
            else
                HamburgerIsVisible = true;
            async void p(object sender, Payment e)
            {
                var id = e.PaymentId;
                if (id == LastLoadedItemId && CanGetMorePages)
                {
                    LastLoadedItemId = 0;
                    await LoadData(refreshData: false, FetchNextPage: true);
                }
            }
            ((PaymentsPage)this.CurrentPage).OnPaymentAppeared += p;
        }

        public FreshAwaitCommand OnPulledToRefresh
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    IsRefreshingList = true;
                    await LoadData(true, false);
                    IsRefreshingList = false;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnShowDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var payment = (Payment)par;
                    var alreadyExpandedItem = FetchedPayments?.FirstOrDefault(t => t.DetailsShown && t.PaymentId != payment.PaymentId);
                    if (alreadyExpandedItem != null)
                        alreadyExpandedItem.DetailsShown = false;
                    payment.DetailsShown = !payment.DetailsShown;
                    tcs?.SetResult(true);
                });
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
                        App.MasterDetailNav.IsGestureEnabled = true;
                        //ListIsEnabled = true;
                    }
                    else
                    {
                        App.MasterDetailNav.IsGestureEnabled = false;
                        SelectedDateRange = new DateRange(FilterDueDate.StartDate, FilterDueDate.EndDate);
                        PopContentView = new Views.PaymentFilterView(this).Content;
                        if (FetchedPayments != null && FetchedPayments.Any())
                            ((PaymentsPage)CurrentPage).ScrollToFirst(FetchedPayments.First());
                        // ListIsEnabled = false;
                    }
                    FilterSelectViewIsShown = !FilterSelectViewIsShown;
                    //copy existing filters, user can cancel new filter changes (undo)
                    if (FilterSelectViewIsShown)
                        CurrentFilter = ParameterItem.Clone();
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnExpandAmountFilterTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    RangeSelectorIsShown = true;
                    var picker = new Controls.MGRangePicker();
                    picker.SetBinding(Controls.MGRangePicker.SelectedRangeProperty, "SelectedAmountRange");
                    RangePickerView = picker;
                    tcs?.SetResult(true);
                });
            }
        }

        private async Task SetupFilterViewForSelectedBuildings()
        {
            if (Buildings is null || !Buildings.Any(b => b.IsSelected))
            {
                SelectedBuildingsString = "Select";
                Units?.Clear();
                Tenants?.Clear();
            }
            else if (Buildings.Count(t => t.IsSelected) == 1)
            {
                SelectedBuildingsString = Buildings.First(t => t.IsSelected).BuildingShortAddress;
                var details = await Services.DataAccess.GetBuildingDetails(Buildings.First(t => t.IsSelected).BuildingId);
                Units = details.Units;
            }
            else if (Buildings.Count(t => t.IsSelected) > 1)
            {
                SelectedBuildingsString = SelectedBuildingsString + ", +" + (Buildings.Count(t => t.IsSelected) - 1).ToString() + " more";
                Units?.Clear();
                Tenants?.Clear();
            }
        }

        private async Task SetupFilterViewForSelectedUnits()
        {
            if (Units != null && Units.Count(t => t.IsSelected) == 1)
            {
                SelectedUnitString = Units.First(t => t.IsSelected).UnitName;
                var par = new TenantRequestItem
                {
                    Units = Units.Where(t => t.IsSelected).Select(t => t.UnitId).ToList()
                };
                SelectedTenantString = string.Empty;
                Tenants = (await DataAccess.GetTenantsAsync(par)).ToList();
            }
            else if (Units != null && Units.Count(t => t.IsSelected) > 1)
            {
                SelectedUnitString = SelectedUnitString + ", +" + (Units.Count(t => t.IsSelected) - 1).ToString() + " more";
                Tenants?.Clear();
                SelectedTenantString = string.Empty;
            }
            else if (Units is null || !Units.Any(b => b.IsSelected))
            {
                Tenants?.Clear();
                SelectedUnitString = "Select";
                SelectedTenantString = "Select";
            }
        }

        private void SetupFilterViewForSelectedTenants()
        {
            if (Tenants != null && Tenants.Count(t => t.IsSelected) == 1)
                SelectedTenantString = Tenants.First(t => t.IsSelected).FullName;
            else if (Tenants != null && Tenants.Count(t => t.IsSelected) > 1)
                SelectedTenantString = SelectedTenantString + ", " + (Tenants.Count(t => t.IsSelected) - 1).ToString() + " more";
            else
                SelectedTenantString = string.Empty;
        }

        public FreshAwaitCommand OnBuildingTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var building = (Building)par;
                    try
                    {
                        building.IsSelected = !building.IsSelected;
                        await SetupFilterViewForSelectedBuildings();
                        if (Units != null && Units.Any(u => u.IsSelected))
                        {
                            foreach (var u in Units.Where(u => u.IsSelected))
                            {
                                u.IsSelected = false;
                            }
                        }
                        if (Tenants != null)
                        {
                            foreach (var b in Tenants)
                            {
                                b.IsSelected = false;
                            }
                        }
                        FilterUnitsExpanded = false;
                        FilterTenantsExpanded = false;
                        SelectedUnitString = "Select";
                        SelectedTenantString = "Select";
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
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var unit = (Unit)par;
                    if (Buildings != null && Buildings.Count(t => t.IsSelected) > 1)
                    {
                        await CoreMethods.DisplayAlert("ManageGo", "Select only one building to select a unit", "OK");
                        tcs?.SetResult(true);
                        return;
                    }
                    unit.IsSelected = !unit.IsSelected;
                    await SetupFilterViewForSelectedUnits();
                    if (Tenants != null && Tenants.Any(u => u.IsSelected))
                    {
                        foreach (var u in Tenants.Where(u => u.IsSelected))
                        {
                            u.IsSelected = false;
                        }
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnTenantTapped
        {
            get
            {
                void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var tenant = (Tenant)par;
                    tenant.IsSelected = !tenant.IsSelected;
                    SetupFilterViewForSelectedTenants();
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnDismissPopupTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    RangeSelectorIsShown = false;
                    if (FilteredAmountRange != null)
                        SelectedAmountRange = FilteredAmountRange;
                    else
                        SelectedAmountRange = new Tuple<int?, int?>(0, 5000);
                    RangePickerView = null;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand SetFilterStatus
        {
            get
            {
                return new FreshAwaitCommand((parameter, tcs) =>
                {
                    var status = (string)parameter;
                    switch (status)
                    {
                        case "Sent":
                            SelectedSentPaymentsFilter = !SelectedSentPaymentsFilter;
                            break;
                        case "Reversed":
                            SelectedReversedPaymentsFilter = !SelectedReversedPaymentsFilter;
                            break;
                        case "Received":
                            SelectedReceivedPaymentsFilter = !SelectedReceivedPaymentsFilter;
                            break;
                        case "Refunded":
                            SelectedRefundedPaymentsFilter = !SelectedRefundedPaymentsFilter;
                            break;
                        default:
                            break;
                    }
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnApplyFilterRangeButtonTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    FilteredAmountRange = new Tuple<int?, int?>(SelectedAmountRange.Item1, SelectedAmountRange.Item2);

                    RangeSelectorIsShown = false;
                    RangePickerView = null;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnApplyFiltersTapped
        {
            get
            {
                async void execute(object parameter, TaskCompletionSource<bool> tcs)
                {
                    try
                    {
                        FilterDueDate = new DateRange(SelectedDateRange.StartDate, SelectedDateRange.EndDate);
                        PopContentView = null;
                        FilterSelectViewIsShown = false;
                        ParameterItem = new PaymentsRequestItem();
                        if (Buildings != null && Buildings.Any(f => f.IsSelected))
                            ParameterItem.Buildings = Buildings.Where(f => f.IsSelected).Select(f => f.BuildingId).ToList();
                        if (Units != null && Units.Any(f => f.IsSelected))
                            ParameterItem.Units = Units.Where(f => f.IsSelected).Select(f => f.UnitId).ToList();
                        if (Tenants != null && Tenants.Any(f => f.IsSelected))
                            ParameterItem.Tenants = Tenants.Where(f => f.IsSelected).Select(f => f.TenantID).ToList();
                        if (FilteredAmountRange != null)
                        {
                            if (FilteredAmountRange.Item1 > 0)
                                ParameterItem.AmountFrom = FilteredAmountRange.Item1;
                            if (FilteredAmountRange.Item2 < 5000)
                                ParameterItem.AmountTo = FilteredAmountRange.Item2;
                        }
                        if (!string.IsNullOrWhiteSpace(FilterKeywords))
                            ParameterItem.Search = FilterKeywords;
                        List<Models.PaymentStatuses> statuses = new List<PaymentStatuses>();
                        if (SelectedReceivedPaymentsFilter)
                            statuses.Add(PaymentStatuses.Received);
                        if (SelectedRefundedPaymentsFilter)
                            statuses.Add(PaymentStatuses.Refunded);
                        if (SelectedReversedPaymentsFilter)
                            statuses.Add(PaymentStatuses.Reveresed);
                        if (SelectedSentPaymentsFilter)
                            statuses.Add(PaymentStatuses.Sent);
                        if (statuses.Any())
                            ParameterItem.PaymentStatuses = statuses;
                        ParameterItem.DateFrom = FilterDueDate.StartDate;
                        ParameterItem.DateTo = FilterDueDate.EndDate;
                        CurrentFilter = ParameterItem;
                        await LoadData(refreshData: true, FetchNextPage: false);

                    }
                    catch (Exception ex)
                    {
                        Crashes.TrackError(ex);
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                        APIhasFailed = true;
                    }
                    finally
                    {
                        HasLoaded = true;
                        tcs?.SetResult(true);
                    }

                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCloseFliterViewTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    //set previous filter values if exists
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
                        if (Tenants != null && CurrentFilter.Tenants != null)
                        {
                            foreach (var t in Tenants)
                            {
                                if (CurrentFilter.Tenants.Contains(t.TenantID))
                                    t.IsSelected = true;
                                else
                                    t.IsSelected = false;
                            }
                        }
                        await SetupFilterViewForSelectedBuildings();
                        await SetupFilterViewForSelectedUnits();
                        SetupFilterViewForSelectedTenants();
                        FilterDueDate = new DateRange(CurrentFilter.DateFrom, CurrentFilter.DateTo);
                        FilteredAmountRange = new Tuple<int?, int?>(CurrentFilter.AmountFrom, CurrentFilter.AmountTo);
                        SelectedAmountRange = new Tuple<int?, int?>(CurrentFilter.AmountFrom, CurrentFilter.AmountTo);
                        FilterKeywords = CurrentFilter.Search;
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
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
                    if (Tenants != null)
                    {
                        foreach (var b in Tenants)
                        {
                            b.IsSelected = false;
                        }
                    }
                    SelectedReceivedPaymentsFilter = false;
                    SelectedRefundedPaymentsFilter = false;
                    SelectedSentPaymentsFilter = false;
                    SelectedReversedPaymentsFilter = false;
                    SelectedUnitString = "Select";
                    SelectedTenantString = "Select";
                    FilterDueDate = null;
                    SelectedBuildingsString = "Select";
                    FilteredAmountRange = null;
                    SelectedAmountRange = new Tuple<int?, int?>(0, 5000);
                    FilterKeywords = string.Empty;
                    ParameterItem = null;
                    CurrentFilter = null;
                    await LoadData();
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
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
                            FilterDueDateExpanded = false;
                            FilterBuildingsExpanded = false;
                            FilterUnitsExpanded = false;
                            FilterTenantsExpanded = false;
                            break;
                        case "DueDates":
                            FilterDueDateExpanded = !FilterDueDateExpanded;
                            FilterStatusExpanded = false;
                            FilterBuildingsExpanded = false;
                            FilterUnitsExpanded = false;
                            FilterTenantsExpanded = false;
                            break;
                        case "Buildings":
                            FilterBuildingsExpanded = !FilterBuildingsExpanded;
                            FilterDueDateExpanded = false;
                            FilterStatusExpanded = false;
                            FilterUnitsExpanded = false;
                            FilterTenantsExpanded = false;
                            break;
                        case "Units":
                            if (Buildings is null || !Buildings.Any(t => t.IsSelected) || Buildings.Count(b => b.IsSelected) > 1)
                            {
                                await CoreMethods.DisplayAlert("ManageGo", "Select one building first.", "Dismiss");
                            }
                            else if (Units is null || !Units.Any())
                            {
                                await CoreMethods.DisplayAlert("ManageGo", "No units in building.", "Dismiss");
                            }
                            else
                            {
                                FilterUnitsExpanded = !FilterUnitsExpanded;
                                FilterBuildingsExpanded = false;
                                FilterDueDateExpanded = false;
                                FilterStatusExpanded = false;
                                FilterTenantsExpanded = false;
                            }

                            break;
                        case "Tenants":
                            if (Units is null || !Units.Any(t => t.IsSelected))
                                await CoreMethods.DisplayAlert("ManageGo", "Select a unit first", "Dismiss");
                            else if (Units != null && Units.Count(t => t.IsSelected) > 1)
                                await CoreMethods.DisplayAlert("ManageGo", "Select one unit to see tenants.", "Dismiss");
                            else if (Tenants is null || !Tenants.Any())
                                await CoreMethods.DisplayAlert("ManageGo", "No tenants in unit", "Dismiss");
                            else
                            {
                                FilterTenantsExpanded = !FilterTenantsExpanded;
                                FilterBuildingsExpanded = false;
                                FilterDueDateExpanded = false;
                                FilterStatusExpanded = false;
                                FilterUnitsExpanded = false;
                            }
                            break;
                        default:
                            break;
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
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


    }
}
