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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Model;
    using WindowsAzure.Management.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Get Windows Azure Service Extension Image.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceExtensionImage"), OutputType(typeof(IEnumerable<HostedServiceExtensionImageContext>))]
    public class GetAzureServiceExtensionImageCommand : ServiceManagementBaseCmdlet
    {
        public GetAzureServiceExtensionImageCommand()
        {
        }

        public GetAzureServiceExtensionImageCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        public void ExecuteCommand()
        {
            ExecuteClientActionInOCS(null,
                CommandRuntime.ToString(),
                s => this.Channel.ListLatestExtensions(CurrentSubscription.SubscriptionId),
                (op, extensions) => extensions.Select(extension => new HostedServiceExtensionImageContext
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

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }
    }
}
