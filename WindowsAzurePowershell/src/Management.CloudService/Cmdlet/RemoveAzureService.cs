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
    [Cmdlet(VerbsCommon.Remove, "AzureService", SupportsShouldProcess = true), OutputType(typeof(bool))]
    public class RemoveAzureServiceCommand : CloudBaseCmdlet<IServiceManagement>
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "name of the hosted service")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "name of subscription which has this service")]
        public string Subscription
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

        [Parameter(Position = 3, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        public RemoveAzureServiceCommand() { }

        public RemoveAzureServiceCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        public void RemoveAzureServiceProcess(string rootName, string inSubscription, string inServiceName)
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(
                rootName,
                inServiceName,
                null,
                null,
                null,
                null,
                inSubscription,
                out serviceName);

            if (string.IsNullOrEmpty(serviceName))
            {
                throw new Exception(Resources.InvalidServiceName);
            }

            ConfirmAction(
                Force.IsPresent,
                string.Format(Resources.RemoveServiceWarning, serviceName),
                Resources.RemoveServiceWhatIfMessage,
                serviceName,
                () =>
                {
                    if (!string.IsNullOrEmpty(settings.Subscription))
                    {
                        var globalComponents = GlobalComponents.Load(GlobalPathInfo.GlobalSettingsDirectory);
                        CurrentSubscription = globalComponents.Subscriptions.Values.First(
                            sub => sub.SubscriptionName == settings.Subscription);
                    }

                    // Check that cloud service exists
                    WriteVerboseWithTimestamp(Resources.LookingForServiceMessage, serviceName);
                    bool found = false;

                    InvokeInOperationContext(() =>
                    {
                        this.RetryCall(s => found = !Channel.IsDNSAvailable(CurrentSubscription.SubscriptionId, serviceName).Result);
                    });

                    if (found)
                    {
                        StopAndRemove(rootName, serviceName, CurrentSubscription.SubscriptionName, ArgumentConstants.Slots[Slot.Production]);
                        StopAndRemove(rootName, serviceName, CurrentSubscription.SubscriptionName, ArgumentConstants.Slots[Slot.Staging]);
                        RemoveService(serviceName);

                        if (PassThru)
                        {
                            WriteObject(true);
                        }
                    }
                    else
                    {
                        WriteExceptionError(new Exception(string.Format(Resources.ServiceDoesNotExist, serviceName)));
                    }
                });
        }

        private void StopAndRemove(string rootName, string serviceName, string subscription, string slot)
        {
            var deploymentStatusCommand = new GetDeploymentStatus(Channel) { ShareChannel = ShareChannel, CurrentSubscription = CurrentSubscription };
            if (deploymentStatusCommand.DeploymentExists(rootName, serviceName, slot, subscription))
            {
                InvokeInOperationContext(() =>
                {
                    this.RetryCall(s => this.Channel.DeleteDeploymentBySlot(s, serviceName, slot));
                });

                // Wait until deployment is removed
                while (deploymentStatusCommand.DeploymentExists(rootName, serviceName, slot, subscription));
            }
        }

        private void RemoveService(string serviceName)
        {
            WriteVerboseWithTimestamp(string.Format(Resources.RemoveAzureServiceWaitMessage, serviceName));
            InvokeInOperationContext(() => RetryCall(s => this.Channel.DeleteHostedService(s, serviceName)));
        }

        public override void ExecuteCmdlet()
        {
            RemoveAzureServiceProcess(General.TryGetServiceRootPath(CurrentPath()), Subscription, ServiceName);
        }
    }
}