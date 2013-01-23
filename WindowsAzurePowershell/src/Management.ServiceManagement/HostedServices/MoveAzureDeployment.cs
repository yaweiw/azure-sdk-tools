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
    using System.Globalization;
    using System.Management.Automation;
    using System.ServiceModel;
    using Samples.WindowsAzure.ServiceManagement;
    using Cmdlets.Common;
    using Extensions;
    using Management.Model;

    /// <summary>
    /// Swaps the deployments in production and stage.
    /// </summary>
    [Cmdlet(VerbsCommon.Move, "AzureDeployment")]
    public class MoveAzureDeploymentCommand : CloudBaseCmdlet<IServiceManagement>
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

        public void ExecuteCommand()
        {
            Deployment prodDeployment = null;
            try
            {
                InvokeInOperationContext(() => prodDeployment = RetryCall(s => Channel.GetDeploymentBySlot(s, ServiceName, "Production")));
                if (prodDeployment != null && prodDeployment.RoleList != null)
                {
                    if (string.Compare(prodDeployment.RoleList[0].RoleType, "PersistentVMRole", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new ArgumentException("Cannot Move Deployments with Virtual Machines Present in Production");
                    }
                }
            }
            catch (EndpointNotFoundException e)
            {
                this.WriteDebug("No deployment found in production");
            }

            Deployment stagingDeployment = null;
            try
            {
                InvokeInOperationContext(() => stagingDeployment = RetryCall(s => Channel.GetDeploymentBySlot(s, ServiceName, "Staging")));
                if (stagingDeployment != null && stagingDeployment.RoleList != null)
                {
                    if (string.Compare(stagingDeployment.RoleList[0].RoleType, "PersistentVMRole", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new ArgumentException("Cannot Move Deployments with Virtual Machines Present in Staging");
                    }
                }
            }
            catch (EndpointNotFoundException e)
            {
                this.WriteDebug("No deployment found in staging");
            }

            if(stagingDeployment == null && prodDeployment == null)
            {
                throw new ArgumentOutOfRangeException(String.Format("No deployment found in Staging or Production: {0}", ServiceName));
            }
            else if(stagingDeployment == null && prodDeployment != null)
            {
                throw new ArgumentOutOfRangeException(String.Format("No deployment found in Staging: {0}", ServiceName));
            }
            else if(stagingDeployment != null && prodDeployment == null)
            {
                WriteVerbose(string.Format("Moving deployment from Staging to Production:{0}", ServiceName));
            }
            else if(stagingDeployment != null && prodDeployment != null)
            {
                WriteVerbose(string.Format("VIP Swap is taking place between Staging and Production deployments.:{0}", ServiceName));
            }

            String productionName = GetDeploymentName(DeploymentSlotType.Production);
            String stagingName = GetDeploymentName(DeploymentSlotType.Staging);

            if (stagingName == null)
            {
                throw new ArgumentException("The Staging deployment slot is empty.");
            }

            var swapDeploymentInput = new SwapDeploymentInput();
            swapDeploymentInput.SourceDeployment = stagingName;
            swapDeploymentInput.Production = productionName;

            ExecuteClientActionInOCS(swapDeploymentInput, CommandRuntime.ToString(), s => this.Channel.SwapDeployment(s, this.ServiceName, swapDeploymentInput), WaitForOperation);
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }

        private string GetDeploymentName(String slot)
        {
            try
            {
                Deployment deployment = RetryCall(s => Channel.GetDeploymentBySlot(s, ServiceName, slot.ToString(CultureInfo.InvariantCulture)));
                if (deployment != null)
                {
                    return deployment.Name;
                }
            }
            catch (CommunicationException)
            {
                // Do nothing
            }

            return null;
        }
    }
}
