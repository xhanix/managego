using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MGDataAccessLibrary.Models;

namespace MGDataAccessLibrary.BussinessLogic
{
    public static class UserProcessor
    {
        public static async Task<Models.LoginResponse> Login(string userName, string password)
        {
#if DEBUG
            userName = "pmc@mobile.test";
            password = "111111";
#endif
            var request = new Models.LoginRequest { Login = userName, Password = password };
            var res = await DataAccess.WebAPI.PostForm<Models.LoginRequest, Models.BaseApiResponse<Models.LoginResponse>>(request, DataAccess.ApiEndPoint.authorize, null);
            DataAccess.WebAPI.SetAuthToken(res.Result.UserInfo.AccessToken);
            return res.Result;
        }

        public static async Task UpdateUser(SignedInUserInfo userDetails)
        {
            var res = await DataAccess.WebAPI.PostForm<Models.SignedInUserInfo, Models.BaseApiResponse<string>>(userDetails, DataAccess.ApiEndPoint.UserSettings, null);
        }
    }
}
