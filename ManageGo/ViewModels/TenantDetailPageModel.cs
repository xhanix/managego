using System;
using System.Collections.Generic;
using System.Linq;
using FreshMvvm;
using Microsoft.AppCenter.Analytics;
using PropertyChanged;
using Xamarin.Forms;

namespace ManageGo
{
    public class TenantDetailPageModel : FreshBasePageModel
    {

        public Tenant Tenant { get; private set; }
        public List<Unit> TenantUnits => Tenant.TenantUnits;
        public double UnitsListHeight => Tenant.UnitsListHeight;
        public string FullName => Tenant.TenantDetails;
        public string TenantEmailAddress => Tenant.TenantEmailAddress;
        public string TenantHomePhone => Tenant.TenantHomePhone;
        public string TenantCellPhone => Tenant.TenantCellPhone;
        public bool HasUnits => Tenant.TenantUnits != null && Tenant.TenantUnits.Any();

        public override void Init(object initData)
        {
            base.Init(initData);
            if (initData is Tenant tenant)
            {
                Tenant = tenant;
                RaisePropertyChanged();
            }
            Analytics.TrackEvent("View tenant details from Ticket");
            if (Tenant.TenantUnits is null)
                Analytics.TrackEvent("Tenant units is null from Ticket detail page");
        }

        public FreshAwaitCommand OnBackButtonTapped
        {
            get
            {
                async void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    Analytics.TrackEvent("Closed tenant details from Ticket");
                    await CoreMethods.PopPageModel(modal: true);
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnEmailTenantTapped
        {
            get
            {
                void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    Analytics.TrackEvent("Tenant email tapped from Ticket");
                    Device.OpenUri(new Uri($"mailto:{Tenant.TenantEmailAddress}"));
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnHomePhoneTenantTapped
        {
            get
            {
                void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    Analytics.TrackEvent("Tenant home phone tapped from Ticket");
                    Device.OpenUri(new Uri($"tel:{Tenant.TenantHomePhone}"));
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnCellPhoneTenantTapped
        {
            get
            {
                void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    Analytics.TrackEvent("Tenant cell phone tapped from Ticket");
                    Device.OpenUri(new Uri($"tel:{Tenant.TenantCellPhone}"));
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }
    }
}
