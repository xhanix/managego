using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FreshMvvm;
using ManageGo.Services;

namespace ManageGo.ViewModels
{
    internal class UnitsListPageModel : BaseDetailPage
    {
        private int BuildingId { get; set; }
        public string BuildingName { get; set; }
        public List<Unit> Units { get; set; }
        public override void Init(object initData)
        {
            base.Init(initData);
            if (!(initData is int))
                return;
            this.BuildingId = (int)initData;
        }
        public FreshAwaitCommand OnClosePageTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {
                    await CoreMethods.PopPageModel();
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }
        internal override async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            UnitsListPageModel unitsListPageModel = this;
            unitsListPageModel.HasLoaded = false;
            try
            {
                Building buildingDetails = await DataAccess.GetBuildingDetails(unitsListPageModel.BuildingId);
                unitsListPageModel.Units = buildingDetails.Units;
                unitsListPageModel.BuildingName = buildingDetails.BuildingName;
            }
            catch (Exception ex)
            {
                await unitsListPageModel.CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                unitsListPageModel.APIhasFailed = true;
            }
            finally
            {
                unitsListPageModel.HasLoaded = true;
            }
        }
    }
}
