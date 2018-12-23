using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo
{
    internal class BuildingsListPageModel : BaseDetailPage
    {
        public List<Building> Buildings { get; set; }
        public bool IsShowingUnitsPage { get; set; }
        public bool IsSearching { get; set; }
        internal override Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            this.Buildings = App.Buildings ?? new List<Building>();
            return (Task)Task.FromResult<int>(0);
        }

        public FreshAwaitCommand OnTicketsTapped
        {
            get
            {
                async void execute(object par, TaskCompletionSource<bool> tcs)
                {
                    Building building = (Building)par;
                    await CoreMethods.PushPageModel<MaintenanceTicketsPageModel>(building.BuildingId, false, true);
                    tcs?.SetResult(true);
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
                    Building building = (Building)par;
                    IsShowingUnitsPage = true;
                    await CoreMethods.PushPageModel<UnitsListPageModel>(building.BuildingId, false, true);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }
        public FreshAwaitCommand OnTenantTapped
        {
            get
            {
                async void p1(object par, TaskCompletionSource<bool> tcs)
                {
                    Building building = (Building)par;
                    if (!App.MasterDetailNav.Pages.ContainsKey("Tenants"))
                        App.MasterDetailNav.AddPage<TenantsPageModel>("Tenants", (object)null);
                    FreshBasePageModel freshBasePageModel = await App.MasterDetailNav.SwitchSelectedRootPageModel<TenantsPageModel>();
                    if (App.MasterDetailNav.Detail is NavigationPage && ((NavigationPage)App.MasterDetailNav.Detail).CurrentPage.GetModel() is TenantsPageModel model)
                    {
                        model.Buildings = App.Buildings;
                        if (model.Buildings != null)
                        {
                            foreach (Building building1 in model.Buildings.Where(b => b.BuildingId == building.BuildingId))
                                building1.IsSelected = true;
                            model.OnApplyFiltersTapped.Execute(null);
                        }
                    }
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(p1);
            }
        }
    }
}
