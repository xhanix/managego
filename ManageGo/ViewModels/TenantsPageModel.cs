using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;
using ManageGo.Services;
using System.Linq;
using FreshMvvm;
using Xamarin.Essentials;

namespace ManageGo
{
    internal class TenantsPageModel : BaseDetailPage
    {
        public bool FilterSelectViewIsShown { get; set; }
        public bool FilterBuildingsExpanded { get; set; }
        public bool FilterUnitsExpanded { get; set; }
        public bool FilterStatusExpanded { get; set; }
        public View PopContentView { get; set; }
        public bool ListIsEnabled { get; set; }
        public List<Building> Buildings { get; set; }
        public List<Unit> Units { get; set; }
        public List<Tenant> FetchedTenants { get; set; }
        private int LastLoadedItemId { get; set; }
        private int CurrentListPage { get; set; }
        private bool CanGetMorePages { get; set; }
        public string NumberOfAppliedFilters { get; set; }
        internal Dictionary<string, object> FiltersDictionary { get; set; }
        public bool IsSearching { get; set; }
        public string SelectedBuildingsString { get; set; }
        public bool SelectedActiveTenantFilter { get; set; }
        public bool SelectedInActiveTenantFilter { get; set; }
        public string ActiveTenantCheckBoxImage
        {
            get
            {
                return !this.SelectedActiveTenantFilter ? "unchecked.png" : "checked.png";
            }
        }
        public string InActiveTenantCheckBoxImage
        {

            get
            {
                return !this.SelectedInActiveTenantFilter ? "unchecked.png" : "checked.png";
            }
        }

        public string SelectedStatusFlagsString
        {
            get
            {
                if (this.SelectedActiveTenantFilter && this.SelectedInActiveTenantFilter || !this.SelectedActiveTenantFilter && !this.SelectedInActiveTenantFilter)
                    return "All";
                return !this.SelectedActiveTenantFilter ? "Inactive" : "Active";
            }
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            HamburgerIsVisible = true;
        }


        internal override async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            TenantsPageModel tenantsPageModel = this;
            if (!(tenantsPageModel.FetchedTenants == null | refreshData))
                return;
            if (tenantsPageModel.FiltersDictionary == null | refreshData)
                tenantsPageModel.FiltersDictionary = new Dictionary<string, object>()
                    {
                      { "PageSize", 30},
                      { "Page", CurrentListPage }
                    };
            try
            {
                if (tenantsPageModel.FetchedTenants == null | applyNewFilter)
                {
                    List<Tenant> tenantsAsync = await DataAccess.GetTenantsAsync(tenantsPageModel.FiltersDictionary);
                    tenantsPageModel.FetchedTenants = tenantsAsync;
                    double num = Math.Floor((double)tenantsPageModel.FetchedTenants.IndexOf(tenantsPageModel.FetchedTenants.Last<Tenant>()) / 2.0);
                    Tenant tenant = tenantsPageModel.FetchedTenants.ElementAt<Tenant>((int)num);
                    tenantsPageModel.LastLoadedItemId = tenant.TenantID;
                    tenantsPageModel.CanGetMorePages = tenantsPageModel.FetchedTenants.Count == 30;
                }
                else
                {
                    List<Tenant> list = tenantsPageModel.FetchedTenants.ToList<Tenant>();
                    List<Tenant> tenantsAsync = await DataAccess.GetTenantsAsync(tenantsPageModel.FiltersDictionary);
                    tenantsPageModel.CanGetMorePages = tenantsAsync.Count == 30;
                    list.AddRange((IEnumerable<Tenant>)tenantsAsync);
                    tenantsPageModel.FetchedTenants = list;
                    list = (List<Tenant>)null;
                }
                if (tenantsPageModel.FetchedTenants.Count > 0 && tenantsPageModel.CanGetMorePages)
                {
                    double num = Math.Floor((double)tenantsPageModel.FetchedTenants.IndexOf(tenantsPageModel.FetchedTenants.Last<Tenant>()) / 2.0);
                    Tenant tenant = tenantsPageModel.FetchedTenants.ElementAt<Tenant>((int)num);
                    tenantsPageModel.LastLoadedItemId = tenant.TenantID;
                }
                tenantsPageModel.Buildings = App.Buildings;
                tenantsPageModel.ListIsEnabled = true;
                Console.WriteLine(string.Format("Tenants Fetched: {0}", (object)tenantsPageModel.FetchedTenants.Count));
                tenantsPageModel.HasLoaded = true;
            }
            catch
            {
                tenantsPageModel.APIhasFailed = true;
                tenantsPageModel.FetchedTenants = (List<Tenant>)null;
                await CoreMethods.DisplayAlert("Something went wrong", "Unable to get tickets. Connect to network and try again", "Try again", "Dismiss");
            }
        }

        public FreshAwaitCommand OnCloseFliterViewTapped
        {
            get
            {
                return new FreshAwaitCommand((Action<TaskCompletionSource<bool>>)(tcs =>
                {
                    this.PopContentView = (View)null;
                    this.FilterSelectViewIsShown = false;
                    this.ListIsEnabled = true;
                    tcs?.SetResult(true);
                }));
            }
        }
        public FreshAwaitCommand SetFilterStatus
        {
            get
            {
                return new FreshAwaitCommand((Action<object, TaskCompletionSource<bool>>)((parameter, tcs) =>
                {
                    string str = (string)parameter;
                    if (!(str == "Active"))
                    {
                        if (str == "Inactive")
                            this.SelectedInActiveTenantFilter = !this.SelectedInActiveTenantFilter;
                    }
                    else
                        this.SelectedActiveTenantFilter = !this.SelectedActiveTenantFilter;
                    tcs?.SetResult(true);
                }));
            }
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (this.Buildings != null)
            {
                foreach (Building building in Buildings.Where(t => t.IsSelected))
                {
                    building.IsSelected = false;
                }
            }
            if (this.Units != null)
            {
                foreach (Unit unit in this.Units.Where((t => t.IsSelected)))
                    unit.IsSelected = false;
            }
            this.NumberOfAppliedFilters = " ";
            this.SelectedActiveTenantFilter = true;
            this.SelectedInActiveTenantFilter = true;
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
                    if (this.FilterSelectViewIsShown)
                    {
                        this.PopContentView = (View)null;
                        this.ListIsEnabled = true;
                    }
                    else
                    {
                        //  this.PopContentView = new TenantFilterSelectView((FreshBasePageModel)this).Content;
                        this.ListIsEnabled = false;
                    }
                    this.FilterSelectViewIsShown = !this.FilterSelectViewIsShown;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnContactTenantTapped
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var contactMethod = (string)par;
                    if (contactMethod.Contains("@"))
                    {
                        try
                        {
                            var message = new EmailMessage
                            {
                                Subject = string.Empty,
                                Body = string.Empty,
                                To = new List<string> { contactMethod },
                                //Cc = ccRecipients,
                                //Bcc = bccRecipients
                            };
                            await Email.ComposeAsync(message);
                        }
                        catch (FeatureNotSupportedException fbsEx)
                        {
                            // Email is not supported on this device
                            await CoreMethods.DisplayAlert("Email not supported", fbsEx.Message, "DISMISS");
                        }
                        catch (Exception ex)
                        {
                            await CoreMethods.DisplayAlert("Unable to send email", ex.Message, "DISMISS");
                        }
                    }
                    else
                    {
                        try
                        {
                            PhoneDialer.Open(contactMethod);
                        }
                        catch (ArgumentNullException anEx)
                        {
                            // Number was null or white space
                            await CoreMethods.DisplayAlert("Number was white space", anEx.Message, "DISMISS");
                        }
                        catch (FeatureNotSupportedException ex)
                        {
                            // Phone Dialer is not supported on this device.
                            await CoreMethods.DisplayAlert("Phone Dialer not supported", ex.Message, "DISMISS");
                        }
                        catch (Exception ex)
                        {
                            await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                        }
                    }
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnShowTenantDetailsTapped
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    var tenant = (Tenant)par;
                    tenant.DetailsShown = !tenant.DetailsShown;
                    tcs?.SetResult(true);
                });
            }
        }

        public string SelectedUnitString { get; set; }
        public string FilterKeywords { get; set; }
    }
}
