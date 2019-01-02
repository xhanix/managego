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
    internal class TransactionsPageModel : BaseDetailPage
    {

        int CurrentListPage { get; set; } = 1;
        public View RangePickerView { get; set; }
        public string FilterKeywords { get; set; }
        DateRange dateRange;
        [AlsoNotifyFor("NumberOfAppliedFilters")]
        Dictionary<string, object> FilterDictionary { get; set; }
        public bool FilterSelectViewIsShown { get; set; }
        public List<BankTransaction> FetchedTransactions { get; set; }
        public bool RangeSelectorIsShown { get; private set; }
        public List<BankAccount> BankAccounts { get; private set; }
        public Tuple<int, int> SelectedAmountRange { get; set; }
        public bool BackbuttonIsVisible { get; set; }
        [AlsoNotifyFor("FilterAmountString")]
        private Tuple<int, int> FilteredAmountRange { get; set; }
        public bool FilterAccountsExpanded { get; set; }
        public bool FilterDateRangeExpanded { get; set; }
        public View PopContentView { get; private set; }
        public string NumberOfAppliedFilters => FilterDictionary is null || !FilterDictionary.Keys.Any() ? " " : $"{FilterDictionary.Keys.Count}";
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
        }

        public bool NothingFetched { get; private set; }

        internal override async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {

            try
            {
                if (FilterDictionary is null || refreshData)
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        { "PageSize", 50},
                        { "Page", CurrentListPage},
                        { "DateFrom", DateRange.StartDate},
                    };
                    if (DateRange.EndDate.HasValue)
                    {
                        parameters.Add("DateTo", DateRange.EndDate.Value);
                    }
                    FetchedTransactions = await Services.DataAccess.GetTransactionsAsync(parameters);
                }
                else
                {
                    FetchedTransactions = await Services.DataAccess.GetTransactionsAsync(FilterDictionary);
                }
                HasLoaded = true;
            }
            catch (Exception)
            {
                APIhasFailed = true;
                FetchedTransactions = (List<BankTransaction>)null;
                await CoreMethods.DisplayAlert("Something went wrong", "Unable to get tickets. Connect to network and try again", "Try again", "Dismiss");
            }
            finally
            {
                NothingFetched = FetchedTransactions is null || !FetchedTransactions.Any();
            }

        }


        public FreshAwaitCommand OnShowDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var transaction = (BankTransaction)par;
                    var alreadyExpandedItem = FetchedTransactions.FirstOrDefault(t => t.DetailsShown && t.Id != transaction.Id);
                    if (alreadyExpandedItem != null)
                        alreadyExpandedItem.DetailsShown = false;
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
                return new FreshAwaitCommand(async (parameter, tcs) =>
                {

                    PopContentView = null;
                    FilterSelectViewIsShown = false;
                    Dictionary<string, object> paramDic = new Dictionary<string, object>();
                    if (BankAccounts != null && BankAccounts.Any(f => f.IsSelected))
                    {
                        paramDic.Add("BankAccounts", BankAccounts.Where(f => f.IsSelected).Select(f => f.BankAccountID));
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
                    paramDic.Add("DateFrom", DateRange.StartDate);
                    if (DateRange.EndDate.HasValue)
                    {
                        paramDic.Add("DateTo", DateRange.EndDate.Value);
                    }
                    FilterDictionary = paramDic;
                    HasLoaded = false;
                    FetchedTransactions = await DataAccess.GetTransactionsAsync(paramDic);
                    HasLoaded = true;
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
                    FilterDictionary = null;
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
                    SelectedAmountRange = new Tuple<int, int>(0, 5000);
                    FilterKeywords = string.Empty;
                    Dictionary<string, object> parameters = new Dictionary<string, object>
                    {
                        { "PageSize", 50},
                        { "Page", CurrentListPage},
                        { "DateFrom",DateRange.StartDate}
                    };
                    if (DateRange.EndDate.HasValue)
                    {
                        parameters.Add("DateTo", DateRange.EndDate.Value);
                    }
                    FetchedTransactions = await DataAccess.GetTransactionsAsync(parameters);
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
                        SelectedAmountRange = new Tuple<int, int>(0, 5000);
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
                        //ListIsEnabled = true;
                    }
                    else
                    {
                        PopContentView = new Views.TransactionsFilterPage(this).Content;
                        // ListIsEnabled = false;
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
                    foreach (BankAccount b in BankAccounts)
                    {
                        b.IsSelected = false;
                    }
                    account.IsSelected = true;
                    SelectedAccountString = account.Title;
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


    }
}
