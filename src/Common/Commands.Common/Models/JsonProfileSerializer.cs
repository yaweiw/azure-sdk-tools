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
using Newtonsoft.Json;
using System.Linq;

namespace Microsoft.WindowsAzure.Commands.Common.Models
{
    public class JsonProfileSerializer : IProfileSerializer
    {
        public string Serialize(AzureProfile profile)
        {
            return JsonConvert.SerializeObject(new
            {
                Environments = profile.Environments.Values.ToList(),
                Subscriptions = profile.Subscriptions.Values.ToList()
            });
        }

        public void Deserialize(string contents, AzureProfile profile)
        {
            dynamic obj = JsonConvert.DeserializeObject(contents);

            foreach (AzureEnvironment env in obj.Environments)
            {
                profile.Environments[env.Name] = env;
            }

            foreach (AzureSubscription subscription in obj.Subscriptions)
            {
                profile.Subscriptions[subscription.Id] = subscription;
            }
        }
    }
}
