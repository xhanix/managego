using System;
using FreshMvvm;
using Xamarin.Forms;

namespace ManageGo
{
    public class MasterMenuPageModel : FreshBasePageModel
    {
        public bool HamburgerIsVisible { get; set; }
        public MasterMenuPageModel()
        {
            HamburgerIsVisible = true;
        }

        public FreshAwaitCommand OnSupportEmailTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    var address = "support@managego.com";
                    Device.OpenUri(new Uri($"mailto:{address}"));
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnSupportPhoneTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    var phone = "2123007950";
                    Device.OpenUri(new Uri($"tel:{phone}"));
                    tcs?.SetResult(true);
                });
            }
        }


        public FreshAwaitCommand OnMasterMenuTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnMaintenanceTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    App.MasterDetailNav.SwitchSelectedRootPageModel<MaintenanceTicketsPageModel>();
                    tcs?.SetResult(true);
                });
            }
        }
        public FreshAwaitCommand OnHomeTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    App.MasterDetailNav.SwitchSelectedRootPageModel<WelcomePageModel>();
                    tcs?.SetResult(true);
                });
            }
        }
    }
}
