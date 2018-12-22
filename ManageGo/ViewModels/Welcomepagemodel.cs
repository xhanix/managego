using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FreshMvvm;
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
        override internal async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            if (cancellationTokenSource.IsCancellationRequested)
                return;

            await Services.DataAccess.GetDashboardAsync().ContinueWith(async (arg) =>
             {
                 if (arg.Status == TaskStatus.Faulted)
                 {
                     await ShowNoInternetView();
                 }
                 else if (arg.Status == TaskStatus.RanToCompletion)
                 {
                     HasLoaded = true;
                     APIhasFailed = false;
                     var result = arg.Result;
                     if (result.TryGetValue(APIkeys.TotalPaymentsThisWeek.ToString(), out string p)
                         && double.TryParse(p, out double numVal))
                     {
                         TotalPaymentsThisWeek = numVal.ToString("c2");
                     }
                     if (result.TryGetValue(APIkeys.TotalPaymentsThisMonth.ToString(), out string pm)
                        && double.TryParse(pm, out double numValp))
                     {
                         TotalPaymentsThisMonth = numValp.ToString("c2");
                     }
                     if (result.TryGetValue(APIkeys.NumberOfOpenTickets.ToString(), out string t))
                     {
                         TotalOpenTickets = t;
                     }
                     if (result.TryGetValue(APIkeys.NumberOfTicketsWithNoReplay.ToString(), out string ut))
                     {
                         TotalUnreadTickets = ut;
                     }

                     UserCanViewPayments =
                         (App.UserPermissions &
                          UserPermissions.CanAccessPayments) == UserPermissions.CanAccessPayments;

                     UserCanViewTickets =
                          (App.UserPermissions &
                           UserPermissions.CanAccessTickets) == UserPermissions.CanAccessTickets;

                     UserName = App.UserName;
                     PMCName = App.PMCName;
                 }

             });



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
