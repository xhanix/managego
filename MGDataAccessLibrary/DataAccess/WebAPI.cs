using System;
using System.Net.Http;
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
                    var result = await response.Content.ReadAsObjectAsync<T2>();
                    return result;
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
                    var result = await response.Content.ReadAsObjectAsync<T>();
                    return result;
                }
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
                    var result = await response.Content.ReadAsObjectAsync<T2>();
                    return result;
                }
            }
        }


    }





}
