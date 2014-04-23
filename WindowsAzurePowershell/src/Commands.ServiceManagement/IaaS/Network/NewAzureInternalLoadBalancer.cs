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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.IaaS
{
    using System.Management.Automation;
    using System.Net;
    using Management.Compute.Models;
    using Model;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.New, "AzureInternalLoadBalancer"), OutputType(typeof(ManagementOperationContext))]
    public class NewAzureInternalLoadBalancer : ServiceManagementBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Internal Load Balancer Name.")]
        [ValidateNotNullOrEmpty]
        public string InternalLoadBalancerName { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName { get; set; }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Name.")]
        [ValidateNotNullOrEmpty]
        public string DeploymentName { get; set; }

        [Parameter(Mandatory = false, Position = 3, ValueFromPipelineByPropertyName = true, HelpMessage = "Type.")]
        [ValidateNotNullOrEmpty]
        public string Type { get; set; }

        [Parameter(Mandatory = false, Position = 4, ValueFromPipelineByPropertyName = true, HelpMessage = "Subnet Name.")]
        [ValidateNotNullOrEmpty]
        public string SubnetName { get; set; }

        [Parameter(Mandatory = false, Position = 5, ValueFromPipelineByPropertyName = true, HelpMessage = "Subnet Name.")]
        [ValidateNotNullOrEmpty]
        public IPAddress IPAddress { get; set; }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            var parameters = new LoadBalancerCreateParameters
            {
                Name = InternalLoadBalancerName,
                FrontendIPConfiguration = new FrontendIPConfiguration
                {
                    Type = Type,
                    SubnetName = SubnetName,
                    StaticVirtualNetworkIPAddress = IPAddress.ToString()
                }
            };

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () => ComputeClient.LoadBalancers.Create(ServiceName, DeploymentName, parameters));
        }
    }
}
