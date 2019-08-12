using System;
using System.Threading.Tasks;

namespace MGDataAccessLibrary.BussinessLogic
{
    public class NotificatinsProcessor
    {
        public static async Task RespondToNotification(int LeaseId, bool isApprove)
        {
            var item = new Models.NotificationsActionItem
            {
                Action = isApprove,
                LeaseID = LeaseId
            };
            var response = await DataAccess.WebAPI.PostItem<Models.NotificationsActionItem, string>(item, DataAccess.ApiEndPoint.PendingApprovalAction);

        }
    }
}
