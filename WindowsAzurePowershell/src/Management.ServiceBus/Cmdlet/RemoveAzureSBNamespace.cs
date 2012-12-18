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

namespace Microsoft.WindowsAzure.Management.ServiceBus.Cmdlet
{
    using System;
    using System.Management.Automation;
    using System.Text.RegularExpressions;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.ServiceBus.Properties;

    /// <summary>
    /// Creates new service bus namespace.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureSBNamespace")]
    public class RemoveAzureSBNamespaceCommand : CloudBaseCmdlet<IServiceManagement>
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Namespace name")]
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the RemoveAzureSBNamespaceCommand class.
        /// </summary>
        public RemoveAzureSBNamespaceCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RemoveAzureSBNamespaceCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public RemoveAzureSBNamespaceCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Removes a service bus namespace.
        /// </summary>
        /// <param name="subscriptionId">The user subscription id</param>
        /// <param name="name">The namespace name</param>
        /// <summary>
        internal void RemoveServiceBusNamespaceProcess(string subscriptionId, string name)
        {
            try
            {
                if (Regex.IsMatch(name, ServiceBusConstants.NamespaceNamePattern))
                {
                    Channel.DeleteServiceBusNamespace(subscriptionId, name);
                    SafeWriteVerbose(string.Format(Resources.RemovingNamespaceMessage, name));
                }
                else
                {
                    SafeWriteError(new ArgumentException(string.Format(Resources.InvalidNamespaceName, name), "Name"));
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals(Resources.InternalServerErrorMessage))
                {
                    SafeWriteError(new Exception(Resources.RemoveNamespaceErrorMessage));
                }
            }
        }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();
            RemoveServiceBusNamespaceProcess(CurrentSubscription.SubscriptionId, Name);
        }
    }
}