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
    using Management.Compute.Models;
    using Management.Network.Models;
    using Model;
    using Model.PersistentVMModel;
    using Utilities.Common;

    [Cmdlet(VerbsCommon.New, ReservedIPConstants.CmdletNoun, DefaultParameterSetName = ReserveNewIPParamSet), OutputType(typeof(ManagementOperationContext))]
    public class NewAzureReservedIPCmdlet : ServiceManagementBaseCmdlet
    {
        protected const string ReserveNewIPParamSet = "CreateNewReservedIP";
        protected const string ReserveInUseIPUsingSlotParamSet = "CreateInUseReservedIPUsingSlot";
        protected const string ReserveInUseIPParamSet = "CreateInUseReservedIP";

        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveNewIPParamSet, HelpMessage = "Reserved IP Name.")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPUsingSlotParamSet, HelpMessage = "Reserved IP Name.")]
        [Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Reserved IP Name.")]
        [ValidateNotNullOrEmpty]
        public string ReservedIPName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPUsingSlotParamSet, HelpMessage = "Service Name.")]
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Service Name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPUsingSlotParamSet, HelpMessage = "Deployment slot. Staging | Production (default Production)")]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        [ValidateNotNullOrEmpty]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, Position = 1, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveNewIPParamSet, HelpMessage = "Reserved IP Label.")]
        [Parameter(Mandatory = false, Position = 3, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPUsingSlotParamSet, HelpMessage = "Reserved IP Label.")]
        [Parameter(Mandatory = false, Position = 2, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Reserved IP Label.")]
        [ValidateNotNullOrEmpty]
        public string Label
        {
            get;
            set;
        }

        [Parameter(Mandatory = true, Position = 2, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveNewIPParamSet, HelpMessage = "Location Name.")]
        [Parameter(Mandatory = true, Position = 4, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPUsingSlotParamSet, HelpMessage = "Location Name.")]
        [Parameter(Mandatory = true, Position = 3, ValueFromPipelineByPropertyName = true, ParameterSetName = ReserveInUseIPParamSet, HelpMessage = "Location Name.")]
        [ValidateNotNullOrEmpty]
        public string Location
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            ServiceManagementProfile.Initialize();

            ExecuteClientActionNewSM(
                null,
                CommandRuntime.ToString(),
                () =>
                {
                    var parameters = new NetworkReservedIPCreateParameters
                    {
                        Name           = this.ReservedIPName,
                        Label          = this.Label,
                        Location       = this.Location,
                        ServiceName    = this.ServiceName,
                        DeploymentName = GetDeploymentName()
                    };

                    return this.NetworkClient.ReservedIPs.Create(parameters);
                });
        }

        protected string GetDeploymentName()
        {
            string deploymentName = null;

            if (!string.IsNullOrEmpty(this.ServiceName))
            {
                DeploymentSlot slot = string.IsNullOrEmpty(this.Slot) ? DeploymentSlot.Production
                                    : (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), this.Slot, true);

                var deployment = this.ComputeClient.Deployments.GetBySlot(this.ServiceName, slot);

                deploymentName = deployment.Name;
            }

            return deploymentName;
        }
    }
}
