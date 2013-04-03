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

namespace Microsoft.WindowsAzure.Management.ServiceManagement.Extensions
{
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Retrieve Windows Azure Extensions.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceExtension"), OutputType(typeof(ServiceExtensionImageContext))]
    public class GetAzureServiceExtensionCommand : ServiceManagementBaseCmdlet
    {
        private const string ServiceNameHelperMessage = "Specify a service name to see its hosted extensions; Or do not specify any to see extension images.";

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "GetCloudServiceExtension", HelpMessage = ServiceNameHelperMessage)]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        public GetAzureServiceExtensionCommand()
        {
        }

        public GetAzureServiceExtensionCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }

        public void ExecuteCommand()
        {
            if (string.IsNullOrEmpty(ServiceName))
            {
                ExecuteClientActionInOCS(null,
                CommandRuntime.ToString(),
                s => this.Channel.ListLatestExtensions(CurrentSubscription.SubscriptionId),
                (op, extensions) => extensions.Select(extension => new ServiceExtensionImageContext
                {
                    OperationId = op.OperationTrackingId,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationStatus = op.Status,
                    ProviderNameSpace = extension.ProviderNameSpace,
                    Type = extension.Type,
                    Version = extension.Version,
                    Label = extension.Label,
                    Description = extension.Description,
                    HostingResources = extension.HostingResources,
                    ThumbprintAlgorithm = extension.ThumbprintAlgorithm,
                    PublicConfigurationSchema = extension.PublicConfigurationSchema,
                    PrivateConfigurationSchema = extension.PrivateConfigurationSchema
                }));
            }
            else
            {
                ExecuteClientActionInOCS(null,
                CommandRuntime.ToString(),
                s => this.Channel.ListHostedServiceExtensions(CurrentSubscription.SubscriptionId, ServiceName),
                (op, extensions) => extensions.Select(extension => new HostedServiceExtensionContext
                {
                    OperationId = op.OperationTrackingId,
                    OperationDescription = CommandRuntime.ToString(),
                    OperationStatus = op.Status,
                    ProviderNameSpace = extension.ProviderNameSpace,
                    Type = extension.Type,
                    Id = extension.Id,
                    Version = extension.Version,
                    Thumbprint = extension.Thumbprint,
                    ThumbprintAlgorithm = extension.ThumbprintAlgorithm,
                    PublicConfiguration = extension.PublicConfiguration
                }));
            }
        }
    }
}
