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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Utilities.HttpRecorder;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Factories;
using Microsoft.WindowsAzure.Commands.Common.Models;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Common;

namespace Microsoft.WindowsAzure.Commands.ScenarioTest
{
    public class DummyManagementClientHelper : IClientFactory
    {
        private readonly bool throwWhenNotAvailable;
        public event EventHandler<ClientCreatedArgs> OnClientCreated;

        public List<object> ManagementClients { get; private set; }

        public DummyManagementClientHelper(IEnumerable<object> clients, bool throwIfClientNotSpecified = true)
        {
            ManagementClients = clients.ToList();
            throwWhenNotAvailable = throwIfClientNotSpecified;
        }

        public TClient CreateClient<TClient>(AzureSubscription subscription, AzureEnvironment.Endpoint endpoint) where TClient : ServiceClient<TClient>
        {
            SubscriptionCloudCredentials creds = new TokenCloudCredentials(subscription.Id.ToString(), "fake_token");
            if (HttpMockServer.GetCurrentMode() != HttpRecorderMode.Playback)
            {
                creds = AzurePowerShell.AuthenticationFactory.GetSubscriptionCloudCredentials(subscription.Id);    
            }
            
            Uri endpointUri = AzurePowerShell.Profile.GetEndpoint(subscription, endpoint);
            return CreateClient<TClient>(creds, endpointUri);
        }

        public TClient CreateClient<TClient>(params object[] parameters) where TClient : ServiceClient<TClient>
        {
            TClient client = ManagementClients.FirstOrDefault(o => o is TClient) as TClient;
            if (client == null)
            {
                if (throwWhenNotAvailable)
                {
                    throw new ArgumentException(
                        string.Format("TestManagementClientHelper class wasn't initialized with the {0} client.",
                            typeof(TClient).Name));
                }
                else
                {
                    var realHelper = new ClientFactory();
                    var realClient = realHelper.CreateClient<TClient>(parameters);
                    realClient.WithHandler(HttpMockServer.CreateInstance());
                    return realClient;
                }
            }

            return client;
        }

        public HttpClient CreateHttpClient(string serviceUrl, ICredentials credentials)
        {
            return CreateHttpClient(serviceUrl, ClientFactory.CreateHttpClientHandler(serviceUrl, credentials));
        }

        public HttpClient CreateHttpClient(string serviceUrl, HttpMessageHandler effectiveHandler)
        {
            if (serviceUrl == null)
            {
                throw new ArgumentNullException("serviceUrl");
            }
            if (effectiveHandler == null)
            {
                throw new ArgumentNullException("effectiveHandler");
            }
            var mockHandler = HttpMockServer.CreateInstance();
            mockHandler.InnerHandler = effectiveHandler;

            HttpClient client = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri(serviceUrl),
                MaxResponseContentBufferSize = 30 * 1024 * 1024
            };

            client.DefaultRequestHeaders.Accept.Clear();

            return client;
        }
    }
}
