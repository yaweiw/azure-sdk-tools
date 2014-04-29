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

using System.Linq;
using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager;
using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager.Models;

namespace Microsoft.WindowsAzure.Commands.TrafficManager.Endpoint
{
    using System.Management.Automation;

    [Cmdlet(VerbsCommon.Remove, "AzureTrafficManagerEndpoint"), OutputType(typeof(IProfileWithDefinition))]
    public class RemoveAzureTrafficManagerEndpoint : TrafficManagerConfigurationBaseCmdlet
    {
        [Parameter(Mandatory = true,
            ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string DomainName { get; set; }

        public override void ExecuteCmdlet()
        {
            ProfileWithDefinition profile = TrafficManagerProfile.GetInstance();
            if (!profile.Endpoints.Any(e => e.DomainName == DomainName))
            {
                // TODO: Add message cannot remove non existent endpoint
                WriteVerboseWithTimestamp("");
            }
            else
            {
                TrafficManagerEndpoint endpoint = profile.Endpoints.First(e => e.DomainName == DomainName);
                profile.Endpoints.Remove(endpoint);
            }

            WriteObject(profile);
        }
    }
}
