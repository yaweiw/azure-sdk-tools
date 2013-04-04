// ----------------------------------------------------------------------------------
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

namespace Microsoft.WindowsAzure.Management.Utilities.Subscriptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Common;
    using Contract;

    public class SubscriptionClient : ISubscriptionClient
    {
        private readonly HttpClient httpClient;
        private readonly SubscriptionData subscription;

        private static readonly XNamespace azureNS = "http://schemas.microsoft.com/windowsazure";

        public SubscriptionClient(SubscriptionData subscription)
            : this(subscription, CreateDefaultFinalHandler(subscription))
        {
        }

        public SubscriptionClient(SubscriptionData subscription, HttpMessageHandler finalHandler)
        {
            this.httpClient = CreateHttpClient(finalHandler);
            this.subscription = subscription;
            httpClient.BaseAddress = new Uri(subscription.ServiceEndpoint);
        }

        public Task<IEnumerable<ProviderResource>> ListResourcesAsync(IEnumerable<string> knownResourceTypes)
        {
            string resourceList = string.Join(",", knownResourceTypes);
            var path = string.Format("/{0}/services/?servicelist={1}&expandlist=ServiceResource",
                subscription.SubscriptionId, resourceList);

            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(path, UriKind.Relative));
            request.Headers.Add("x-ms-version", "2012-08-01");
            request.Headers.Add("accept", "application/xml");

            Task<HttpResponseMessage> responseTask = httpClient.SendAsync(request);
            var getContentTask = ProcessListResourcesResponse(responseTask);
            var deserializeTask = getContentTask.ContinueWith(st => DeserializeListResourcesResponse(st));
            return deserializeTask;
        }

        private Task<string> ProcessListResourcesResponse(Task<HttpResponseMessage> responseMessage)
        {
            var response = responseMessage.Result;
            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync();
        }

        private IEnumerable<ProviderResource> DeserializeListResourcesResponse(Task<string> contentTask)
        {
            string content = contentTask.Result;

            XDocument doc = XDocument.Parse(content);
            return doc.Root.Descendants(azureNS + "Service").Select(element =>
                new ProviderResource()
                {
                    State = (string) (element.Element(azureNS + "State")),
                    Type = (string) (element.Element(azureNS + "Type"))
                });
        }

        private static HttpClient CreateHttpClient(HttpMessageHandler handler)
        {
            return new HttpClient(handler);
        }

        private static HttpMessageHandler CreateDefaultFinalHandler(SubscriptionData subscription)
        {
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(subscription.Certificate);
            return handler;
        }
    }
}