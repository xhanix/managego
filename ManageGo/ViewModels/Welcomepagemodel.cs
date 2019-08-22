using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FreshMvvm;
using Plugin.Permissions.Abstractions;
using PropertyChanged;
using Xamarin.Essentials;

namespace ManageGo
{
    internal class WelcomePageModel : BaseDetailPage
    {
        public string TotalPaymentsThisWeek { get; private set; }
        public string TotalPaymentsThisMonth { get; private set; }
        public string TotalOpenTickets { get; private set; }
        public string TotalUnreadTickets { get; private set; }
        public bool UserCanViewPayments { get; private set; }
        public bool UserCanViewTickets { get; private set; }
        public string UserName { get; private set; }
        public string PMCName { get; private set; }

        public override void Init(object initData)
        {
            base.Init(initData);
            HamburgerIsVisible = true;
        }


        override internal async Task LoadData(bool refreshData = false, bool FetchNextPage = false)
        {
            if (cancellationTokenSource.IsCancellationRequested)
                return;
            App.CurrentPageModel = this;
            if (App.HasPendingNotification)
            {
                await App.NotificationReceived(App.NotificationType, App.NotificationObject, App.NotificationIsSummary);
            }
            try
            {
                var dashData = await Services.DataAccess.GetDashboardAsync();
                TotalPaymentsThisWeek = dashData.TotalPaymentsThisWeek.ToString("c2");
                TotalPaymentsThisMonth = dashData.TotalPaymentsThisMonth.ToString("c2");
                TotalOpenTickets = dashData.NumberOfOpenTickets.ToString();
                TotalUnreadTickets = dashData.NumberOfTicketsWithNoReplay.ToString();

                UserCanViewPayments = (App.UserPermissions &
                     UserPermissions.CanAccessPayments) == UserPermissions.CanAccessPayments;

                UserCanViewTickets = (App.UserPermissions &
                      UserPermissions.CanAccessTickets) == UserPermissions.CanAccessTickets;

                UserName = App.UserName;
                PMCName = App.PMCName;
                HasLoaded = true;
                APIhasFailed = false;
            }
            catch (Exception)
            {
                await ShowNoInternetView();
                APIhasFailed = true;
            }

        }


        private async Task ShowNoInternetView()
        {
            APIhasFailed = true;
            HasLoaded = false;
            ErrorText = Connectivity.NetworkAccess != NetworkAccess.Internet ?
                                    "No Internet Connection" : "Host Unreachable";
            await Task.Run(() =>
            {
                while (Connectivity.NetworkAccess != NetworkAccess.Internet
                       && !cancellationTokenSource.IsCancellationRequested)
                {
                    Thread.Sleep(1000);
                }
            });
            await LoadData();
        }
    }
}
