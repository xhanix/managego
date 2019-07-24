using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MGDataAccessLibrary
{
    public static class Extensions
    {
        public static async Task<Models.BaseApiResponse<T>> ReadAsApiResponseForType<T>(this HttpContent content, string nodeName = null)
        {
            var responseString = await content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseString))
                return default;
            var j = JObject.Parse(responseString);
            if (j["Status"].ToObject<Models.ResponseStatus>() == Models.ResponseStatus.Error)
            {
                return j.ToObject<Models.BaseApiResponse<T>>();
            }
            var _j = j["Result"].ToObject<T>();
            return new Models.BaseApiResponse<T>
            {
                Result = _j
            };
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

    }
}
