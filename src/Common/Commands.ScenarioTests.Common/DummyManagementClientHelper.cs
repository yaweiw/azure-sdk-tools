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
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using Microsoft.WindowsAzure.Common;

namespace Microsoft.WindowsAzure.Commands.ScenarioTest
{
    public class DummyManagementClientHelper : IManagementClientHelper
    {
        private readonly bool throwWhenNotAvailable;
        public List<object> ManagementClients { get; private set; }

        public DummyManagementClientHelper(IEnumerable<object> clients, bool throwIfClientNotSpecified = true)
        {
            ManagementClients = clients.ToList();
            throwWhenNotAvailable = throwIfClientNotSpecified;
        }

        public TClient CreateClient<TClient>(bool addRestLogHandler, EventHandler<ClientCreatedArgs> clientCreatedHandler,
            params object[] parameters) where TClient : ServiceClient<TClient>
        {
            TClient client = ManagementClients.FirstOrDefault(o => o is TClient) as TClient;
            if (client == null)
            {
                if (throwWhenNotAvailable)
                {
                    throw new ArgumentException(
                        string.Format("TestManagementClientHelper class wasn't initialized with the {0} client.",
                            typeof (TClient).Name));
                }
                else
                {
                    var realHelper = new ManagementClientHelper();
                    var realClient = realHelper.CreateClient<TClient>(addRestLogHandler, clientCreatedHandler, parameters);
                    realClient.WithHandler(HttpMockServer.CreateInstance());
                    return realClient;
                }
            }

            return client;
        }

        public HttpClient CreateHttpClient(string serviceUrl, ICredentials credentials)
        {
            if (serviceUrl == null)
            {
                throw new ArgumentNullException("serviceUrl");
            }
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            var realHelper = new ManagementClientHelper();
            var mockHandler = HttpMockServer.CreateInstance();
            var authenticationHandler = realHelper.CreateClientHandler(serviceUrl, credentials);
            mockHandler.InnerHandler = authenticationHandler;
            
            HttpClient client = new HttpClient(mockHandler)
            {
                BaseAddress = new Uri(serviceUrl),
                MaxResponseContentBufferSize = 30 * 1024 * 1024
            };

            client.DefaultRequestHeaders.Accept.Clear();

            return client;
        }

        public HttpClient CreateHttpClient(string serviceUrl, HttpMessageHandler effectiveHandler)
        {
            throw new NotImplementedException();
        }
    }
}
