﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ManageGo.Services
{
    public static class DataAccess
    {
        public static string BaseUrl = "https://ploop.dynamo-ny.com/api/pmc_v2/";
        static readonly HttpClient client = new HttpClient();
        internal static string AccessToken { get; private set; }
        private static DateTimeOffset TokenExpiry { get; set; } = DateTimeOffset.FromUnixTimeSeconds(0);

        public static async Task<object> Login()
        {
            Dictionary<string, string> credentials = new Dictionary<string, string>
            {
                { "login", "pmc@mobile.test" },
                { "password", "111111" }
            };
            var content = new FormUrlEncodedContent(credentials);
            var response = await client.PostAsync(BaseUrl + APIpaths.authorize.ToString(), content);

            var responseString = await response.Content.ReadAsStringAsync();
            //create jobject from response
            var responseObject = JObject.Parse(responseString);
            //get the result jtoken
            if (responseObject.TryGetValue("Result", out JToken result))
            {
                //result is a dictionary of jobjects
                var jResult = result.ToObject<Dictionary<string, JObject>>();
                //get user-info
                jResult.TryGetValue(APIkeys.UserInfo.ToString(), out JObject userInfo);
                jResult.TryGetValue(APIkeys.PMCInfo.ToString(), out JObject pmcInfo);
                //user info is a dictionary
                var dic = userInfo.ToObject<Dictionary<string, string>>();
                var pmcDic = pmcInfo.ToObject<Dictionary<string, string>>();
                //get access token
                if (dic.TryGetValue(APIkeys.AccessToken.ToString(), out string token))
                {
                    AccessToken = token;
                    TokenExpiry = DateTimeOffset.Now.AddMinutes(60);
                    client.DefaultRequestHeaders.Remove(APIkeys.AccessToken.ToString());
                    client.DefaultRequestHeaders.Add(APIkeys.AccessToken.ToString(), AccessToken);
                }
                if (dic.TryGetValue(APIkeys.UserFirstName.ToString(), out string firstName)
                    && dic.TryGetValue(APIkeys.UserLastName.ToString(), out string lastName))
                {
                    App.UserName = firstName + " " + lastName;
                }
                if (pmcDic.TryGetValue(APIkeys.PMCName.ToString(), out string pmcName))
                {
                    App.PMCName = pmcName;
                }
                //get Permissions
                jResult.TryGetValue(APIkeys.Permissions.ToString(), out JObject permisions);
                var permissionDic = permisions.ToObject<Dictionary<string, object>>();
                if (permissionDic.TryGetValue(APIkeys.AccessToPayments.ToString(), out object ap)
                    && ap is bool && (bool)ap)
                {
                    App.UserPermissions = UserPermissions.CanAccessPayments;
                }
                if (permissionDic.TryGetValue(APIkeys.AccessToMaintenance.ToString(), out object at)
                    && at is bool && (bool)at)
                {
                    App.UserPermissions |= UserPermissions.CanAccessTickets;
                }
            }
            Console.WriteLine(responseString);
            return responseString;
        }

        #region MAINTENANCE OBJECT - CATEGORIES
        public static async Task GetAllCategoriesAndTags()
        {
            var response = await client.PostAsync(BaseUrl + APIpaths.MaintenanceObjects.ToString(), null);
            var responseString = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(responseString);
            var result = obj.GetValue("Result");

            if (result.ToObject<Dictionary<string, object>>().TryGetValue("Categories", out object list)
                && list is JContainer && result.ToObject<Dictionary<string, object>>().TryGetValue("Tags", out object tagsList)
                && result.ToObject<Dictionary<string, object>>().TryGetValue("ExternalContacts", out object contactList))
            {
                App.Categories = ((JContainer)list).ToObject<List<Categories>>();
                App.Tags = ((JContainer)tagsList).ToObject<List<Tags>>();
                App.ExternalContacts = ((JContainer)contactList).ToObject<List<ExternalContact>>();
            }
        }

        #endregion

        #region DASHBOARD
        public static async Task<Dictionary<string, string>> GetDashboardAsync()
        {
            if (TokenExpiry < DateTimeOffset.Now)
                await Login();
            var response = await client.PostAsync(BaseUrl + APIpaths.dashboard.ToString(), null);
            return await GetResultDictionaryFromResponse(response);
        }
        #endregion

        #region BUILDINGS
        public static async Task GetBuildings()
        {
            var param = new Dictionary<string, string> { { "page", "0" } };
            var response = await client.PostAsync(BaseUrl + APIpaths.buildings.ToString(), new FormUrlEncodedContent(param));
            var responseString = await response.Content.ReadAsStringAsync();
            var dic = JObject.Parse(responseString);
            if (dic.TryGetValue("Result", out JToken list))
            {
                App.Buildings = list.ToObject<List<Building>>();
            }
        }
        #endregion

        #region USERS
        public static async Task GetAllUsers()
        {
            var response = await client.PostAsync(BaseUrl + APIpaths.Users.ToString(), null);
            var responseString = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(responseString);
            var result = obj.GetValue("Result");
            App.Users = result.ToObject<List<User>>();
        }
        #endregion

        #region TICKETS

        public static async Task<List<MaintenanceTicket>> GetTicketsAsync(Dictionary<string, object> filters)
        {
            if (TokenExpiry < DateTimeOffset.Now)
                await Login();
            var jsonString = JsonConvert.SerializeObject(filters);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");//new FormUrlEncodedContent(filters);
            var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + APIpaths.tickets.ToString())
            {
                Content = content
            };
            var response = await client.SendAsync(msg);

            return await GetTicketsFromResponse(response);
        }


        public static async Task<int> SendNewCommentAsync(Dictionary<string, object> parametes)
        {
            if (TokenExpiry < DateTimeOffset.Now)
                await Login();
            var jsonString = JsonConvert.SerializeObject(parametes);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");//new FormUrlEncodedContent(filters);
            var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + APIpaths.TicketNewComment.ToString())
            {
                Content = content
            };
            var response = await client.SendAsync(msg);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseString);
            return responseObject.TryGetValue("Result", out JToken result) ? (int)result["CommentID"] : 0;
        }

        public static async Task<int> SendNewWorkOurderAsync(Dictionary<string, object> parameters)
        {
            if (TokenExpiry < DateTimeOffset.Now)
                await Login();
            var jsonString = JsonConvert.SerializeObject(parameters);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");//new FormUrlEncodedContent(filters);
            var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + APIpaths.CreateWorkOrder.ToString())
            {
                Content = content
            };
            var response = await client.SendAsync(msg);
            var responseString = await response.Content.ReadAsStringAsync();
            JObject responseObject = JObject.Parse(responseString);
            return responseObject.TryGetValue("Result", out JToken result) ? (int)result["WorkOrderID"] : 0;
        }

        public static async Task<byte[]> GetCommentFile(Dictionary<string, object> parametes)
        {
            if (TokenExpiry < DateTimeOffset.Now)
                await Login();
            var jsonString = JsonConvert.SerializeObject(parametes);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");//new FormUrlEncodedContent(filters);
            var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + APIpaths.GetTicketFile.ToString())
            {
                Content = content
            };
            var response = await client.SendAsync(msg);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseString);
            if (responseObject.TryGetValue("Result", out JToken result))
            {
                var array = result.ToObject<byte[]>();
                return array;
            }
            throw new Exception("Downloaded data not valid");
        }


        public static async Task UploadFile(File file)
        {
            if (TokenExpiry < DateTimeOffset.Now)
                await Login();
            var parameters = new Dictionary<string, object> {
                {"CommentID",file.ParentComment},
                {"FileName", file.Name},
                {"File", file.Content},
                {"IsCompleted", false}
            };
            var jsonString = JsonConvert.SerializeObject(parameters);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");//new FormUrlEncodedContent(filters);
            var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + APIpaths.CommentNewFile.ToString())
            {
                Content = content
            };
            var response = await client.SendAsync(msg);
            var responseString = await response.Content.ReadAsStringAsync();
        }

        public static async Task UploadCompleted(int commentId)
        {

            if (TokenExpiry < DateTimeOffset.Now)
                await Login();
            var parameters = new Dictionary<string, object> {
                {"CommentID",commentId},
            };
            var jsonString = JsonConvert.SerializeObject(parameters);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");//new FormUrlEncodedContent(filters);
            var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + APIpaths.CommentFilesCompleted.ToString())
            {
                Content = content
            };
            var response = await client.SendAsync(msg);
        }
        #endregion

        static async Task<Dictionary<string, string>> GetResultDictionaryFromResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseString);
            if (responseObject.TryGetValue("Result", out JToken token))
            {
                try
                {
                    var dic = token.ToObject<Dictionary<string, string>>();
                    return dic;
                }
                catch //result property types have changed in the backend
                {
                    return new Dictionary<string, string>();
                }

            }
            else
            {
                throw new Exception("Unable to get the Result token from the HTTP response content");
            }
        }

        internal static async Task<TicketDetails> GetTicketDetails(int ticketId)
        {
            var content = new StringContent($"{{TicketID: {ticketId}}}", Encoding.UTF8, "application/json");//new FormUrlEncodedContent(filters);
            var msg = new HttpRequestMessage(HttpMethod.Post, BaseUrl + APIpaths.TicketsDetails.ToString())
            {
                Content = content
            };
            var response = await client.SendAsync(msg);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseString);
            if (responseObject.TryGetValue("Result", out JToken token))
            {
                Console.WriteLine(token);
                var ticketDetails = token.ToObject<TicketDetails>();
                return ticketDetails;
            }
            throw new Exception("Unable to get the Result token from the HTTP response content");
        }

        static async Task<List<MaintenanceTicket>> GetTicketsFromResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseString);
            if (responseObject.TryGetValue("Result", out JToken token))
            {
                try
                {
                    var dic = token.ToObject<List<MaintenanceTicket>>();
                    return dic;
                }
                catch //result property types have changed in the backend
                {
                    return new List<MaintenanceTicket>();
                }
            }
            else
            {
                throw new Exception("Unable to get the Result token from the HTTP response content");
            }
        }


    }
}

