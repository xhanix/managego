using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FreshMvvm;

namespace ManageGo
{
    internal class NotificationsPageModel : BaseDetailPage
    {
        public ObservableCollection<Models.PendingApprovalItem> FetchedNotifications { get; set; }


        public FreshAwaitCommand ShowDetails
        {
            get
            {
                return new FreshAwaitCommand((par, tcs) =>
                {
                    Models.PendingApprovalItem item = (Models.PendingApprovalItem)par;
                    foreach (var i in FetchedNotifications.Where(t => t.DetailsShown && t.LeaseID != item.LeaseID))
                    {
                        i.DetailsShown = false;
                    }
                    item.DetailsShown = !item.DetailsShown;
                    tcs?.SetResult(true);
                });
            }
        }

        public FreshAwaitCommand ApproveItem
        {
            get
            {
                return new FreshAwaitCommand(async (par, tcs) =>
                {
                    Models.PendingApprovalItem item = (Models.PendingApprovalItem)par;
                    var confirmResult = await CoreMethods.DisplayActionSheet($"Confirm approval for {item.Tenant?.FullName}", "Cancel", "Approve");
                    if (confirmResult != "Approve")
                        return;
                    try
                    {
                        await Services.DataAccess.ApproveItem(item);
                        FetchedNotifications.Remove(item);
                    }
                    catch (Exception ex)
                    {
                        await CoreMethods.DisplayAlert("Something went wrong", ex.Message, "DISMISS");
                    }
                    tcs?.SetResult(true);
                });
            }
        }

        internal override async Task LoadData(bool refreshData = false, bool applyNewFilter = false)
        {
            try
            {
                FetchedNotifications = new ObservableCollection<Models.PendingApprovalItem>(
                    await Services.DataAccess.GetPendingNotifications());
            }
            catch (Exception ex)
            {
                APIhasFailed = true;
                await CoreMethods.DisplayAlert("Something went wrong!", ex.Message, "DIMISS");
            }
            finally
            {
                HasLoaded = true;
            }
        }
    }
}
