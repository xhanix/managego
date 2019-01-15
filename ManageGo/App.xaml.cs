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
                        if (nav.CurrentPage.BindingContext is BaseDetailPage)
                            (nav.CurrentPage.BindingContext as BaseDetailPage).HamburgerIsVisible = !MasterDetailNav.IsPresented;
                    }
                }
            };
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
