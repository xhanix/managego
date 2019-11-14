using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreshMvvm;

namespace ManageGo
{
    internal class BuildingsListPageModel : BaseDetailPage
    {
        public List<Building> Buildings { get; set; }
        public bool IsShowingUnitsPage { get; set; }
        public bool IsSearching { get; set; }
        public bool CanSelectTenants { get; set; }
        public bool CanAccessMaintenance { get; set; }

        internal override Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            this.Buildings = App.Buildings ?? new List<Building>();
            ((BuildingsListPage)CurrentPage).DataLoaded();
            return Task.FromResult(0);
        }


        public override void Init(object initData)
        {
            base.Init(initData);
            App.OnLoggedOut += App_OnLoggedOut;
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            CanSelectTenants = App.UserPermissions.HasFlag(UserPermissions.CanAccessTenants);
            CanAccessMaintenance = App.UserPermissions.HasFlag(UserPermissions.CanAccessTickets);
        }

        private void App_OnLoggedOut(object sender, bool e)
        {
            Buildings = null;
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
                    if (!CanSelectTenants)
                        return;
                    Building building = (Building)par;
                    await CoreMethods.PushPageModel<TenantsPageModel>(data: building);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(p1);
            }
        }
    }
}
