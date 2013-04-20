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
    using System.Collections.Generic;

    using Management.Utilities.Common;
    using Model;
    using WindowsAzure.ServiceManagement;
    using Management.Utilities.CloudService;
    using Microsoft.WindowsAzure.Management.Utilities.Properties;

    /// <summary>
    /// Get Windows Azure Service Remote Desktop Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceRemoteDesktopExtension"), OutputType(typeof(string))]
    public class GetAzureServiceRemoteDesktopExtensionCommand : BaseAzureServiceRemoteDesktopExtensionCmdlet
    {
        public GetAzureServiceRemoteDesktopExtensionCommand()
        {
        }

        public GetAzureServiceRemoteDesktopExtensionCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "GetAzureServiceRemoteDesktopExtension", HelpMessage = "Service Name")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "GetAzureServiceRemoteDesktopExtension", HelpMessage = "Slot")]
        [ValidateSet("Production", "Staging", IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        private Deployment Deployment
        {
            get;
            set;
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

        private string ParseAllRolesConfig()
        {
            string extensionConfigurationOutput = "All Roles:\n    ";
            string extensionFormatString = "RDP Enabled User: {0}, Expires: {1}";
            if (Deployment.ExtensionConfiguration != null)
            {
                foreach (Extension extension in Deployment.ExtensionConfiguration.AllRoles)
                {
                    HostedServiceExtension ext1 = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extension.Id);
                    if (CheckExtensionType(ext1))
                    {
                        extensionConfigurationOutput += string.Format(extensionFormatString,
                            ParseUserName(ext1.PublicConfiguration), ParseExpiration(ext1.PublicConfiguration)) + "\n";
                    }
                }
            }
            return extensionConfigurationOutput;
        }

        private string ParseNamedRolesConfig()
        {
            string extensionConfigurationOutput = "";
            string extensionFormatString = "RDP Enabled User: {0}, Expires: {1}";
            if (Deployment.ExtensionConfiguration != null)
            {
                foreach (RoleExtensions roleExts in Deployment.ExtensionConfiguration.NamedRoles)
                {
                    foreach (Extension extension in roleExts.Extensions)
                    {
                        HostedServiceExtension ext2 = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extension.Id);
                        if (CheckExtensionType(ext2))
                        {
                            extensionConfigurationOutput += roleExts.RoleName + ":\n    ";
                            extensionConfigurationOutput += string.Format(extensionFormatString,
                                ParseUserName(ext2.PublicConfiguration), ParseExpiration(ext2.PublicConfiguration)) + "\n";
                        }
                    }
                }
            }
            return extensionConfigurationOutput;
        }

        protected override bool CheckExtensionType(string extensionId)
        {
            if (!string.IsNullOrEmpty(extensionId))
            {
                HostedServiceExtension ext = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extensionId);
                return CheckExtensionType(ext);
            }
            return false;
        }

        private bool ValidateParameters()
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(General.TryGetServiceRootPath(CurrentPath()), ServiceName, null, null, null, null, CurrentSubscription.SubscriptionId, out serviceName);

            if (string.IsNullOrEmpty(serviceName))
            {
                WriteExceptionError(new Exception("Invalid service name"));
                return false;
            }
            else
            {
                ServiceName = serviceName;
                if (!IsServiceAvailable(ServiceName))
                {
                    WriteExceptionError(new Exception("Service not found: " + ServiceName));
                    return false;
                }
            }

            Slot = string.IsNullOrEmpty(Slot) ? "Production" : Slot;

            Deployment = this.Channel.GetDeploymentBySlot(CurrentSubscription.SubscriptionId, ServiceName, Slot);

            return true;
        }
        public void ExecuteCommand()
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

            WriteObject(ParseAllRolesConfig() + "\n" + ParseNamedRolesConfig());
        }

        protected override void OnProcessRecord()
        {
            if (ValidateParameters())
            {
                ExecuteCommand();
            }
            else
            {
                WriteExceptionError(new ArgumentException("Invalid Cmdlet parameters."));
            }
        }
    }
}
