using System;
using System.Linq;
using FreshMvvm;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ManageGo
{
    public partial class App : Application
    {
        internal static FreshMasterDetailNavigationContainer MasterDetailNav { get; private set; }
        //set on login. See returned UserInfo.
        internal static UserPermissions UserPermissions { get; set; }
        internal static string UserName { get; set; }

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

        public App()
        {
            InitializeComponent();
            MasterDetailNav = new FreshMasterDetailNavigationContainer();
            MasterDetailNav.Init("");
            var page = FreshPageModelResolver.ResolvePageModel<MasterMenuPageModel>();
            page.Title = "Menu";
            MasterDetailNav.Master = page;
            MasterDetailNav.AddPage<WelcomePageModel>("Home", null);
            MasterDetailNav.AddPage<MaintenanceTicketsPageModel>("Maintenance Tickets", null);
            MainPage = MasterDetailNav;
            MasterDetailNav.IsPresentedChanged += (sender, e) =>
            {
                foreach (var _page in ((FreshMasterDetailNavigationContainer)sender).Pages.Values)
                {
                    var nav = _page as NavigationPage;
                    (nav.CurrentPage.BindingContext as BaseDetailPage).HamburgerIsVisible = !(nav.CurrentPage.BindingContext as BaseDetailPage).HamburgerIsVisible;
                }
            };
        }

        protected override void OnStart()
        {
            // Handle when your app starts

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
}
