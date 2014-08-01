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

using Microsoft.WindowsAzure.Commands.Common.Interfaces;
using System;
using System.Linq;

namespace Microsoft.WindowsAzure.Commands.Common.Model
{
    public partial class AzureProfile
    {
        public Uri GetEndpoint(AzureSubscription subscription, AzureEnvironment.Endpoint endpoint)
        {
            AzureEnvironment env = GetEnvironment(subscription.Environment);
            Uri endpointUri = null;

            if (env != null)
            {
                string endpointString = env.GetEndpoint(endpoint);
                if (!string.IsNullOrEmpty(endpointString))
                {
                    endpointUri = new Uri(endpointString);
                }
            }

            return endpointUri;
        }

        public AzureEnvironment GetEnvironment(string name)
        {
            return Environments.FirstOrDefault(e => e.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase));
        }

        public AzureSubscription GetSubscription(Guid? id)
        {
            if (id.HasValue)
            {
                return Subscriptions.FirstOrDefault(s => Guid.Equals(id.Value, s.Id));
            }

            return null;
        }

        private void Load(IFileStore store)
        {
            // Here we need to detect the version of the profile file and parse it appropriately
            //throw new NotImplementedException();
        }
    }
}
