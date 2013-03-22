

namespace Microsoft.WindowsAzure.Management.Utilities.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Newtonsoft.Json;

    public static class HttpClientExtensions
    {
        public static T GetJson<T>(this HttpClient client, string requestUri, Action<string> Logger)
        {
            client.DefaultRequestHeaders.UserAgent.Add(ApiConstants.UserAgentValue);
            if (Logger != null)
            {
                Logger(General.GetHttpRequestLog(
                    HttpMethod.Get.Method,
                    client.BaseAddress + requestUri,
                    client.DefaultRequestHeaders,
                    string.Empty));
            }
            
            HttpResponseMessage response = client.GetAsync(requestUri).Result;
            
            string content = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
            if (Logger != null)
            {
                Logger(General.GetHttpResponseLog(
                    response.StatusCode.ToString(),
                    response.Headers,
                    General.TryFormatJson(content)));
            }
            
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
