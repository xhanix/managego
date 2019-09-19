using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;
using ManageGo.Models;
using Microsoft.AppCenter.Crashes;
using PropertyChanged;
using Xamarin.Forms;

namespace ManageGo
{
    internal class TransactionsPageModel : BaseDetailPage
    {
        int CurrentListPage { get; set; } = 1;
        public View RangePickerView { get; set; }
        public string FilterKeywords { get; set; }
        DateRange dateRange;
        private bool filterSelectViewIsShown;

        public TransactionsRequestItem CurrentFilter { get; private set; }
        public TransactionsRequestItem ParameterItem { get; set; }
        public string NumberOfAppliedFilters { get; internal set; } = " ";

        public bool FilterSelectViewIsShown {
            get => filterSelectViewIsShown;
            set
            {
                filterSelectViewIsShown = value;
                FilterDateRangeExpanded = false;
                FilterAccountsExpanded = false;
            }
        }

        public ObservableCollection<BankTransaction> FetchedTransactions { get; set; }
        public bool RangeSelectorIsShown { get; private set; }
        public List<BankAccount> BankAccounts { get; private set; }
        public Tuple<int?, int?> SelectedAmountRange { get; set; } = new Tuple<int?, int?>(0, 5000);
        public bool BackbuttonIsVisible { get; set; }
        [AlsoNotifyFor("FilterAmountString")]
        private Tuple<int?, int?> FilteredAmountRange { get; set; }
        public bool FilterAccountsExpanded { get; set; }
        public bool FilterDateRangeExpanded { get; set; }
        public View PopContentView { get; private set; }

        public string SelectedAccountString { get; private set; }
        [AlsoNotifyFor("CalendarButtonText")]
        public DateRange DateRange
        {
            get
            {
                return dateRange is null ? new DateRange(DateTime.Today, DateTime.Today.AddDays(-30))
                    : dateRange;
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

        public string RangeSelectorMin { get { return SelectedAmountRange != null ? SelectedAmountRange.Item1?.ToString("C0") : ""; } }
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

        public string FilterAmountString
        {
            get
            {
                if (FilteredAmountRange is null ||
                    (FilteredAmountRange.Item1 == 0 && FilteredAmountRange.Item2 == 5000))
                    return "All";
                else
                {
                    var minVal = FilteredAmountRange.Item1 ?? 0;
                    var maxVal = FilteredAmountRange.Item2 ?? 5000;
                    return minVal.ToString("C0") + " - " +
                       (maxVal == 5000 ? maxVal.ToString("C0") + "+" : FilteredAmountRange.Item2?.ToString("C0"));
                }
            }
        }

        public string CalendarButtonText
        {
            get
            {
                if (DateRange.EndDate.HasValue)
                {
                    if (DateRange.StartDate == DateRange.EndDate)
                        return DateRange.StartDate.ToString("MMM-dd");
                    return DateRange.StartDate.ToString("MMM dd") + "-" + DateRange.EndDate.Value.ToString("MMM dd");
                }
                return DateRange.StartDate.ToString("MMM-dd");
            }
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            BankAccounts = App.BankAccounts;
            async void p(object sender, BankTransaction e)
            {
                var id = e.Id;
                if (id == LastLoadedItemId && CanGetMorePages)
                {
                    LastLoadedItemId = 0;
                    await LoadData(refreshData: false, FetchNextPage: true);
                }
            }
            ((TransactionsPage)this.CurrentPage).OnTransactionAppeared += p;
            App.OnLoggedOut += App_OnLoggedOut;
        }

        private void App_OnLoggedOut(object sender, bool e)
        {
            BankAccounts = null;

        }

        public bool NothingFetched { get; private set; }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            NothingFetched = false;
            try
            {
                if (FetchNextPage && ParameterItem != null)
                {
                    ParameterItem.Page++;
                    if (ParameterItem.DateFrom.HasValue && !ParameterItem.DateTo.HasValue)
                        ParameterItem.DateTo = ParameterItem.DateFrom.Value.AddDays(1);
                    var nextPage = await Services.DataAccess.GetTransactionsAsync(ParameterItem);
                    if (nextPage != null)
                    {
                        foreach (var item in nextPage)
                        {
                            FetchedTransactions.Add(item);
                        }
                    }
                    CanGetMorePages = nextPage != null && nextPage.Count == ParameterItem.PageSize;
                    var lastIdx = FetchedTransactions.IndexOf(FetchedTransactions.Last());
                    var index = Math.Floor(lastIdx / 2d);
                    var markedItem = FetchedTransactions.ElementAt((int)index);
                    LastLoadedItemId = markedItem.Id;
                }
                else
                {
                    HasLoaded = false;
                    if (ParameterItem is null)
                    {
                        ParameterItem = new TransactionsRequestItem
                        {
                            DateFrom = DateRange?.StartDate,
                            DateTo = DateRange?.EndDate
                        };
                    }
                    if (refreshData)
                        ParameterItem.Page = 1;

                    // FetchedTickets is null on view init
                    if (ParameterItem.DateFrom.HasValue && !ParameterItem.DateTo.HasValue)
                        ParameterItem.DateTo = ParameterItem.DateFrom.Value.AddDays(1);
                    var fetchedTransactions = await Services.DataAccess.GetTransactionsAsync(ParameterItem);

                    if (fetchedTransactions != null)
                        FetchedTransactions = new ObservableCollection<BankTransaction>(fetchedTransactions);
                    else
                        FetchedTransactions = new ObservableCollection<BankTransaction>();
                    CanGetMorePages = FetchedTransactions != null && FetchedTransactions.Count == ParameterItem.PageSize;
                    if (FetchedTransactions.Any() && CanGetMorePages)
                    {
                        var lastIdx = FetchedTransactions.IndexOf(FetchedTransactions.Last());
                        var index = Math.Floor(lastIdx / 2d);
                        var markedItem = FetchedTransactions.ElementAt((int)index);
                        LastLoadedItemId = markedItem.Id;
                    }
                }
                ((TransactionsPage)CurrentPage).DataLoaded();
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                APIhasFailed = true;
                FetchedTransactions = null;
                await CoreMethods.DisplayAlert("Something went wrong", $"Unable to get tickets. Message: {ex.Message}", "Try again", "Dismiss");
            }
            finally
            {
                HasLoaded = true;
                NothingFetched = FetchedTransactions is null || !FetchedTransactions.Any();
            }
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (FilterSelectViewIsShown)
            {
                UndoUnsavedFilterOptions();
                FilterSelectViewIsShown = false;
                PopContentView = null;
                if (App.MasterDetailNav != null)
                App.MasterDetailNav.IsGestureEnabled = true;
            }
        }


        public FreshAwaitCommand OnShowDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var transaction = (BankTransaction)par;
                    if (transaction is null)
                        return;
                    if (FetchedTransactions != null && FetchedTransactions.Any())
                    {
                        var alreadyExpandedItem = FetchedTransactions?.FirstOrDefault(t => t.DetailsShown && t.Id != transaction.Id);
                        if (alreadyExpandedItem != null)
                            alreadyExpandedItem.DetailsShown = false;
                    }
                    transaction.DetailsShown = !transaction.DetailsShown;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnExpandFilterTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    switch ((string)par)
                    {
                        case "DueDates":
                            FilterDateRangeExpanded = !FilterDateRangeExpanded;
                            break;
                        case "Accounts":
                            FilterAccountsExpanded = !FilterAccountsExpanded;
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
                    PopContentView = null;
                    DateRange = new DateRange(SelectedDateRange.StartDate, SelectedDateRange.EndDate);
                    FilterSelectViewIsShown = false;
                    ParameterItem = new TransactionsRequestItem();
                    if (!string.IsNullOrWhiteSpace(FilterKeywords))
                        ParameterItem.Search = FilterKeywords;
                    else
                    {
                        if (BankAccounts != null && BankAccounts.Any(f => f.IsSelected))
                            ParameterItem.BankAccounts = BankAccounts.Where(f => f.IsSelected).Select(f => f.BankAccountID).ToList();
                        ParameterItem.AmountFrom = FilteredAmountRange?.Item1;
                        ParameterItem.AmountTo = FilteredAmountRange?.Item2 == 5000 ? null : FilteredAmountRange?.Item2;
                        ParameterItem.DateFrom = DateRange.StartDate;
                        ParameterItem.DateTo = DateRange.EndDate;
                    }
                    NumberOfAppliedFilters = ParameterItem.NumberOfAppliedFilters == 0 ? "" : $"{ParameterItem.NumberOfAppliedFilters}";
                    await LoadData(refreshData: true, FetchNextPage: false);
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
                    ParameterItem = null;
                    CurrentFilter = null;
                    NumberOfAppliedFilters = string.Empty;
                    DateRange = null;
                    if (BankAccounts != null)
                    {
                        foreach (var b in BankAccounts)
                        {
                            b.IsSelected = false;
                        }
                    }
                    SelectedAccountString = string.Empty;
                    FilteredAmountRange = null;
                    SelectedAmountRange = new Tuple<int?, int?>(0, 5000);
                    FilterKeywords = string.Empty;
                    await LoadData();
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
                    UndoUnsavedFilterOptions();
                    tcs?.SetResult(true);
                }));
            }
        }

        private void UndoUnsavedFilterOptions()
        {
            if (CurrentFilter != null)
            {
                if (BankAccounts != null && CurrentFilter.BankAccounts != null)
                {
                    foreach (var b in BankAccounts)
                    {
                        if (CurrentFilter.BankAccounts.Contains(b.BankAccountID))
                            b.IsSelected = true;
                        else
                            b.IsSelected = false;
                    }
                }
                if (CurrentFilter.DateFrom.HasValue && CurrentFilter.DateTo.HasValue)
                    DateRange = new DateRange(CurrentFilter.DateFrom.Value, CurrentFilter.DateTo.Value);
                else if (CurrentFilter.DateFrom.HasValue && !CurrentFilter.DateTo.HasValue)
                    DateRange = new DateRange(CurrentFilter.DateFrom.Value);
                FilteredAmountRange = new Tuple<int?, int?>(CurrentFilter.AmountFrom, CurrentFilter.AmountTo);
                SelectedAmountRange = new Tuple<int?, int?>(CurrentFilter.AmountFrom, CurrentFilter.AmountTo);
                FilterKeywords = CurrentFilter.Search;
            }
            else
            {
                dateRange = null;
                SelectedAmountRange = new Tuple<int?, int?>(0, 5000);
                FilteredAmountRange = null;
                BankAccounts?.ForEach(t => t.IsSelected = false);
                
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
                    }
                    else
                    {
                        App.MasterDetailNav.IsGestureEnabled = false;
                        SelectedDateRange = new DateRange(DateRange.StartDate, DateRange.EndDate);
                        CurrentFilter = ParameterItem.Clone();
                        PopContentView = new Views.TransactionsFilterPage(this).Content;
                        SelectedAmountRange = new Tuple<int?, int?>(0, 5000);
                    }
                    FilterSelectViewIsShown = !FilterSelectViewIsShown;
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnBankAccountTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var account = (BankAccount)par;
                    account.IsSelected = !account.IsSelected;
                    int numOfSelectedAccount = BankAccounts.Count(t => t.IsSelected);
                    SelectedAccountString = numOfSelectedAccount == 0 ? string.Empty :
                       numOfSelectedAccount == 1 ? account.Title : numOfSelectedAccount.ToString() + " accounts";
                    tcs?.SetResult(true);
                });
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

        public bool CanGetMorePages { get; private set; }
        public int LastLoadedItemId { get; private set; }
    }
}
