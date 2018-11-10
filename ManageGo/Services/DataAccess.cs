using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ManageGo.Services
{
    public static class DataAccess
    {
        public static string BaseUrl = "https://ploop.dynamo-ny.com/api/pmc_v2/";
        static readonly HttpClient client = new HttpClient();
        static string AccessToken { get; set; }

        public static async Task<object> Login()
        {
            Dictionary<string, string> credentials = new Dictionary<string, string>
            {
                { "login", "xhanix@me.com" },
                { "password", "123123" }
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
                //user info is a dictionary
                var dic = userInfo.ToObject<Dictionary<string, string>>();
                //get access token
                if (dic.TryGetValue(APIkeys.AccessToken.ToString(), out string token))
                {
                    AccessToken = token;
                    client.DefaultRequestHeaders.Add(APIkeys.AccessToken.ToString(), AccessToken);
                }
                if (dic.TryGetValue(APIkeys.UserFirstName.ToString(), out string firstName)
                    && dic.TryGetValue(APIkeys.UserLastName.ToString(), out string lastName))
                {
                    App.UserName = firstName + " " + lastName;
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



        public static async Task<Dictionary<string, string>> GetDashboardAsync()
        {
            await Login();
            var response = await client.PostAsync(BaseUrl + APIpaths.dashboard.ToString(), null);
            return await GetResultDictionaryFromResponse(response);
        }

        static async Task<Dictionary<string, string>> GetResultDictionaryFromResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JObject.Parse(responseString);
            if (responseObject.TryGetValue("Result", out JToken token))
            {
                var dic = token.ToObject<Dictionary<string, string>>();
                return dic;
            }
            else
            {
                throw new Exception("Unable to get the Result token from the HTTP response content");
            }
        }
    }
}

