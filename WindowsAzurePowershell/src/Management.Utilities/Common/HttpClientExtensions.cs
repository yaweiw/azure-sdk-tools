

namespace Microsoft.WindowsAzure.Management.Utilities.Common
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class HttpClientExtensions
    {
        private static void AddUserAgent(HttpClient client)
        {
            if (!client.DefaultRequestHeaders.UserAgent.Contains(ApiConstants.UserAgentValue))
            {
                client.DefaultRequestHeaders.UserAgent.Add(ApiConstants.UserAgentValue);
            }
        }

        private static void LogResponse(
            string statusCode,
            HttpResponseHeaders headers,
            string content,
            Action<string> Logger)
        {
            if (Logger != null)
            {
                Logger(General.GetHttpResponseLog(statusCode, headers, content));
            }
        }

        private static void LogRequest(
            string method,
            string requestUri,
            HttpRequestHeaders headers,
            string body,
            Action<string> Logger)
        {
            if (Logger != null)
            {
                Logger(General.GetHttpRequestLog(method, requestUri, headers, body));
            }
        }

        public static T GetJson<T>(this HttpClient client, string requestUri, Action<string> Logger)
        {
            AddUserAgent(client);
            LogRequest(
                HttpMethod.Get.Method,
                client.BaseAddress + requestUri,
                client.DefaultRequestHeaders,
                string.Empty,
                Logger);
            HttpResponseMessage response = client.GetAsync(requestUri).Result;
            string content = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
            LogResponse(response.StatusCode.ToString(), response.Headers, General.TryFormatJson(content), Logger);
            
            return JsonConvert.DeserializeObject<T>(content);
        }

        public static HttpResponseMessage PostAsJsonAsync(
            this HttpClient client,
            string requestUri,
            JObject json,
            Action<string> Logger)
        {
            AddUserAgent(client);

            LogRequest(
                HttpMethod.Post.Method,
                client.BaseAddress + requestUri,
                client.DefaultRequestHeaders,
                JsonConvert.SerializeObject(json, Formatting.Indented),
                Logger);
            HttpResponseMessage response = client.PostAsJsonAsync(requestUri, json).Result;
            string content = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
            LogResponse(
                response.StatusCode.ToString(),
                response.Headers,
                General.TryFormatJson(content),
                Logger);

            return response;
        }
    }
}
