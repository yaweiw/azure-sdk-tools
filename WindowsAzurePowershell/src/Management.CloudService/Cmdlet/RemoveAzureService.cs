// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.CloudService.Cmdlet
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using Management.Services;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.CloudService.Utilities;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Model;
    using Properties;

    /// <summary>
    /// Deletes the specified hosted service from Windows Azure.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureService", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveAzureServiceCommand : CloudBaseCmdlet<IServiceManagement>
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "name of the hosted service")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "name of subscription which has this service")]
        public string Subscription
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Do not confirm deletion of deployment")]
        public SwitchParameter Force
        {
            get;
            set;
        }

        public RemoveAzureServiceCommand() { }

        public RemoveAzureServiceCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        public void RemoveAzureServiceProcess(string rootName, string inSubscription, string inServiceName)
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(rootName, inServiceName, null, null, null, inSubscription,
                                                            out serviceName);
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new Exception(Resources.InvalidServiceName);
            }

            if (!Force.IsPresent &&
                !ShouldProcess("", string.Format(Resources.RemoveServiceWarning, serviceName),
                                Resources.ShouldProcessCaption))
            {
                return;
            }

            if (!string.IsNullOrEmpty(settings.Subscription))
            {
                var globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory);
                CurrentSubscription = globalComponents.Subscriptions.Values.First(
                    sub => sub.SubscriptionName == settings.Subscription);
            }

            WriteVerboseWithTimestamp(Resources.RemoveServiceStartMessage, serviceName);
            WriteVerboseWithTimestamp(Resources.RemoveDeploymentMessage);
            StopAndRemove(rootName, serviceName, CurrentSubscription.SubscriptionName, ArgumentConstants.Slots[Slot.Production]);
            StopAndRemove(rootName, serviceName, CurrentSubscription.SubscriptionName, ArgumentConstants.Slots[Slot.Staging]);
            WriteVerboseWithTimestamp(Resources.RemoveServiceMessage);
            RemoveService(serviceName);

            WriteObject(true);
            WriteVerboseWithTimestamp(Resources.CompleteMessage);
        }

        private void StopAndRemove(string rootName, string serviceName, string subscription, string slot)
        {
            var deploymentStatusCommand = new GetDeploymentStatus(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
            if (deploymentStatusCommand.DeploymentExists(rootName, serviceName, slot, subscription))
            {
                DeploymentStatusManager setDeployment = new DeploymentStatusManager(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
                setDeployment.CommandRuntime = this.CommandRuntime;
                setDeployment.SetDeploymentStatusProcess(rootName, DeploymentStatus.Suspended, slot, subscription, serviceName);

                deploymentStatusCommand.WaitForState(DeploymentStatus.Suspended, rootName, serviceName, slot, subscription);

                RemoveAzureDeploymentCommand removeDeployment = new RemoveAzureDeploymentCommand(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
                removeDeployment.CommandRuntime = this.CommandRuntime;
                removeDeployment.RemoveAzureDeploymentProcess(rootName, serviceName, slot, subscription);

                while (deploymentStatusCommand.DeploymentExists(rootName, serviceName, slot, subscription)) ;
            }
        }

        private void RemoveService(string serviceName)
        {
            WriteVerboseWithTimestamp(string.Format(Resources.RemoveAzureServiceWaitMessage, serviceName));

            InvokeInOperationContext(() => RetryCall(s => this.Channel.DeleteHostedService(s, serviceName)));
        }

        public override void ExecuteCmdlet()
        {
            RemoveAzureServiceProcess(GetServiceRootPath(), Subscription, ServiceName);
        }
    }
}