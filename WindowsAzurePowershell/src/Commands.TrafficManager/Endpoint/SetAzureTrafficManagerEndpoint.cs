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
using System.Linq;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager;
using Microsoft.WindowsAzure.Management.TrafficManager.Models;

namespace Microsoft.WindowsAzure.Commands.TrafficManager.Endpoint
{
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager.Models;

    [Cmdlet(VerbsCommon.Set, "AzureTrafficManagerEndpoint"), OutputType(typeof(ProfileWithDefinition))]
    public class SetAzureTrafficManagerEndpoint : TrafficManagerConfigurationBaseCmdlet
    {
        [Parameter(Mandatory = true,
           ValueFromPipelineByPropertyName = true)]
        public string Name { get; set; }

        [Parameter(Mandatory = true)]
        public string DomainName { get; set; }

        [Parameter(Mandatory = false)]
        public string Location { get; set; }

        [Parameter(Mandatory = false)]
        [ValidateSet("CloudService", "AzureWebsite", "Any", IgnoreCase = false)]
        public string Type { get; set; }

        [Parameter(Mandatory = false)]
        public System.Int32 Weight { get; set; }

        public override void ExecuteCmdlet()
        {
            ProfileWithDefinition profile = TrafficManagerProfile.GetInstance();

            TrafficManagerEndpoint endpoint = profile.Endpoints.FirstOrDefault(e => e.DomainName == DomainName);

            if (endpoint == null)
            {
                if (String.IsNullOrEmpty(Type) || Weight == 0)
                {
                    // TODO: Add message need parameters when non existent
                    throw new Exception();
                }

                WriteVerboseWithTimestamp(Resources.SetInexistentTrafficManagerEndpointMessage, Name, DomainName);
                endpoint.DomainName = DomainName;
                endpoint.Location = Location;
                endpoint.Type = (EndpointType)Enum.Parse(typeof(EndpointType), Type);
                endpoint.Weight = Weight;
            }

            endpoint.DomainName = DomainName ?? endpoint.DomainName;
            endpoint.Location = Location ?? endpoint.Location;

            endpoint.Type = !String.IsNullOrEmpty(Type)
                ? (EndpointType)Enum.Parse(typeof(EndpointType), Type)
                : endpoint.Type;

            endpoint.Weight = Weight;

            profile.Endpoints.Add(endpoint);
            WriteObject(profile);
        }
    }
}
