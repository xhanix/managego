using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MGDataAccessLibrary
{
    public static class Extensions
    {
        public static async Task<T> ReadAsApiResponseForType<T>(this HttpContent content, string requestBody, string nodeName = null)
        {
            var responseString = await content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
                return default;
            var j = JsonConvert.DeserializeObject<Models.BaseApiResponse<T>>(responseString);
            if (j.Status == Models.ResponseStatus.Error)
            {
                throw new Exception("Server error message: (" + j.ErrorMessage + ") App request: (" + requestBody + ")");
            }
            return j.Result;
        }

        public static async Task<(T, string)> ReadAsApiV3ResponseForType<T>(this HttpContent content, string requestBody, string nodeName = null)
        {
            var responseString = await content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
                return default;
            var j = JsonConvert.DeserializeObject<Models.BaseApiV3Response<T>>(responseString);
            return (j.Data, j.AccessToken);
        }

        public static async Task<T> ReadAsObjectAsync<T>(this HttpContent content, string nodeName = null)
        {
            var responseString = await content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
                return default;
            if (typeof(T).GetInterface(nameof(IList)) != null)
            {
                var responseObject = JArray.Parse(responseString);
                if (!string.IsNullOrWhiteSpace(nodeName))
                {
                    var token = responseObject[nodeName];
                    return token.ToObject<T>();
                }
                return responseObject.ToObject<T>();
            }
            else
            {
                var responseObject = JObject.Parse(responseString);
                if (!string.IsNullOrWhiteSpace(nodeName))
                {
                    var token = responseObject[nodeName];
                    return token.ToObject<T>();
                }
                return responseObject.ToObject<T>();
            }
        }

        public static IDictionary<string, string> ToKeyValue(this object metaToken)
        {
            if (metaToken == null)
            {
                return null;
            }

            JToken token = metaToken as JToken;
            if (token == null)
            {
                return ToKeyValue(JObject.FromObject(metaToken));
            }

            if (token.HasValues)
            {
                var contentData = new Dictionary<string, string>();
                foreach (var child in token.Children().ToList())
                {
                    var childContent = child.ToKeyValue();
                    if (childContent != null)
                    {
                        contentData = contentData.Concat(childContent)
                                                 .ToDictionary(k => k.Key, v => v.Value);
                    }
                }

                return contentData;
            }

            var jValue = token as JValue;
            if (jValue?.Value == null)
            {
                return null;
            }

            var value = jValue?.Type == JTokenType.Date ?
                            jValue?.ToString("o", CultureInfo.InvariantCulture) :
                            jValue?.ToString(CultureInfo.InvariantCulture);

            return new Dictionary<string, string> { { token.Path, value } };
        }

        public static string GetQueryString(this object obj)
        {
            var properties = from p in obj.GetType().GetProperties()
                             where p.GetValue(obj, null) != null
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(obj, null).ToString());

            return String.Join("&", properties.ToArray());
        }
    }

}
