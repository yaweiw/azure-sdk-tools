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
    using System.Linq;
    using System.Management.Automation;
    using System.Text.RegularExpressions;
    using Commands.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using Commands.Utilities.ServiceBus;
    using Commands.Utilities.ServiceBus.Contract;
    using Commands.Utilities.ServiceBus.ResourceModel;

    /// <summary>
    /// Creates new service bus namespace.
    /// </summary>
    [Cmdlet(VerbsCommon.New, "AzureSBNamespace"), OutputType(typeof(ServiceBusNamespace))]
    public class NewAzureSBNamespaceCommand : CloudBaseCmdlet<IServiceBusManagement>
    {
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Namespace name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Namespace location")]
        public string Location { get; set; }

        /// <summary>
        /// Initializes a new instance of the NewAzureSBNamespaceCommand class.
        /// </summary>
        public NewAzureSBNamespaceCommand()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the NewAzureSBNamespaceCommand class.
        /// </summary>
        /// <param name="channel">
        /// Channel used for communication with Azure's service management APIs.
        /// </param>
        public NewAzureSBNamespaceCommand(IServiceBusManagement channel)
        {
            Channel = channel;
        }

        /// <summary>
        /// Creates a new service bus namespace.
        /// </summary>
        /// <param name="subscriptionId">The user subscription id</param>
        /// <param name="name">The namespace name</param>
        /// <param name="region">The region name</param>
        /// <returns>The created service bus namespace</returns>
        public override void ExecuteCmdlet()
        {
            ServiceBusNamespace namespaceDescription = null;
            string subscriptionId = CurrentSubscription.SubscriptionId;
            string name = Name;
            string region = string.IsNullOrEmpty(Location) ? GetDefaultLocation() : Location;

            if (!Regex.IsMatch(name, ServiceBusConstants.NamespaceNamePattern))
            {
                throw new ArgumentException(string.Format(Resources.InvalidNamespaceName, name), "Name");
            }

            if (!Channel.ListServiceBusRegions(subscriptionId)
                .Exists(r => r.Code.Equals(region, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException(string.Format(Resources.InvalidServiceBusLocation, region), "Location");
            }

            try
            {
                namespaceDescription = new ServiceBusNamespace { Region = region };
                namespaceDescription = Channel.CreateServiceBusNamespace(subscriptionId, namespaceDescription, name);
                WriteObject(namespaceDescription);
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals(Resources.InternalServerErrorMessage))
                {
                    throw new Exception(Resources.NewNamespaceErrorMessage);
                }
            }
        }

        private string GetDefaultLocation()
        {
            return Channel.ListServiceBusRegions(CurrentSubscription.SubscriptionId).First().Code;
        }
    }
}