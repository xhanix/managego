using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using ManageGo.Services;
using Microsoft.AppCenter.Crashes;

namespace ManageGo
{
    internal class UnitsListPageModel : BaseDetailPage
    {
        private int BuildingId { get; set; }
        public bool BackbuttonIsVisible { get; private set; }
        public string BuildingName { get; set; }
        public List<Unit> Units { get; set; }
        public override void Init(object initData)
        {
            base.Init(initData);
            if (!(initData is int))
                return;
            this.BuildingId = (int)initData;
            BackbuttonIsVisible = true;
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            App.MasterDetailNav.IsGestureEnabled = false;
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            if (App.MasterDetailNav != null)
                App.MasterDetailNav.IsGestureEnabled = true;
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

        public FreshAwaitCommand OnShowTenantsTapped
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    Unit unit = (Unit)par;
                    await CoreMethods.PushPageModel<TenantsPageModel>(data: unit.BuildingId, modal: false);
                    tcs?.SetResult(true);
                });
            }
        }

        internal override async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            HasLoaded = false;
            if (BuildingId == 0)
                return;
            try
            {
                Building buildingDetails = await DataAccess.GetBuildingDetails(BuildingId);
                Units = buildingDetails.Units;
                foreach (var u in Units)
                {
                    u.BuildingId = BuildingId;
                }
                BuildingName = buildingDetails.BuildingName;
                HasLoaded = true;
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                APIhasFailed = true;
            }
        }
    }
}
