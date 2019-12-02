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

#if DEBUG
            //access to everything
            userName = "pmc@mobile.test";
            password = "Aa1111";
            // userName = "aidendm@managego.com";
            // password = "Qwerasdf1";

            //no access to bank account
            // userName = "aidenclk11@gmail.com";
            // password = "Qwerasdf1";

            //no access to tenants
            // userName = "apptest@test.com";
            // password = "Qwerasdf1";

            //no access to payments
            // userName = "apptest2@test.com";
            // password = "Qwerasdf1";

            //no access to Maintenance
            //userName = "apptest3@test.com";
            //password = "Qwerasdf1";

            //pmc production
            //userName = "aidenclk1@gmail.com";
            //password = "Qwerasdf1";

#endif

            /*
            #if DEBUG
                        userName = "Waltz11211@gmail.com";//"xhanix@me.com";
                        password = "MGwaltz311";//"Hani123";
            #endif*/


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
