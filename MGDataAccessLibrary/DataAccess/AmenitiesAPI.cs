using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MGDataAccessLibrary.DataAccess
{
    public static class AmenitiesAPI
    {

#if DEBUG
        private const string BaseUrl = "https://ploop.dynamo-ny.com/apiCore/api/pmc/v3/";
#else
        private const string BaseUrl = "https://portal.managego.com/apiCore/api/pmc/v3/";
#endif


        public static async Task<T> GetItems<T>(string pathWithParameted)
        {

            using var response = await WebAPI.WebClient.GetAsync("https://ploop.dynamo-ny.com/apiCore/api/pmc/v3/bookings?pageSize=25&page=1");
            if (!response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                throw new Exception(result);
            }
            else
            {
                var result = await response.Content.ReadAsApiV3ResponseForType<T>(requestBody: pathWithParameted);
                return result;
            }
        }
    }
}
