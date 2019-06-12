using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FreshMvvm;
using ManageGo.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Linq;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ManageGo
{
    public partial class App : Application
    {
        internal static FreshMasterDetailNavigationContainer MasterDetailNav { get; private set; }
        //set on login. See returned UserInfo.
        internal static UserPermissions UserPermissions { get; set; }
        internal static string UserName { get; set; }
        internal static string PMCName { get; set; }
        internal static SignedInUserInfo UserInfo { get; set; }
        internal static List<Building> Buildings { get; set; }
        internal static List<Categories> Categories { get; set; }
        internal static List<Tags> Tags { get; set; }
        internal static List<User> Users { get; set; }
        internal static List<ExternalContact> ExternalContacts { get; set; }
        internal FreshMasterDetailNavigationContainer MasterDetailContainer { get; private set; }
        internal static bool HasPendingNotification { get; private set; }
        internal static int NotificationType { get; private set; }
        internal static int NotificationObject { get; private set; }

        public static async Task NotificationReceived(int type, int notificationObject)
        {
            if (CurrentPageModel is null)
            {
                HasPendingNotification = true;
                NotificationObject = notificationObject;
                NotificationType = type;
                return;
            }
            HasPendingNotification = false;
            //1
            //5981
            var notificationType = (Models.PushNotificationType)type;
            switch (notificationType)
            {
                case PushNotificationType.TicketCreated:
                case PushNotificationType.TicketAssigned:
                case PushNotificationType.TicketReply:
                case PushNotificationType.TicketReplyInternal:
                    await ShowTicketNotification(notificationObject);
                    break;
                case PushNotificationType.TenantAwaitingApproval:
                case PushNotificationType.UnitAwaitingApproval:
                    await CurrentPageModel.CoreMethods.PushPageModel<NotificationsPageModel>(data: true);
                    break;
                case PushNotificationType.PaymentReceived:
                    await CurrentPageModel.CoreMethods.PushPageModel<PaymentsPageModel>(data: true);
                    break;
            }
        }

        private static async Task ShowTicketNotification(int ticketId)
        {
            try
            {
                var ticketDetails = await Services.DataAccess.GetTicketDetails(ticketId);
                var tickets = await Services.DataAccess.GetTickets(new TicketRequestItem { Ticket = ticketId });
                var ticket = tickets.FirstOrDefault();
                var dic = new Dictionary<string, object>
                            {
                            {"TicketDetails", ticketDetails},
                            {"TicketNumber", ticket.TicketNumber},
                            {"Address", ticket.Building?.BuildingName + " #" + ticket.Unit?.UnitName},
                            {"TicketTitleText", ticket.TicketSubject},
                            {"Ticket", ticket}
                            };
                await CurrentPageModel.CoreMethods.PushPageModel<TicketDetailsPageModel>(dic, false, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool MenuIsPresented
        {
            get
            {
                return MasterDetailNav.IsPresented;
            }
            set
            {
                MasterDetailNav.IsPresented = value;
            }
        }

        public static List<BankAccount> BankAccounts { get; internal set; }
        internal static BaseDetailPage CurrentPageModel { get; set; }

        public App()
        {
            InitializeComponent();
            Tags = new List<Tags>();
            Users = new List<User>();
            Categories = new List<Categories>();
            var page = FreshPageModelResolver.ResolvePageModel<LoginPageModel>();
            MainPage = page;
            ((LoginPageModel)page.BindingContext).OnSuccessfulLogin += Handle_OnSuccessfulLogin;
        }

        void Handle_OnSuccessfulLogin(object sender, bool e)
        {
            void action()
            {
                MasterDetailNav = new FreshMasterDetailNavigationContainer();
                MasterDetailNav.Init("");
                var page = FreshPageModelResolver.ResolvePageModel<MasterMenuPageModel>();
                ((MasterMenuPageModel)page.BindingContext).OnLogout += (_sender, _e) =>
                {
                    var __page = FreshPageModelResolver.ResolvePageModel<LoginPageModel>();
                    MainPage = __page;
                    MasterDetailNav = null;
                    MasterDetailContainer = null;
                    ((LoginPageModel)__page.BindingContext).OnSuccessfulLogin += Handle_OnSuccessfulLogin;
                };
                page.Title = "Menu";
                MasterDetailNav.Master = page;
                MasterDetailNav.AddPage<WelcomePageModel>("Home", null);
                CurrentPageModel = MasterDetailNav.Pages.Values.First().BindingContext as BaseDetailPage;
                MasterDetailNav.AddPage<MaintenanceTicketsPageModel>("Maintenance Tickets", null);
                this.MainPage = MasterDetailNav;
                MasterDetailContainer = MasterDetailNav;
                MasterDetailNav.IsPresentedChanged += (_sender, _e) =>
                {
                    if (_sender is FreshMasterDetailNavigationContainer)
                    {
                        foreach (var _page in ((FreshMasterDetailNavigationContainer)_sender).Pages.Values)
                        {
                            var nav = _page as NavigationPage;
                            if (nav.CurrentPage is null || nav.CurrentPage.BindingContext is null)
                                return;
                            if (nav.CurrentPage.BindingContext is BaseDetailPage currentPageModel)
                            {
                                if (!currentPageModel.IsModal)
                                    currentPageModel.HamburgerIsVisible = !MasterDetailNav.IsPresented;
                                else
                                    currentPageModel.HamburgerIsVisible = false;
                                CurrentPageModel = currentPageModel;
                            }
                        }
                    }
                };

            }
            Xamarin.Forms.Device.BeginInvokeOnMainThread(action);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
            AppCenter.Start("android=817faa27-5418-4f2b-b55a-0013186c5482;" +
                  "ios=575732ee-7291-4330-98b1-8f1c79713205;",
                  typeof(Analytics), typeof(Crashes));

        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }

    public interface IGoogleCloudMessagingHelper
    {
        void SubscribeToTopic(string topic);
        void UnSubscribeFromTopics();
    }

    public interface ILocalAuthHelper
    {
        LocalAuthType GetLocalAuthType();
        void Authenticate(string userId, Action onSuccess, Action onFailure);
    }

    public interface IPicturePicker
    {
        Task<Tuple<Stream, string, Services.MGFileType>> GetImageStreamAsync();
    }

}
