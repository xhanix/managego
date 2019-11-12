using System;
using System.Threading.Tasks;
using FreshMvvm;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;

namespace ManageGo
{
    public class MasterMenuPageModel : FreshBasePageModel
    {
        public bool HamburgerIsVisible { get; set; }
        public bool PaymentsIsVisible { get; private set; }
        public bool MaintenanceIsVisible { get; private set; }
        public bool NotificationsIsVisible { get; private set; }
        public bool TenantIsVisible { get; private set; }

        internal event EventHandler<bool> OnLogout;
        public MasterMenuPageModel()
        {
            HamburgerIsVisible = true;
        }

        protected override void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            PaymentsIsVisible = App.UserPermissions.HasFlag(UserPermissions.CanAccessPayments);
            MaintenanceIsVisible = App.UserPermissions.HasFlag(UserPermissions.CanAccessTickets);
            NotificationsIsVisible = App.UserPermissions.HasFlag(UserPermissions.CanApproveNewTenantsUnits);
            TenantIsVisible = App.UserPermissions.HasFlag(UserPermissions.CanAccessTenants);
        }

        public FreshAwaitCommand OnSupportEmailTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    var address = "support@managego.com";
                    Analytics.TrackEvent("Support email tapped");
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
                    Analytics.TrackEvent("Support phone tapped");
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

        public FreshAwaitCommand OnNotificationsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("Notification menu tapped");

                    if (!App.MasterDetailNav.Pages.ContainsKey("Notifications"))
                        App.MasterDetailNav.AddPage<NotificationsPageModel>("Notifications");
                    App.MasterDetailNav.SwitchSelectedRootPageModel<NotificationsPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnMaintenanceTapped
        {
            get
            {
                async void execute(System.Threading.Tasks.TaskCompletionSource<bool> tcs)
                {
                    Analytics.TrackEvent("Tikets menu tapped");
                    if (!App.MasterDetailNav.Pages.ContainsKey("Maintenance Tickets"))
                        App.MasterDetailNav.AddPage<MaintenanceTicketsPageModel>("Maintenance Tickets");
                    await App.MasterDetailNav.SwitchSelectedRootPageModel<MaintenanceTicketsPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnHomeTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("Home menu tapped");

                    App.MasterDetailNav.SwitchSelectedRootPageModel<WelcomePageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }



        public FreshAwaitCommand OnFeedbackTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("Feedback menu tapped");

                    if (!App.MasterDetailNav.Pages.ContainsKey("Feedback"))
                        App.MasterDetailNav.AddPage<FeedbackPageModel>("Feedback");
                    App.MasterDetailNav.SwitchSelectedRootPageModel<FeedbackPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnSettingsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("Settings menu tapped");

                    if (!App.MasterDetailNav.Pages.ContainsKey("Settings"))
                        App.MasterDetailNav.AddPage<SettingsPageModel>("Settings");
                    App.MasterDetailNav.SwitchSelectedRootPageModel<SettingsPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnCalendarPageTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("Calendar menu tapped");

                    if (!App.MasterDetailNav.Pages.ContainsKey("OnCalendarPage"))
                        App.MasterDetailNav.AddPage<CalendarPageModel>("OnCalendarPage");
                    App.MasterDetailNav.SwitchSelectedRootPageModel<CalendarPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnTenantsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("Tenants menu tapped");

                    if (!App.MasterDetailNav.Pages.ContainsKey("Tenants"))
                        App.MasterDetailNav.AddPage<TenantsPageModel>("Tenants");
                    App.MasterDetailNav.SwitchSelectedRootPageModel<TenantsPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand PaymentsTapped
        {
            get
            {
                async void execute(TaskCompletionSource<bool> tcs)
                {

                    //allow the side menu animation to completed
                    Analytics.TrackEvent("Payments menu tapped");
                    if (!App.MasterDetailNav.Pages.ContainsKey("Payments"))
                        App.MasterDetailNav.AddPage<PaymentsPageModel>("Payments");
                    await App.MasterDetailNav.SwitchSelectedRootPageModel<PaymentsPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                }
                return new FreshAwaitCommand(execute);
            }
        }

        public FreshAwaitCommand OnTransactionsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("Transations menu tapped");

                    if (!App.MasterDetailNav.Pages.ContainsKey("Transactions"))
                        App.MasterDetailNav.AddPage<TransactionsPageModel>("Transactions");
                    App.MasterDetailNav.SwitchSelectedRootPageModel<TransactionsPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnBuildingsTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("Buildings menu tapped");

                    if (!App.MasterDetailNav.Pages.ContainsKey("Buildings"))
                        App.MasterDetailNav.AddPage<BuildingsListPageModel>("Buildings");
                    App.MasterDetailNav.SwitchSelectedRootPageModel<BuildingsListPageModel>();
                    App.MenuIsPresented = false;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand OnAmenitiesTapped => new FreshAwaitCommand((tcs) =>
        {
            Analytics.TrackEvent("Ameneties menu tapped");

            if (!App.MasterDetailNav.Pages.ContainsKey("Ameneties"))
                App.MasterDetailNav.AddPage<AmenitiesListPageModel>("Ameneties");
            App.MasterDetailNav.SwitchSelectedRootPageModel<AmenitiesListPageModel>();
            App.MenuIsPresented = false;
            tcs?.SetResult(true);
        });

        public FreshAwaitCommand OnLogoutTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    Analytics.TrackEvent("User logged out from menu");
                    DependencyService.Get<IGoogleCloudMessagingHelper>().UnSubscribeFromTopics();
                    App.Buildings = null;
                    App.Categories = null;
                    App.BankAccounts = null;
                    App.Tags = null;
                    App.UserInfo = null;
                    App.UserName = null;
                    App.UserPermissions = UserPermissions.None;
                    App.Users = null;
                    OnLogout?.Invoke(this, true);

                });
            }
        }
    }
}
