using System;
using System.Threading;
using System.Threading.Tasks;
using FreshMvvm;
using PropertyChanged;

namespace ManageGo
{
    internal abstract class BaseDetailPage : FreshBasePageModel
    {
        public bool HamburgerIsVisible { get; internal set; }
        [AlsoNotifyFor("IsLoading")]
        public bool HasLoaded { get; internal set; }
        [AlsoNotifyFor("IsLoading")]
        public bool APIhasFailed { get; internal set; }
        public bool IsLoading
        {
            get
            {
                return !APIhasFailed && !HasLoaded;
            }
        }
        public string ErrorText { get; internal set; }
        internal CancellationTokenSource cancellationTokenSource;


        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);
            cancellationTokenSource = new CancellationTokenSource();
            await LoadData();
        }

        internal abstract Task LoadData(bool refreshData = false, bool applyNewFilter = false);

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            base.ViewIsDisappearing(sender, e);
            cancellationTokenSource?.Cancel();
        }

        public FreshAwaitCommand OnMasterMenuTapped
        {
            get
            {
                return new FreshAwaitCommand((tcs) =>
                {
                    App.MenuIsPresented = true;
                    HamburgerIsVisible = false;
                    tcs?.SetResult(true);
                });
            }
        }

    }
}
