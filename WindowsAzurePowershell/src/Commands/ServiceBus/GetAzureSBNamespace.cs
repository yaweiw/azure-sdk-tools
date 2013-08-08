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

namespace Microsoft.WindowsAzure.Commands.ServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Text.RegularExpressions;
    using Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Commands.Utilities.ServiceBus;
    using Commands.Utilities.ServiceBus.Contract;
    using Commands.Utilities.ServiceBus.ResourceModel;

    /// <summary>
    /// Lists all service bus namespaces asscoiated with a subscription
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSBNamespace"), OutputType(typeof(List<ServiceBusNamespace>), typeof(ServiceBusNamespace))]
    public class GetAzureSBNamespaceCommand : CloudBaseCmdlet<IServiceBusManagement>
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Namespace name")]
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the PublishAzureServiceCommand class.
        /// </summary>
        public GetAzureSBNamespaceCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the GetAzureServiceBusNamespacesCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public GetAzureSBNamespaceCommand(IServiceBusManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Gets service bus namespace by it's name or lists all namespaces if name is empty.
        /// </summary>
        /// <param name="name">The namespace name</param>
        /// <returns>The namespace instance</returns>
        internal void GetNamespaceProcess(string subscriptionId, string name)
        {
            ServiceBusNamespace serviceBusNamespace = null;

            if (!Regex.IsMatch(name, ServiceBusConstants.NamespaceNamePattern))
            {
                throw new ArgumentException(string.Format(Resources.InvalidNamespaceName, name), "Name");
            }

            try
            {
                serviceBusNamespace = Channel.GetServiceBusNamespace(subscriptionId, name);
                WriteObject(serviceBusNamespace);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals(Resources.InternalServerErrorMessage))
                {
                    WriteExceptionError(new Exception(Resources.ServiceBusNamespaceMissingMessage));
                }
            }
        }

        /// <summary>
        /// Gets a list of all namespaces associated with a subscription.
        /// </summary>
        /// <returns>The namespace list</returns>
        internal void ListNamespacesProcess(string subscriptionId)
        {
            List<ServiceBusNamespace> namespaces = Channel.ListServiceBusNamespaces(subscriptionId);
            WriteObject(namespaces, true);
        }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            if (string.IsNullOrEmpty(Name))
            {
                ListNamespacesProcess(CurrentSubscription.SubscriptionId);
            }
            else
            {
                GetNamespaceProcess(CurrentSubscription.SubscriptionId, Name);
            }
        }
    }
}