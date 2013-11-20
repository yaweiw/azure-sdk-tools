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

namespace Microsoft.WindowsAzure.Commands.ServiceManagement.HostedServices
{
    using System;
    using System.Management.Automation;
    using System.Net;
    using AutoMapper;
    using Management.Compute;
    using Management.Compute.Models;
    using Management.VirtualNetworks;
    using Model.PersistentVMModel;
    using Properties;
    using Utilities.Common;

    /// <summary>
    /// Deletes the specified deployment.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureDeployment"), OutputType(typeof(ManagementOperationContext))]
    public class RemoveAzureDeploymentCommand : ServiceManagementBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot. Staging | Production")]
        [ValidateSet(DeploymentSlotType.Staging, DeploymentSlotType.Production, IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, HelpMessage = "Do not confirm deletion of deployment")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        public virtual SwitchParameter DeleteReservedVIP
        {
            get;
            set;
        }

        public void RemoveDeploymentProcess()
        {
            ServiceManagementProfile.Initialize();
            
            var slotType = (DeploymentSlot)Enum.Parse(typeof(DeploymentSlot), this.Slot, true);

            DeploymentGetResponse deploymentGetResponse = this.ComputeClient.Deployments.GetBySlot(this.ServiceName, slotType);

            bool toDeleteReservedIP = false;
            if (DeleteReservedVIP.IsPresent)
            {
                if (deploymentGetResponse != null && !string.IsNullOrEmpty(deploymentGetResponse.ReservedIPName))
                {
                    WriteWarning(string.Format(Resources.ReservedIPNameNoLongerInUseAndWillBeDeleted, deploymentGetResponse.ReservedIPName));
                    toDeleteReservedIP = true;
                }
            }
            else if (deploymentGetResponse != null && !string.IsNullOrEmpty(deploymentGetResponse.ReservedIPName))
            {
                WriteWarning(string.Format(Resources.ReservedIPNameNoLongerInUseButStillBeingReserved, deploymentGetResponse.ReservedIPName));
            }

            ExecuteClientActionNewSM<OperationResponse>(
                null,
                CommandRuntime.ToString(),
                () =>
                {
                    OperationResponse response = this.ComputeClient.Deployments.DeleteBySlot(this.ServiceName, slotType);

                    if (toDeleteReservedIP)
                    {
                        this.NetworkClient.Networks.DeleteReservedIP(deploymentGetResponse.ReservedIPName);
                    }

                    return response;
                });
        }

        protected override void OnProcessRecord()
        {
            if (this.Force.IsPresent || this.ShouldContinue(Resources.DeployedArtifactsWillBeRemoved, Resources.DeploymentDeletion))
            {
                this.RemoveDeploymentProcess();
            }
        }
    }
}
