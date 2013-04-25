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
    using Management.Utilities.CloudService;
    using Management.Utilities.Common;
    using Management.Utilities.Properties;
    using Model;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Get Windows Azure Service Diagnostics Extension.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureServiceDiagnosticsExtension"), OutputType(typeof(string))]
    public class GetAzureServiceDiagnosticsExtensionCommand : BaseAzureServiceDiagnosticsExtensionCmdlet
    {
        public GetAzureServiceDiagnosticsExtensionCommand()
            : base()
        {
        }

        public GetAzureServiceDiagnosticsExtensionCommand(IServiceManagement channel)
            : base(channel)
        {
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "GetAzureServiceRemoteDesktopExtension", HelpMessage = "Service Name")]
        [ValidateNotNullOrEmpty]
        public override string ServiceName
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

        private string ParseConnectionQualifiers(string config)
        {
            return ParseConfig(config, "<ConnectionQualifiers>", "</ConnectionQualifiers>");
        }

        private string ParseDefaultEndpointsProtocol(string config)
        {
            return ParseConfig(config, "<DefaultEndpointsProtocol>", "</DefaultEndpointsProtocol>");
        }

        private string ParseName(string config)
        {
            return ParseConfig(config, "<Name>", "</Name>");
        }

        private string ParseAllRolesConfig()
        {
            string outputStr = "All Roles:\n    ";
            if (Deployment.ExtensionConfiguration != null)
            {
                foreach (Extension extension in Deployment.ExtensionConfiguration.AllRoles)
                {
                    HostedServiceExtension ext1 = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extension.Id);
                    if (CheckExtensionType(ext1))
                    {
                        outputStr += "<" + ext1.Id + "> ";
                        outputStr += string.Format(PublicConfigurationDescriptionTemplate,
                            ParseName(ext1.PublicConfiguration), ParseDefaultEndpointsProtocol(ext1.PublicConfiguration), ParseName(ext1.PublicConfiguration)) + "\n";
                    }
                }
            }
            return outputStr;
        }

        private string ParseNamedRolesConfig()
        {
            string outputStr = "Named Roles:\n    ";
            if (Deployment.ExtensionConfiguration != null)
            {
                foreach (RoleExtensions roleExts in Deployment.ExtensionConfiguration.NamedRoles)
                {
                    foreach (Extension extension in roleExts.Extensions)
                    {
                        HostedServiceExtension ext2 = Channel.GetHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extension.Id);
                        if (CheckExtensionType(ext2))
                        {
                            outputStr += roleExts.RoleName + ":\n    ";
                            outputStr += "<" + ext2.Id + "> ";
                            outputStr += string.Format(PublicConfigurationDescriptionTemplate,
                                ParseName(ext2.PublicConfiguration), ParseDefaultEndpointsProtocol(ext2.PublicConfiguration), ParseName(ext2.PublicConfiguration)) + "\n";
                        }
                    }
                }
            }
            return outputStr;
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

            ExtensionManager = new HostedServiceExtensionManager(Channel, CurrentSubscription.SubscriptionId, ServiceName);

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
            ValidateParameters();
            ExecuteCommand();
        }
    }
}