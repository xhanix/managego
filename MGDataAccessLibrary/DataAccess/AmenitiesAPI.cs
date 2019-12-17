using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MGDataAccessLibrary.DataAccess
{
    public static class AmenitiesAPI
    {

#if !DEBUG
        private const string BaseUrl = "https://ploop.dynamo-ny.com/apiCore/api/pmc/v3/";
#else
        private const string BaseUrl = "https://portal.managego.com/apiCore/api/pmc/v3/";
#endif
        private static string tokenResponse = string.Empty;


        public static async Task<T> GetItems<T>(string pathWithParameters)
        {

            using var response = await WebAPI.WebClient.GetAsync(BaseUrl + pathWithParameters);
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    throw new HttpRequestException(result, new Exception("404"));
                throw new HttpRequestException(result);
            }
            else
            {
                var result = await response.Content.ReadAsApiV3ResponseForType<T>(requestBody: pathWithParameters);
                if (!string.IsNullOrWhiteSpace(result.Item2) && result.Item2 != tokenResponse)
                {
                    tokenResponse = result.Item2;
                    WebAPI.SetAuthToken(result.Item2);
                }

                return result.Item1;
            }
        }

        internal static async Task PostItem<T>(string path, T request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await WebAPI.WebClient.PostAsync(BaseUrl + path, content);
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception(result);
            }
        }

        internal static async Task PatchItem(string path)
        {
            var method = new HttpMethod("PATCH");
            var request = new HttpRequestMessage(method, BaseUrl + path);
            using var response = await WebAPI.WebClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception(result);
            }
        }
    }
}
