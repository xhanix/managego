using System;
using Xamarin.Forms;
using FreshMvvm;
using System.Threading.Tasks;

namespace ManageGo
{
    public class UpdatePageModel : FreshBasePageModel
    {
        public FreshAwaitCommand OnBackButtonTapped
        {
            get
            {
                Func<TaskCompletionSource<bool>, Task> funck = ClosePage;

                return new FreshAwaitCommand((tcs) => funck.Invoke(tcs));
            }
        }

        public FreshAwaitCommand OnUpdateButtonTapped
        {
            get
            {

                return new FreshAwaitCommand((tcs) =>
                {
                    DependencyService.Get<IAppStoreOpener>().OpenAppStore();
                    tcs?.SetResult(true);
                });
            }
        }

        async Task ClosePage(TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PopPageModel(modal: true);
            tcs?.SetResult(true);
        }
    }
}

