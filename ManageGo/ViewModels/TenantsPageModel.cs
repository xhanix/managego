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
        public string SelectedUnitString { get; set; }
        public string FilterKeywords { get; set; }
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
                    PopContentView = (View)null;
                    FilterSelectViewIsShown = false;
                    ListIsEnabled = true;
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
                return new FreshAwaitCommand(async (parameter, tcs) =>
                {

                    PopContentView = null;
                    var numberOfFilters = 0;
                    FilterSelectViewIsShown = false;
                    Dictionary<string, object> paramDic = new Dictionary<string, object>();
                    if (Buildings != null && Buildings.Any(f => f.IsSelected))
                    {
                        paramDic.Add("Buildings", Buildings.Where(f => f.IsSelected).Select(f => f.BuildingId));
                        numberOfFilters++;
                    }
                    if (Units != null && Units.Any(f => f.IsSelected))
                    {
                        paramDic.Add("Units", Units.Where(f => f.IsSelected).Select(f => f.UnitId));
                        numberOfFilters++;
                    }
                    if (SelectedActiveTenantFilter && SelectedInActiveTenantFilter)
                    {
                        paramDic.Add("Status", 2);
                    }
                    else if (SelectedActiveTenantFilter)
                    {
                        paramDic.Add("Status", 0);
                        numberOfFilters++;
                    }
                    else if (SelectedInActiveTenantFilter)
                    {
                        paramDic.Add("Status", 1);
                        numberOfFilters++;
                    }
                    else
                    {
                        paramDic.Add("Status", 2);
                    }
                    NumberOfAppliedFilters = numberOfFilters > 0 ? $"{numberOfFilters}" : " ";
                    FiltersDictionary = paramDic;
                    IsSearching = true;
                    FetchedTenants = await Services.DataAccess.GetTenantsAsync(paramDic);
                    ListIsEnabled = true;
                    IsSearching = false;
                    tcs?.SetResult(true);
                });
            }
        }
        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            Buildings = App.Buildings;
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
            if (Units != null)
            {
                foreach (Unit unit in Units.Where((t => t.IsSelected)))
                    unit.IsSelected = false;
            }
            NumberOfAppliedFilters = " ";
            SelectedActiveTenantFilter = true;
            SelectedInActiveTenantFilter = true;
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

        public FreshAwaitCommand OnExpandFilterTapped
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    switch ((string)par)
                    {
                        case "Status":
                            FilterStatusExpanded = !FilterStatusExpanded;
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
                        default:
                            break;
                    }
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnBuildingTapped
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    var building = (Building)par;
                    try
                    {
                        var details = await DataAccess.GetBuildingDetails(building.BuildingId);
                        foreach (Building b in Buildings)
                        {
                            b.IsSelected = false;
                        }
                        building.IsSelected = true;
                        SelectedBuildingsString = building.BuildingName;
                        Units = details.Units;
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    finally
                    {
                        tcs?.SetResult(true);
                    }
                });
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
                    SelectedActiveTenantFilter = false;
                    SelectedInActiveTenantFilter = false;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }
    }
}
