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
using System.Net.Http;
using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Common;

namespace Microsoft.WindowsAzure.Commands.Utilities.Common
{
    public class ManagementClientHelper : IManagementClientHelper
    {
        public TClient CreateClient<TClient>(bool addRestLogHandler, EventHandler<ClientCreatedArgs> clientCreatedHandler, 
            params object[] parameters) where TClient : ServiceClient<TClient>
        {
            List<Type> types = new List<Type>();
            foreach (object obj in parameters)
            {
                types.Add(obj.GetType());
            }

            var constructor = typeof(TClient).GetConstructor(types.ToArray());

            if (constructor == null)
            {
                throw new InvalidOperationException(string.Format(Resources.InvalidManagementClientType, typeof(TClient).Name));
            }

            TClient client = (TClient)constructor.Invoke(parameters);
            client.UserAgent.Add(ApiConstants.UserAgentValue);
            if (clientCreatedHandler != null)
            {
                ClientCreatedArgs args = new ClientCreatedArgs { CreatedClient = client, ClientType = typeof(TClient) };
                clientCreatedHandler(this, args);
                client = (TClient)args.CreatedClient;
            }

            if (addRestLogHandler)
            {
                // Add the logging handler
                var withHandlerMethod = typeof(TClient).GetMethod("WithHandler", new[] { typeof(DelegatingHandler) });
                TClient finalClient =
                    (TClient)withHandlerMethod.Invoke(client, new object[] { new HttpRestCallLogger() });
                client.Dispose();

                return finalClient;
            }
            else
            {
                return client;
            }
        }
    }
}
