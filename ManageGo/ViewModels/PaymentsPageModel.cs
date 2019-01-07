using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;
using ManageGo.Models;
using ManageGo.Services;
using PropertyChanged;
using Xamarin.Forms;

namespace ManageGo
{
    internal class PaymentsPageModel : BaseDetailPage
    {
        public View RangePickerView { get; set; }
        int CurrentListPage { get; set; } = 1;
        DateRange dateRange;
        [AlsoNotifyFor("NumberOfAppliedFilters")]
        Dictionary<string, object> FilterDictionary { get; set; }
        public bool IsRefreshingList { get; set; }
        public bool FilterSelectViewIsShown { get; set; }
        public bool RangeSelectorIsShown { get; private set; }
        List<Payment> fetchedPayments;
        public List<Payment> FetchedPayments
        {
            get { return fetchedPayments; }
            set
            {
                fetchedPayments = value;
                if (fetchedPayments != null && !fetchedPayments.Any())
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
                return FilteredAmountRange is null ||
                    (FilteredAmountRange.Item1 == 0 && FilteredAmountRange.Item2 == 5000) ?
                    "All" : FilteredAmountRange.Item1.ToString("C0") + " - " +
                    (FilteredAmountRange.Item2 == 5000 ? FilteredAmountRange.Item2.ToString("C0") + "+"
                     : FilteredAmountRange.Item2.ToString("C0"));
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

        public Tuple<int, int> SelectedAmountRange { get; set; }

        [AlsoNotifyFor("FilterAmountString")]
        private Tuple<int, int> FilteredAmountRange { get; set; }


        [AlsoNotifyFor("CalendarButtonText", "FilterDueDateString")]
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

        public string FilterDueDateString
        {
            get
            {
                return FilterDueDate != null ?
                    FilterDueDate.EndDate.HasValue ? FilterDueDate.StartDate.ToShortDateString() + "-" + FilterDueDate.EndDate.Value.ToShortDateString()
                                     :
                    FilterDueDate.StartDate.ToShortDateString() : "All";
            }
        }
        public string SelectedStatusFlagsString
        {
            get
            {
                List<string> strings = new List<string>();
                if (SelectedSentPaymentsFilter)
                    strings.Add("Sent");
                if (SelectedReceivedPaymentsFilter)
                    strings.Add("Received");
                if (SelectedReversedPaymentsFilter)
                    strings.Add("Reveresed");
                if (SelectedRefundedPaymentsFilter)
                    strings.Add("Refunded");
                return string.Join(", ", strings);
            }
        }
        public string RangeSelectorMin { get { return SelectedAmountRange != null ? SelectedAmountRange.Item1.ToString("C0") : ""; } }
        public string RangeSelectorMax
        {
            get
            {
                if (SelectedAmountRange is null)
                    return string.Empty;
                else if (SelectedAmountRange.Item2 >= 5000)
                    return "$5,000+";
                return SelectedAmountRange.Item2.ToString("C0");
            }
        }

        public bool BackbuttonIsVisible { get; set; }
        public View PopContentView { get; private set; }
        public string NumberOfAppliedFilters
        {
            get
            {
                return FilterDictionary is null ||
                !FilterDictionary.Keys.Any() ? " " :
                    $"{FilterDictionary.Keys.Count(t => t != "DateFrom")}";
            }
        }

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
        }

        public bool NothingFetched { get; private set; }

        internal override async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            try
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        { "PageSize", 50},
                        { "Page", CurrentListPage},
                        { "DateFrom",FilterDueDate.StartDate}
                    };
                if (FilterDueDate.EndDate.HasValue)
                {
                    parameters.Add("DateTo", FilterDueDate.EndDate.Value);
                }
                FetchedPayments = await DataAccess.GetPaymentsAsync(parameters);
                HasLoaded = true;
            }
            catch
            {
                APIhasFailed = true;
                FetchedPayments = null;
                await CoreMethods.DisplayAlert("Something went wrong", "Unable to get payment records. Connect to network and try again", "Try again", "Dismiss");
            }
            finally
            {
                NothingFetched = FetchedPayments is null || !FetchedPayments.Any();
            }
        }

        public FreshAwaitCommand OnPulledToRefresh
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    IsRefreshingList = true;
                    if (FilterDictionary is null || !FilterDictionary.Keys.Any())
                        await LoadData(false, false);
                    else
                        OnApplyFiltersTapped.Execute(null);
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
                    var alreadyExpandedItem = FetchedPayments.FirstOrDefault(t => t.DetailsShown && t.PaymentId != payment.PaymentId);
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
                        //ListIsEnabled = true;
                    }
                    else
                    {
                        PopContentView = new Views.PaymentFilterView(this).Content;
                        // ListIsEnabled = false;
                    }
                    FilterSelectViewIsShown = !FilterSelectViewIsShown;
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

        public FreshAwaitCommand OnBuildingTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    var building = (Building)par;
                    try
                    {

                        var details = await Services.DataAccess.GetBuildingDetails(building.BuildingId);
                        foreach (Building b in Buildings)
                        {
                            b.IsSelected = false;
                        }
                        building.IsSelected = true;
                        SelectedBuildingsString = building.BuildingName;
                        Units = details.Units;
                        SelectedUnitString = string.Empty;
                    }
                    catch (Exception ex)
                    {
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
                    foreach (Unit u in Units)
                    {
                        u.IsSelected = false;
                    }
                    unit.IsSelected = true;
                    SelectedUnitString = unit.UnitName;
                    var dic = new Dictionary<string, object>
                        {
                        {"Units", Units.Where(t=>t.IsSelected).Select(t=>t.UnitId)}
                        };
                    SelectedTenantString = string.Empty;
                    Tenants = await DataAccess.GetTenantsAsync(dic);
                    if (!Tenants.Any())
                    {
                        FilterTenantsExpanded = false;
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
                    foreach (Tenant t in Tenants)
                    {
                        t.IsSelected = false;
                    }
                    tenant.IsSelected = true;
                    SelectedTenantString = tenant.FullName;
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
                        SelectedAmountRange = new Tuple<int, int>(0, 5000);
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
                    FilteredAmountRange = new Tuple<int, int>(SelectedAmountRange.Item1, SelectedAmountRange.Item2);
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

                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    Dictionary<string, object> paramDic = new Dictionary<string, object>();
                    if (Buildings != null && Buildings.Any(f => f.IsSelected))
                    {
                        paramDic.Add("Buildings", Buildings.Where(f => f.IsSelected).Select(f => f.BuildingId));
                    }
                    if (Units != null && Units.Any(f => f.IsSelected))
                    {
                        paramDic.Add("Units", Units.Where(f => f.IsSelected).Select(f => f.UnitId));
                    }
                    if (Tenants != null && Tenants.Any(f => f.IsSelected))
                    {
                        paramDic.Add("Tenants", Tenants.Where(f => f.IsSelected).Select(f => f.TenantID));
                    }
                    if (FilteredAmountRange != null)
                    {
                        if (FilteredAmountRange.Item1 > 0)
                        {
                            paramDic.Add("AmountFrom", FilteredAmountRange.Item1);
                        }
                        if (FilteredAmountRange.Item2 < 5000)
                        {
                            paramDic.Add("AmountTo", FilteredAmountRange.Item2);
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(FilterKeywords))
                    {
                        paramDic.Add("Search", FilterKeywords);
                    }
                    paramDic.Add("DateFrom", FilterDueDate.StartDate);
                    if (FilterDueDate.EndDate.HasValue)
                    {
                        paramDic.Add("DateTo", FilterDueDate.EndDate.Value);
                    }
                    FilterDictionary = paramDic;
                    HasLoaded = false;
                    FetchedPayments = await DataAccess.GetPaymentsAsync(paramDic);
                    HasLoaded = true;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCloseFliterViewTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs =>
                {
                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    tcs?.SetResult(true);
                }));
            }
        }

        public FreshAwaitCommand OnResetFiltersButtonTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    PopContentView = null;
                    FilterDictionary = null;
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
                    SelectedUnitString = string.Empty;
                    SelectedTenantString = string.Empty;
                    SelectedBuildingsString = string.Empty;
                    FilteredAmountRange = null;
                    SelectedAmountRange = new Tuple<int, int>(0, 5000);
                    FilterKeywords = string.Empty;
                    Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        { "PageSize", 50},
                        { "Page", CurrentListPage},
                        { "DateFrom",FilterDueDate.StartDate}
                    };
                    if (FilterDueDate.EndDate.HasValue)
                    {
                        parameters.Add("DateTo", FilterDueDate.EndDate.Value);
                    }
                    FetchedPayments = await DataAccess.GetPaymentsAsync(parameters);
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
                            break;
                        case "DueDates":
                            FilterDueDateExpanded = !FilterDueDateExpanded;
                            break;
                        case "Buildings":
                            FilterBuildingsExpanded = !FilterBuildingsExpanded;
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
                            break;
                        case "Tenants":
                            if (Units is null || !Units.Any(t => t.IsSelected))
                            {
                                await CoreMethods.DisplayAlert("ManageGo", "Select a unit first", "DIMISS");
                            }
                            else if (Tenants != null && !Tenants.Any())
                            {
                                await CoreMethods.DisplayAlert("ManageGo", "Unit appears to be empty", "DIMISS");
                            }
                            else
                            {
                                FilterTenantsExpanded = !FilterTenantsExpanded;
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
