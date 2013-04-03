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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.HostedServices
{
    using System;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Swaps the deployments in production and stage.
    /// </summary>
    [Cmdlet(VerbsCommon.Move, "AzureDeployment"), OutputType(typeof(ManagementOperationContext))]
    public class MoveAzureDeploymentCommand : ServiceManagementBaseCmdlet
    {
        public MoveAzureDeploymentCommand()
        {
        }

        public MoveAzureDeploymentCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }

        public void ExecuteCommand()
        {
            var prodDeployment = GetDeploymentBySlot(DeploymentSlotType.Production);
            var stagingDeployment = GetDeploymentBySlot(DeploymentSlotType.Staging);

            if(stagingDeployment == null && prodDeployment == null)
            {
                throw new ArgumentOutOfRangeException(String.Format("No deployment found in Staging or Production: {0}", ServiceName));
            }

            if(stagingDeployment == null && prodDeployment != null)
            {
                throw new ArgumentOutOfRangeException(String.Format("No deployment found in Staging: {0}", ServiceName));
            }

            if(prodDeployment == null)
            {
                this.WriteVerbose(string.Format("Moving deployment from Staging to Production:{0}", ServiceName));
            }
            else
            {
                this.WriteVerbose(string.Format("VIP Swap is taking place between Staging and Production deployments.:{0}", ServiceName));
            }

            var swapDeploymentInput = new SwapDeploymentInput
            {
                SourceDeployment = stagingDeployment.Name, 
                Production = prodDeployment == null ? null : prodDeployment.Name
            };

            ExecuteClientActionInOCS(swapDeploymentInput, CommandRuntime.ToString(), s => this.Channel.SwapDeployment(s, this.ServiceName, swapDeploymentInput));
        }

        private Deployment GetDeploymentBySlot(string slot)
        {
            Deployment prodDeployment = null;
            try
            {
                InvokeInOperationContext(() => prodDeployment = RetryCall(s => Channel.GetDeploymentBySlot(s, ServiceName, slot)));
                if (prodDeployment != null && prodDeployment.RoleList != null)
                {
                    if (string.Compare(prodDeployment.RoleList[0].RoleType, "PersistentVMRole", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new ArgumentException(String.Format("Cannot Move Deployments with Virtual Machines Present in {0}", slot));
                    }
                }
            }
            catch (ServiceManagementClientException)
            {
                this.WriteDebug(String.Format("No deployment found in {0}", slot));
            }

            return prodDeployment;
        }
    }
}
