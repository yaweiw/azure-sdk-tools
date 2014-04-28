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
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Management.Compute.Models;
    using Model;
    using Model.PersistentVMModel;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.Get, "AzureInternalLoadBalancer"), OutputType(typeof(InternalLoadBalancerContext))]
    public class GetAzureInternalLoadBalancer : ServiceManagementBaseCmdlet
    {
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Internal Load Balancer Name.")]
        [ValidateNotNullOrEmpty]
        public string InternalLoadBalancerName { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName { get; set; }

        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment Name.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        public string Slot { get; set; }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            var slot = string.IsNullOrEmpty(this.Slot) ? DeploymentSlot.Production
                     : (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), this.Slot, true);

            ExecuteClientActionNewSM(
                null,
                CommandRuntime.ToString(),
                () => this.ComputeClient.Deployments.GetBySlot(this.ServiceName, slot),
                (s, d) => d.LoadBalancers == null ? null : d.LoadBalancers.Select(
                    b => new InternalLoadBalancerContext
                    {
                        InternalLoadBalancerName = b.Name,
                        ServiceName = this.ServiceName,
                        DeploymentName = d.Name,
                        IPAddress = b.FrontendIPConfiguration != null ? b.FrontendIPConfiguration.StaticVirtualNetworkIPAddress : null,
                        SubnetName = b.FrontendIPConfiguration != null ? b.FrontendIPConfiguration.SubnetName : null
                    }));
        }
    }
}
