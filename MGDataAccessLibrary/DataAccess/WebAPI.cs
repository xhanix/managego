using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MGDataAccessLibrary.DataAccess
{
    internal static class WebAPI
    {
        private static HttpClient WebClient { get; set; }
#if DEBUG
        private const string BaseUrl = "https://ploop.dynamo-ny.com/api/pmc_v2/";
#else
        private const string BaseUrl = "https://portal.managego.com/api/pmc_v2/";
#endif
        static WebAPI()
        {

            WebClient = new HttpClient { BaseAddress = new Uri(BaseUrl) };

        }

        internal static async Task<T2> PostItem<T1, T2>(T1 item, ApiEndPoint enpoint, string subpath = null)
        {
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
                    var result = await response.Content.ReadAsObjectAsync<Models.BaseApiResponse<T2>>();
                    if (result.Status == Models.ResponseStatus.Error)
                        throw new Exception(result.ErrorMessage);
                    return result.Result;
                }
            }
        }

        internal static void SetAuthToken(string accessToken)
        {
            WebClient.DefaultRequestHeaders.Remove("AccessToken");
            WebClient.DefaultRequestHeaders.Add("AccessToken", accessToken);
        }

        internal static async Task<T> PostRequest<T>(ApiEndPoint enpoint, string subpath = null)
        {
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
                    var result = await response.Content.ReadAsObjectAsync<Models.BaseApiResponse<T>>();
                    if (result.Status == Models.ResponseStatus.Error)
                        throw new Exception(result.ErrorMessage);
                    return result.Result;
                }
            }
        }

        internal static async Task<string> Get(string url)
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

        internal static async Task<T2> PostForm<T1, T2>(T1 item, ApiEndPoint enpoint, string subpath = null)
        {
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
