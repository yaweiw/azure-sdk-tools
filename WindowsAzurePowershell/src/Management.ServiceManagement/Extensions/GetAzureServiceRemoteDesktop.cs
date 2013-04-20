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
    using System.Linq;
    using System.Management.Automation;

    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Retrieve Windows Azure Extensions.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceRemoteDesktop"), OutputType(typeof(ManagementOperationContext))]
    public class GetAzureServiceExtensionCommand : ServiceManagementBaseCmdlet
    {
        private const string ServiceNameHelperMessage = "Specify a service name to see its hosted extensions; or do not specify any to see extension images.";

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "GetCloudServiceExtension", HelpMessage = ServiceNameHelperMessage)]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "GetCloudServiceExtension", HelpMessage = "Slot")]
        [ValidateSet("Production", "Staging", IgnoreCase = true)]
        public string Slot
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

        private string ParseConfig(string config, string prefix, string postfix)
        {
            string value = "";
            if (!string.IsNullOrEmpty(config) && !string.IsNullOrEmpty(prefix) && !string.IsNullOrEmpty(postfix))
            {
                int startIndex = config.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
                int endIndex = config.IndexOf(postfix, StringComparison.OrdinalIgnoreCase);
                if (startIndex + prefix.Length <= endIndex)
                {
                    value = config.Substring(startIndex + prefix.Length, endIndex - (startIndex + prefix.Length));
                }
            }
            return value;
        }

        private string ParseUserName(string config)
        {
            return ParseConfig(config, "<UserName>", "</UserName>");
        }

        private string ParseExpiration(string config)
        {
            return ParseConfig(config, "<Expiration>", "</Expiration>");
        }

        public void ExecuteCommand()
        {
            if (string.IsNullOrEmpty(ServiceName))
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

                Deployment deployment = this.Channel.GetDeploymentBySlot(CurrentSubscription.SubscriptionId, ServiceName, string.IsNullOrEmpty(Slot) ? "Production" : Slot);
                string extConfigStr = "";
                if (deployment.ExtensionConfiguration != null)
                {
                    extConfigStr = "AllRoles";
                    foreach (Extension ext in deployment.ExtensionConfiguration.AllRoles)
                    {
                        HostedServiceExtension hostedSvcExt = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, ext.Id);
                        if (hostedSvcExt.Type == "RDP")
                        {
                            extConfigStr += "\n    RDP Enabled User: " + ParseUserName(hostedSvcExt.PublicConfiguration);
                            extConfigStr += ", Expires: " + ParseExpiration(hostedSvcExt.PublicConfiguration);
                        }
                    }
                    foreach (RoleExtensions roleExts in deployment.ExtensionConfiguration.NamedRoles)
                    {
                        foreach (Extension ext2 in roleExts.Extensions)
                        {
                            HostedServiceExtension hostedSvcExt2 = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, ext2.Id);
                            if (hostedSvcExt2.Type == "RDP")
                            {
                                extConfigStr += "\n" + roleExts.RoleName + "\n";
                                extConfigStr += "    RDP Enabled User: " + ParseUserName(hostedSvcExt2.PublicConfiguration);
                                extConfigStr += ", Expires: " + ParseExpiration(hostedSvcExt2.PublicConfiguration);
                            }
                        }
                    }
                }
                WriteObject(extConfigStr);
            }
        }

        protected override void OnProcessRecord()
        {
            this.ExecuteCommand();
        }
    }
}
