using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MGDataAccessLibrary.Models;
using Newtonsoft.Json;

namespace MGDataAccessLibrary.DataAccess
{
    public static class WebAPI
    {
        private static HttpClient WebClient { get; set; }
#if DEBUG
        private const string BaseUrl = "https://ploop.dynamo-ny.com/api/pmc_v2/";
#else
        private const string BaseUrl = "https://portal.managego.com/api/pmc_v2/";
#endif
        private static DateTimeOffset TokenExpiry { get; set; } = default;
        internal static string UserName { get; set; }
        internal static string Password { get; set; }
        public static string RefreshToken { get; private set; }

        static WebAPI()
        {
            WebClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };
        }



        internal static async Task<LoginResponse> RefreshAccessTokenWithtoken()
        {
            var request = new Models.LoginRequest
            {
                AccessToken = RefreshToken
            };
            var res = await PostForm<Models.LoginRequest, Models.LoginResponse>(request, DataAccess.ApiEndPoint.authorize, null);
            SetAuthToken(res.UserInfo.AccessToken);
            return res;
        }

        public static async Task<T2> PostItem<T1, T2>(T1 item, ApiEndPoint enpoint, string subpath = null)
        {
            if (TokenExpiry != default && TokenExpiry < DateTimeOffset.Now)
            {
                Console.WriteLine("**** Re -logged in ****");
                TokenExpiry = default;
                await RefreshAccessTokenWithtoken();
            }
            var myContent = JsonConvert.SerializeObject(item);
            var content = new StringContent(myContent, Encoding.UTF8, "application/json");
            var path = enpoint.ToString();
            if (!string.IsNullOrWhiteSpace(subpath))
                path = path + @"/" + subpath;
            using (var response = await WebClient.PostAsync(path, content))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    throw new Exception(result);
                }
                else
                {
                    var result = await response.Content.ReadAsApiResponseForType<T2>(requestBody: myContent);
                    return result;
                }
            }
        }

        internal static void SetCredentials(LoginRequest request)
        {
            UserName = request.Login;
            Password = request.Password;
        }

        public static void SetAuthToken(string accessToken)
        {
            WebClient.DefaultRequestHeaders.Remove("AccessToken");
            WebClient.DefaultRequestHeaders.Add("AccessToken", accessToken);
            RefreshToken = accessToken;
#if DEBUG
            TokenExpiry = DateTimeOffset.Now.AddMinutes(1);
#else
            TokenExpiry = DateTimeOffset.Now.AddMinutes(50);
#endif
        }



        public static async Task<T> PostRequest<T>(ApiEndPoint enpoint, string subpath = null)
        {
            if (TokenExpiry != default && TokenExpiry < DateTimeOffset.Now)
            {
                TokenExpiry = default;
                Console.WriteLine("**** Re -logged in ****");
                await RefreshAccessTokenWithtoken();
            }
            var path = enpoint.ToString();
            if (!string.IsNullOrWhiteSpace(subpath))
                path = path + @"/" + subpath;
            using (var response = await WebClient.PostAsync(path, null))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    throw new Exception(result);
                }
                else
                {
                    var result = await response.Content.ReadAsApiResponseForType<T>(requestBody: path);
                    return result;
                }
            }
        }

        public static async Task<HttpResponseMessage> SendFeedBack(string feedback)
        {

            var content = new StringContent(feedback
                , encoding: Encoding.UTF8,
                mediaType: "application/json");
            return await WebClient.PostAsync("https://inject.socketlabs.com/api/v1/email", content);
        }

        public static async Task<string> Get(string url)
        {
            try
            {
                CacheControlHeaderValue cacheControl = new CacheControlHeaderValue();
                cacheControl.NoCache = true;
                cacheControl.MustRevalidate = true;
                WebClient.DefaultRequestHeaders.CacheControl = cacheControl;
                var result = await WebClient.GetStringAsync(url);
                // WebClient.DefaultRequestHeaders.CacheControl.NoCache = false;
                return result;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static async Task<T2> PostForm<T1, T2>(T1 item, ApiEndPoint enpoint, string subpath = null)
        {
            if (TokenExpiry != default && TokenExpiry < DateTimeOffset.Now)
            {
                Console.WriteLine("**** Re -logged in ****");
                TokenExpiry = default;
                await RefreshAccessTokenWithtoken();
            }
            var myContent = new FormUrlEncodedContent(item.ToKeyValue());
            var path = enpoint.ToString();
            if (!string.IsNullOrWhiteSpace(subpath))
                path = path + @"/" + subpath;
            using (var response = await WebClient.PostAsync(path, myContent))
            {
                if (!response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    throw new Exception(result);
                }
                else
                {
                    var result = await response.Content.ReadAsObjectAsync<Models.BaseApiResponse<T2>>();
                    if (result.Status == Models.ResponseStatus.Error)
                        throw new Exception(result.ErrorMessage);
                    return result.Result;
                }
            }
        }
    }
}
