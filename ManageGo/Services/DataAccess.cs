using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ManageGo.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ManageGo.Services
{
    public static class DataAccess
    {
        public static async Task Login(string userName = null, string password = null)
        {
            var result = await MGDataAccessLibrary.BussinessLogic.UserProcessor.Login(userName, password);
            App.UserInfo = new SignedInUserInfo
            {
                AccessToken = result.UserInfo.AccessToken,
                UserEmailAddress = result.UserInfo.UserEmailAddress,
                UserFirstName = result.UserInfo.UserFirstName,
                UserLastName = result.UserInfo.UserLastName,
                TenantPushNotification = result.UserInfo.TenantPushNotification,
                MaintenancePushNotification = result.UserInfo.MaintenancePushNotification,
                PaymentPushNotification = result.UserInfo.PaymentPushNotification,
                PushNotification = result.UserInfo.PushNotification,
                UserID = result.UserInfo.UserID
            };

            DependencyService.Get<IGoogleCloudMessagingHelper>().SubscribeToTopic($"{result.UserInfo.UserID}");
            var perm = result.Permissions;
            //reset the permissions on log in
            App.UserPermissions = UserPermissions.None;
            if (perm.CanAccessPayments)
                App.UserPermissions |= UserPermissions.CanAccessPayments;
            if (perm.CanAccessMaintenanceTickets)
                App.UserPermissions |= UserPermissions.CanAccessTickets;
            if (perm.CanReplyPublicly)
                App.UserPermissions |= UserPermissions.CanReplyPublicly;
            if (perm.CanReplyInternally)
                App.UserPermissions |= UserPermissions.CanReplyInternally;
            if (perm.CanAccessTenants)
                App.UserPermissions |= UserPermissions.CanAccessTenants;
            if (perm.CanAddWorkordersAndEvents)
                App.UserPermissions |= UserPermissions.CanAddWorkordersAndEvents;
            if (perm.CanApproveNewTenantsUnits)
                App.UserPermissions |= UserPermissions.CanApproveNewTenantsUnits;
            if (perm.CanEditTicket)
                App.UserPermissions |= UserPermissions.CanEditTicketDetails;
        }


        public static async Task ResetPassword(string userName)
        => await MGDataAccessLibrary.DataAccess.WebAPI.PostItem<object, object>(new { PMCUserEmailAddress = userName }
            , MGDataAccessLibrary.DataAccess.ApiEndPoint.ResetPassword, null);



        #region MAINTENANCE OBJECT - CATEGORIES
        public static async Task GetAllCategoriesAndTags()
        {
            var response = await MGDataAccessLibrary.DataAccess.WebAPI
                   .PostRequest<MaintenanceObjects>(MGDataAccessLibrary.DataAccess.ApiEndPoint.MaintenanceObjects, null);
            App.Categories = response.Categories.ToList();
            App.Tags = response.Tags.ToList();
            App.ExternalContacts = response.ExternalContacts.ToList();
        }

        #endregion

        #region DASHBOARD
        public static async Task<DashboardResponseItem> GetDashboardAsync()
        => await MGDataAccessLibrary.DataAccess.WebAPI
                   .PostRequest<DashboardResponseItem>(MGDataAccessLibrary.DataAccess.ApiEndPoint.dashboard, null);


        #endregion

        public static async Task GetBankAccounts()
        {
            App.BankAccounts = await MGDataAccessLibrary.DataAccess.WebAPI
                  .PostRequest<List<Models.BankAccount>>(MGDataAccessLibrary.DataAccess.ApiEndPoint.BankAccounts, null);

        }

        #region PENDING NOTIFICATIONS
        public static async Task<List<PendingApprovalItem>> GetPendingNotifications()
            => await MGDataAccessLibrary.DataAccess.WebAPI
                .PostRequest<List<PendingApprovalItem>>(MGDataAccessLibrary.DataAccess.ApiEndPoint.PendingApprovals, null);

        #endregion


        #region BUILDINGS
        public static async Task GetBuildings()
        {
            App.Buildings = await MGDataAccessLibrary.DataAccess.WebAPI
                .PostItem<object, List<Building>>(new { page = 1 }, MGDataAccessLibrary.DataAccess.ApiEndPoint.buildings, null);
            App.Buildings?.Sort();
        }

        public static async Task<Building> GetBuildingDetails(int id)
            => await MGDataAccessLibrary.DataAccess.WebAPI
            .PostItem<object, Building>(new { BuildingID = id },
                MGDataAccessLibrary.DataAccess.ApiEndPoint.BuildingDetails, null);
        #endregion

        #region USERS
        public static async Task GetAllUsers()
            => App.Users = await MGDataAccessLibrary.DataAccess.WebAPI
            .PostRequest<List<User>>(MGDataAccessLibrary.DataAccess.ApiEndPoint.Users);


        internal static async Task UpdateUserInfo(Dictionary<string, object> filtersDictionary)
        => await MGDataAccessLibrary.DataAccess.WebAPI
            .PostItem<object, object>(filtersDictionary,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.UserSettings, null);


        internal static async Task<EventDatesResponseItem> GetEventsList(EventsDatesRequestItem item)
            => await MGDataAccessLibrary.DataAccess.WebAPI
            .PostItem<EventsDatesRequestItem, Models.EventDatesResponseItem>(item,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.EventsListDates, null);

        internal static async Task<List<Models.CalendarEvent>> GetEventsForDate(CalendarEventRequestItem eventRequestItem)
            => await MGDataAccessLibrary.DataAccess.WebAPI
            .PostItem<CalendarEventRequestItem, List<CalendarEvent>>(eventRequestItem,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.EventList, null);


        internal static async Task<IEnumerable<Tenant>> GetTenantsAsync(TenantRequestItem requestParameterItem)
            => await MGDataAccessLibrary.DataAccess.WebAPI
            .PostItem<TenantRequestItem, IEnumerable<Tenant>>(requestParameterItem,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.Tenants, null);
        #endregion

        #region TICKETS

        internal static async Task<IEnumerable<MaintenanceTicket>> GetTicketsAsync(TicketRequestItem filters)
            => await MGDataAccessLibrary.DataAccess.WebAPI
            .PostItem<TicketRequestItem, IEnumerable<MaintenanceTicket>>(filters,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.tickets, null);

        public static async Task UpdateTicket(Models.UpdateTicketRequestItem item)
            => await MGDataAccessLibrary.DataAccess.WebAPI
                .PostItem<object, object>(item,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.UpdateTicket, null);

        public static async Task<int> CreateTicket(MGDataAccessLibrary.Models.CreateTicketRequestItem item)
        => (await MGDataAccessLibrary.DataAccess.WebAPI
                .PostItem<object, CreateTicketResponse>(item,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.CreateTicket, null)).TicketID;


        public static async Task<int> SendNewCommentAsync(Dictionary<string, object> parameters)
        => (await MGDataAccessLibrary.DataAccess.WebAPI
                .PostItem<object, SendCommentResponse>(parameters,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.TicketNewComment, null)).CommentID;



        public static async Task<int> SendNewWorkOurderAsync(Models.CreateWorkorderRequestItem item)
            => (await MGDataAccessLibrary.DataAccess.WebAPI
                .PostItem<object, CreateWorkorderResponse>(item,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.CreateWorkOrder, null)).WorkOrderID;


        public static async Task<byte[]> GetCommentFile(Dictionary<string, object> parameters)
        => await MGDataAccessLibrary.DataAccess.WebAPI
                .PostItem<object, byte[]>(parameters,
                MGDataAccessLibrary.DataAccess.ApiEndPoint.GetTicketFile, null);




        public static async Task UploadFile(File file)
        {

            await MGDataAccessLibrary.DataAccess.WebAPI
                .PostItem<object, object>(new
                {
                    CommentID = file.ParentComment,
                    FileName = file.Name,
                    File = file.Content,
                    IsCompleted = false
                },
                MGDataAccessLibrary.DataAccess.ApiEndPoint.CommentNewFile, null);
        }

        public static async Task UploadCompleted(int commentId)
        {
            await MGDataAccessLibrary.DataAccess.WebAPI
                .PostItem<object, object>(new
                {
                    CommentID = commentId
                },
                MGDataAccessLibrary.DataAccess.ApiEndPoint.CommentNewFile, null);
        }
        #endregion


        #region Payments
        internal static async Task<IEnumerable<Models.Payment>> GetPaymentsAsync(PaymentsRequestItem filtersDictionary)
            => await MGDataAccessLibrary.DataAccess.WebAPI.PostItem<PaymentsRequestItem, IEnumerable<Models.Payment>>(filtersDictionary, MGDataAccessLibrary.DataAccess.ApiEndPoint.Payments, null);


        internal static async Task<List<Models.BankTransaction>> GetTransactionsAsync(TransactionsRequestItem requestParameters)
            => await MGDataAccessLibrary.DataAccess.WebAPI.PostItem<TransactionsRequestItem, List<BankTransaction>>(requestParameters, MGDataAccessLibrary.DataAccess.ApiEndPoint.BankTransactions, null);

        #endregion

        internal static async Task<TicketDetails> GetTicketDetails(int ticketId)
            => await MGDataAccessLibrary.DataAccess.WebAPI.PostItem<object, TicketDetails>(new { TicketID = ticketId }, MGDataAccessLibrary.DataAccess.ApiEndPoint.TicketsDetails, null);


    }
}

