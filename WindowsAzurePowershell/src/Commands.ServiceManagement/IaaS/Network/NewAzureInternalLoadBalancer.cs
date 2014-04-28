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
    using System.Management.Automation;
    using System.Net;
    using Management.Compute.Models;
    using Model.PersistentVMModel;
    using Utilities.Common;

    [Cmdlet(
        VerbsCommon.New,
        AzureInternalLoadBalancerNoun,
        DefaultParameterSetName = ServiceAndSlotParamSet),
    OutputType(
        typeof(ManagementOperationContext))]
    public class NewAzureInternalLoadBalancer : ServiceManagementBaseCmdlet
    {
        protected const string AzureInternalLoadBalancerNoun = "AzureInternalLoadBalancer";
        protected const string ServiceAndSlotParamSet = "ServiceAndSlot";
        protected const string SubnetNameAndIPParamSet = "SubnetNameAndIP";

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Internal Load Balancer Name.")]
        [ValidateNotNullOrEmpty]
        public string InternalLoadBalancerName { get; set; }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName { get; set; }

        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true, HelpMessage = "Slot.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        public string Slot { get; set; }

        [Parameter(ParameterSetName = SubnetNameAndIPParamSet, Position = 3, ValueFromPipelineByPropertyName = true, HelpMessage = "Subnet Name.")]
        [ValidateNotNullOrEmpty]
        public string SubnetName { get; set; }

        [Parameter(ParameterSetName = SubnetNameAndIPParamSet, Position = 4, ValueFromPipelineByPropertyName = true, HelpMessage = "Subnet IP Address.")]
        [ValidateNotNullOrEmpty]
        public IPAddress StaticVNetIPAddress { get; set; }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            var parameters = new LoadBalancerCreateParameters
            {
                Name = this.InternalLoadBalancerName,
                FrontendIPConfiguration = new FrontendIPConfiguration
                {
                    Type = FrontendIPConfigurationType.Private,
                    SubnetName = this.SubnetName,
                    StaticVirtualNetworkIPAddress = this.StaticVNetIPAddress == null ? null
                                                  : this.StaticVNetIPAddress.ToString()
                }
            };

            ExecuteClientActionNewSM(null,
                CommandRuntime.ToString(),
                () =>
                {
                    var slot = string.IsNullOrEmpty(this.Slot) ? DeploymentSlot.Production
                             : (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), this.Slot, true);
                    var deploymentName = this.ComputeClient.Deployments.GetBySlot(this.ServiceName, slot).Name;
                    return this.ComputeClient.LoadBalancers.Create(this.ServiceName, deploymentName, parameters);
                });
        }
    }
}
