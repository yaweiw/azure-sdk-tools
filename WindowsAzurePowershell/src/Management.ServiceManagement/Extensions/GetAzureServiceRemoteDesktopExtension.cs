


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
            : base()
        {
        }

        public GetAzureServiceRemoteDesktopExtensionCommand(IServiceManagement channel)
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
                            ParseUserName(ext1.PublicConfiguration), ParseExpiration(ext1.PublicConfiguration)) + "\n";
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
                                ParseUserName(ext2.PublicConfiguration), ParseExpiration(ext2.PublicConfiguration)) + "\n";
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
            WriteObject(ParseAllRolesConfig() + "\n" + ParseNamedRolesConfig());
        }

        protected override void OnProcessRecord()
        {
            ValidateParameters();
            ExecuteCommand();
        }
    }
}
