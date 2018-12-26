using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomCalendar;
using FreshMvvm;
using ManageGo.Models;
using PropertyChanged;
using Xamarin.Forms;

namespace ManageGo
{
    internal class PaymentsPageModel : BaseDetailPage
    {
        int CurrentListPage { get; set; } = 1;
        DateRange dateRange;
        [AlsoNotifyFor("NumberOfAppliedFilters")]
        Dictionary<string, object> FilterDictionary { get; set; }
        public bool FilterSelectViewIsShown { get; set; }
        public bool RangeSelectorIsShown { get; private set; }
        public List<Payment> FetchedPayments { get; set; }
        public List<Building> Buildings { get; private set; }
        public List<Tenant> Tenants { get; private set; }
        public List<Unit> Units { get; private set; }



        public bool BackbuttonIsVisible { get; set; }
        public View PopContentView { get; private set; }
        public string NumberOfAppliedFilters
        {
            get
            {
                return FilterDictionary is null || !FilterDictionary.Keys.Any() ? " " : $"{FilterDictionary.Keys.Count}";
            }
        }
        [AlsoNotifyFor("CalendarButtonText")]
        DateRange DateRange
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
                    FetchedPayments = await Services.DataAccess.GetPaymentsAsync(parameters);
                }
                else
                {
                    FetchedPayments = await Services.DataAccess.GetPaymentsAsync(FilterDictionary);
                }
                HasLoaded = true;
            }
            catch (Exception)
            {
                APIhasFailed = true;
                FetchedPayments = (List<Payment>)null;
                await CoreMethods.DisplayAlert("Something went wrong", "Unable to get tickets. Connect to network and try again", "Try again", "Dismiss");
            }
            finally
            {
                NothingFetched = FetchedPayments is null || !FetchedPayments.Any();
            }

        }


        public FreshAwaitCommand OnShowDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var tenant = (Payment)par;
                    tenant.DetailsShown = !tenant.DetailsShown;
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
