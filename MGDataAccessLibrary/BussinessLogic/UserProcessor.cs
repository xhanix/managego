using System;
using System.Threading.Tasks;
using MGDataAccessLibrary.Models;

namespace MGDataAccessLibrary.BussinessLogic
{
    public static class UserProcessor
    {
        public static Action<Models.LoginResponse> onRefreshedToken;

        public static async Task<Models.LoginResponse> Login(string userName, string password, Action<Models.LoginResponse> onNewLoginDataAvailable)
        {
            onRefreshedToken = onNewLoginDataAvailable;
            /*
            #if DEBUG
                userName = "pmc@mobile.test";
                password = "Aa1111";
            #endif
            */

#if DEBUG
            userName = "Waltz11211@gmail.com";//"xhanix@me.com";
            password = "MGwaltz311";//"Hani123";
#endif


            var request = new Models.LoginRequest { Login = userName, Password = password };
            var res = await DataAccess.WebAPI.PostForm<Models.LoginRequest, Models.LoginResponse>(request, DataAccess.ApiEndPoint.authorize, null);
            DataAccess.WebAPI.SetAuthToken(res.UserInfo.AccessToken);
            DataAccess.WebAPI.SetCredentials(request);
            return res;
        }

        public static async Task UpdateUser(SignedInUserInfo userDetails)
        {
            var res = await DataAccess.WebAPI.PostItem<Models.SignedInUserInfo, string>(userDetails, DataAccess.ApiEndPoint.UserSettings, null);
        }

        public static async Task<LoginResponse> LoginWithExistingCredentials()
        {
            return await DataAccess.WebAPI.RefreshAccessTokenWithtoken();
        }
    }
}
