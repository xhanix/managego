using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;
using ManageGo.Services;

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

        internal override async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            HasLoaded = false;
            try
            {
                Building buildingDetails = await DataAccess.GetBuildingDetails(BuildingId);
                Units = buildingDetails.Units;
                BuildingName = buildingDetails.BuildingName;
                HasLoaded = true;
            }
            catch (Exception ex)
            {
                await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                APIhasFailed = true;
            }
        }
    }
}
