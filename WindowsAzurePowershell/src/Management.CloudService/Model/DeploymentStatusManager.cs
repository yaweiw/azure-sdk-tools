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

namespace Microsoft.WindowsAzure.Management.CloudService.Model
{
    using System.Linq;
    using System.Management.Automation;
    using Management.Services;
    using Utilities;
    using Cmdlets.Common;
    using Properties;
    using ServiceManagement;


    /// <summary>
    /// Change deployment status to running or suspended.
    /// </summary>
    public class DeploymentStatusManager : CloudServiceManagementBaseCmdlet
    {
        public DeploymentStatusManager() { }

        public DeploymentStatusManager(IServiceManagement channel)
        {
            Channel = channel;
        }

        public string Status
        {
            get;
            set;
        }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Service name.")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Deployment slot. Staging | Production")]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Subscription name")]
        public string Subscription
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        public virtual void SetDeploymentStatusProcess(string rootPath, string newStatus, string slot, string subscription, string serviceName)
        {
            string result;

            if (!string.IsNullOrEmpty(subscription))
            {
                var globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory);
                CurrentSubscription = globalComponents.Subscriptions.Values.First(
                    sub => sub.SubscriptionName == subscription);
            }

            // Check that deployment slot for the service exists
            WriteVerboseWithTimestamp(Resources.LookingForDeploymentMessage, slot, serviceName);
            result = CheckDeployment(newStatus, serviceName, slot);

            if (string.IsNullOrEmpty(result))
            {
                SetDeployment(newStatus, serviceName, slot);
                GetDeploymentStatus deploymentStatusCommand = new GetDeploymentStatus(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
                deploymentStatusCommand.WaitForState(newStatus, rootPath, serviceName, slot, CurrentSubscription.SubscriptionName);
                Deployment deployment = this.RetryCall<Deployment>(s => this.Channel.GetDeploymentBySlot(s, serviceName, slot));

                if (PassThru)
                {
                    WriteObject(deployment);                    
                }

                WriteVerboseWithTimestamp(string.Format(Resources.ChangeDeploymentStatusCompleteMessage, serviceName, newStatus));
            }
            else
            {
                WriteWarning(result);
            }
        }

        private string CheckDeployment(string status, string serviceName, string slot)
        {
            string result = string.Empty;

            try
            {
                var deployment = RetryCall(s => Channel.GetDeploymentBySlot(s, serviceName, slot));

                // Check to see if the service is in transitioning state
                //
                if (deployment.Status != DeploymentStatus.Running && deployment.Status != DeploymentStatus.Suspended)
                {
                    result = string.Format(Resources.ServiceIsInTransitionState, slot, serviceName, deployment.Status);
                }
                // Check to see if user trying to stop an already stopped service or 
                // starting an already starting service
                //
                else if (deployment.Status == DeploymentStatus.Running && status == DeploymentStatus.Running ||
                    deployment.Status == DeploymentStatus.Suspended && status == DeploymentStatus.Suspended)
                {
                    result = string.Format(Resources.DeploymentAlreadyInState, slot, serviceName, status);
                }
            }
            catch
            {
                // If we reach here that means the slot doesn't exist
                //
                result = string.Format(Resources.ServiceSlotDoesNotExist, slot, serviceName);
            }

            return result;
        }

        private void SetDeployment(string status, string serviceName, string slot)
        {
            var updateDeploymentStatus = new UpdateDeploymentStatusInput
            {
                Status = status
            };

            InvokeInOperationContext(() => RetryCall(s => Channel.UpdateDeploymentStatusBySlot(
                s,
                serviceName,
                slot,
                updateDeploymentStatus)));
        }

        public override void ExecuteCmdlet()
        {
            string serviceName;
            string rootPath = General.TryGetServiceRootPath(CurrentPath());
            ServiceSettings settings = General.GetDefaultSettings(
                rootPath,
                ServiceName,
                Slot,
                null,
                null,
                null,
                Subscription,
                out serviceName);

            SetDeploymentStatusProcess(rootPath, Status, settings.Slot, settings.Subscription, serviceName);
        }
    }
}