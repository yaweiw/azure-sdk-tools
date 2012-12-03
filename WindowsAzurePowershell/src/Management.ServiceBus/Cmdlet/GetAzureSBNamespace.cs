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
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.CloudService.Cmdlet.Common;
    using Microsoft.WindowsAzure.Management.ServiceBus.Properties;

    /// <summary>
    /// Lists all service bus namespaces asscoiated with a subscription
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureSBNamespace")]
    public class GetAzureSBNamespaceCommand : CloudCmdlet<IServiceManagement>
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
        public GetAzureSBNamespaceCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Gets service bus namespace by it's name or lists all namespaces if name is empty.
        /// </summary>
        /// <param name="name">The namespace name</param>
        /// <returns>The namespace instance</returns>
        internal ServiceBusNamespace GetNamespaceProcess(string subscriptionId, string name)
        {
            ServiceBusNamespace serviceBusNamespace = null;

            try
            {
                serviceBusNamespace = Channel.GetServiceBusNamespace(subscriptionId, name);
                WriteOutputObject(serviceBusNamespace);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals(Resources.InternalServerErrorMessage))
                {
                    SafeWriteError(new Exception(Resources.ServiceBusNamespaceMissingMessage, ex.InnerException));
                }
            }

            return serviceBusNamespace;
        }

        /// <summary>
        /// Gets a list of all namespaces associated with a subscription.
        /// </summary>
        /// <returns>The namespace list</returns>
        internal ServiceBusNamespaceList ListNamespacesProcess(string subscriptionId)
        {
            ServiceBusNamespaceList namespaces = Channel.ListServiceBusNamespaces(subscriptionId);
            WriteOutputObject(namespaces);

            return namespaces;
        }

        /// <summary>
        /// Executes the cmdlet.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

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