// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

namespace Microsoft.WindowsAzure.Commands.Utilities.Common.HttpRecorder
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Linq;
    using System.Net;

    public class RecordEntry
    {
        public string RequestUri { get; set; }

        public string RequestMethod { get; set; }

        public string RequestBody { get; set; }

        public Dictionary<string, List<string>> RequestHeaders { get; set; }

        public string ResponseBody { get; set; }

        public Dictionary<string, List<string>> ResponseHeaders { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public RecordEntry()
        {

        }

        public RecordEntry(HttpResponseMessage response)
        {
            HttpRequestMessage request = response.RequestMessage;
            RequestUri = request.RequestUri.ToString();
            RequestMethod = request.Method.Method;
            RequestBody = request.Content == null ? string.Empty : request.Content.ReadAsStringAsync().Result;
            RequestHeaders = new Dictionary<string, List<string>>();
            request.Headers.ForEach(h => RequestHeaders.Add(h.Key, h.Value.ToList()));
            ResponseBody = response.Content == null ? string.Empty : response.Content.ReadAsStringAsync().Result;
            ResponseHeaders = new Dictionary<string, List<string>>();
            response.Headers.ForEach(h => ResponseHeaders.Add(h.Key, h.Value.ToList()));
            StatusCode = response.StatusCode;
        }

        private static string GetKey(string httpMethod, string requestUri)
        {
            return string.Format("{0} {1}", httpMethod, requestUri);
        }

        public static string GetKey(RecordEntry recordEntry)
        {
            return GetKey(recordEntry.RequestMethod, recordEntry.RequestUri);
        }

        public static string GetKey(HttpResponseMessage response)
        {
            return GetKey(response.RequestMessage);
        }

        public static string GetKey(HttpRequestMessage request)
        {
            return GetKey(request.Method.Method, request.RequestUri.ToString());
        }

        public HttpResponseMessage GetResponse()
        {
            HttpResponseMessage response = new HttpResponseMessage();
            response.StatusCode = StatusCode;
            ResponseHeaders.ForEach(h => response.Headers.Add(h.Key, h.Value));
            response.Content = new StringContent(ResponseBody);

            return response;
        }
    }
}
