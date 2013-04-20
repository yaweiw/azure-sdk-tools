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
    using System.Security.Cryptography.X509Certificates;

    using WindowsAzure.Management.ServiceManagement.Helpers;
    using WindowsAzure.Management.Utilities.CloudService;
    using WindowsAzure.Management.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Set Windows Azure Service Remote Desktop.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceRemoteDesktop"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceRemoteDesktopExtensionCommand : ServiceManagementBaseCmdlet
    {
        private const string LegacySettingStr = "Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled";

        private const string ExtensionNameSpace = "Microsoft.Windows.Azure.Extensions";
        private const string ExtensionType = "RDP";

        private const string PublicConfigurationTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                                           "<?xml-stylesheet type=\"text/xsl\" href=\"style.xsl\"?>" +
                                                           "<PublicConfig>" +
                                                           "<UserName>{0}</UserName>" +
                                                           "<Expiration>{1}</Expiration>" +
                                                           "</PublicConfig>";

        private const string PrivateConfigurationTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                                            "<?xml-stylesheet type=\"text/xsl\" href=\"style.xsl\"?>" +
                                                            "<PrivateConfig>" +
                                                            "<Password>{0}</Password>" +
                                                            "</PrivateConfig>";

        public SetAzureServiceRemoteDesktopExtensionCommand()
        {
        }

        public SetAzureServiceRemoteDesktopExtensionCommand(IServiceManagement channel)
        {
            Channel = channel;
        }

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Cloud Service Name")]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Production (default) or Staging.")]
        [ValidateSet("Production", "Staging", IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Default All Roles, or specify ones for Named Roles.")]
        public string[] Roles
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ParameterSetName = "SetExtension", HelpMessage = "Remote Desktop User Name")]
        [ValidateNotNullOrEmpty]
        public string UserName
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = true, ParameterSetName = "SetExtension", HelpMessage = "Remote Desktop User Password")]
        [ValidateNotNullOrEmpty]
        public string Password
        {
            get;
            set;
        }

        [Parameter(Position = 5, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "Remote Desktop User Expiration Date")]
        [ValidateNotNullOrEmpty]
        public DateTime Expiration
        {
            get;
            set;
        }


        [Parameter(Position = 6, Mandatory = false, ParameterSetName = "SetExtension", HelpMessage = "X509Certificate used to encrypt password.")]
        [ValidateNotNullOrEmpty]
        public X509Certificate2 X509Certificate
        {
            get;
            set;
        }

        private string ExpirationStr
        {
            get
            {
                return Expiration.ToString("yyyy-MM-dd");
            }
        }

        private string Thumbprint
        {
            get;
            set;
        }

        private string ThumbprintAlgorithm
        {
            get;
            set;
        }

        private Deployment Deployment
        {
            get;
            set;
        }

        private bool IsServiceAvailable(string serviceName)
        {
            // Check that cloud service exists
            bool found = false;
            InvokeInOperationContext(() =>
            {
                this.RetryCall(s => found = !Channel.IsDNSAvailable(CurrentSubscription.SubscriptionId, serviceName).Result);
            });
            return found;
        }

        private bool ValidateParameters()
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(General.TryGetServiceRootPath(CurrentPath()), ServiceName, null, null, null, null, CurrentSubscription.SubscriptionId, out serviceName);
            ServiceName = serviceName;

            if (!IsServiceAvailable(ServiceName))
            {
                WriteExceptionError(new Exception("Service not found: " + ServiceName));
                return false;
            }

            Slot = string.IsNullOrEmpty(Slot) ? "Production" : Slot;

            Deployment = Channel.GetDeploymentBySlot(CurrentSubscription.SubscriptionId, ServiceName, Slot);
            if (Deployment == null)
            {
                WriteExceptionError(new Exception(string.Format("Deployment not found in service: {0} and slot: {1}", ServiceName, Slot)));
                return false;
            }

            if (Deployment.ExtensionConfiguration == null)
            {
                Deployment.ExtensionConfiguration = new ExtensionConfiguration
                {
                    AllRoles = new AllRoles(),
                    NamedRoles = new NamedRoles()
                };
            }

            if (Roles != null)
            {
                foreach (string roleName in Roles)
                {
                    if (Deployment.RoleList == null || !Deployment.RoleList.Any(r => r.RoleName == roleName))
                    {
                        WriteExceptionError(new Exception(string.Format("Role: {0} not found in deployment {1} of service {2}.", roleName, Slot, ServiceName)));
                        return false;
                    }
                }
            }

            Expiration = Expiration.Equals(default(DateTime)) ? DateTime.Now.AddMonths(6) : Expiration;

            if (X509Certificate != null)
            {
                var operationDescription = string.Format("{0} - Uploading Certificate: {1}", CommandRuntime, X509Certificate.Thumbprint);
                ExecuteClientActionInOCS(null, operationDescription, s => this.Channel.AddCertificates(s, this.ServiceName, CertUtils.Create(X509Certificate)));
                Thumbprint = X509Certificate.Thumbprint;
                ThumbprintAlgorithm = X509Certificate.SignatureAlgorithm.FriendlyName;
            }

            return true;
        }

        private void DeleteHostedServieExtension(string extensionId)
        {
            Channel.DeleteHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extensionId);
        }

        private void AddHostedServiceExtension(string extensionId)
        {
            HostedServiceExtensionInput hostedSvcExtInput = new HostedServiceExtensionInput
            {
                Id = extensionId,
                Thumbprint = Thumbprint,
                ThumbprintAlgorithm = ThumbprintAlgorithm,
                ProviderNameSpace = ExtensionNameSpace,
                Type = ExtensionType,
                PublicConfiguration = string.Format(PublicConfigurationTemplate, UserName, ExpirationStr),
                PrivateConfiguration = string.Format(PrivateConfigurationTemplate, Password)
            };
            Channel.AddHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, hostedSvcExtInput);
        }

        private bool InstallExtension(Deployment deployment, string extensionId)
        {
            HostedServiceExtension extension = HostedServiceExtensionHelper.GetExtension(Channel, CurrentSubscription.SubscriptionId, ServiceName, extensionId);
            if (extension != null)
            {
                if (extension.ProviderNameSpace == ExtensionNameSpace && extension.Type == ExtensionType)
                {
                    DeleteHostedServieExtension(extensionId);
                }
                else
                {
                    // Error - An existing extension found with a non-RDP type, so we cannot overwrite it.
                    WriteExceptionError(new Exception("A non-RDP configuration already exists with ID " + extensionId
                        + " and Type " + extension.Type + ". It cannot be overwritten using this command."));
                    return false;
                }
            }
            AddHostedServiceExtension(extensionId);
            return true;
        }

        private void ChangeDeployment(ExtensionConfiguration extConfig)
        {
            ChangeConfigurationInput changeConfigInput = new ChangeConfigurationInput
            {
                Configuration = Deployment.Configuration,
                ExtendedProperties = Deployment.ExtendedProperties,
                ExtensionConfiguration = Deployment.ExtensionConfiguration = extConfig,
                Mode = "Auto",
                TreatWarningsAsError = false
            };
            ExecuteClientActionInOCS(null, CommandRuntime.ToString(), s => Channel.ChangeConfigurationBySlot(s, ServiceName, Slot, changeConfigInput));
        }

        private void DisableExtension()
        {
            ExtensionConfiguration extConfig = HostedServiceExtensionHelper.GetExtensionConfig(Deployment);
            extConfig = HostedServiceExtensionHelper.RemoveExtension(extConfig, Roles, Channel, CurrentSubscription.SubscriptionId, ServiceName, ExtensionNameSpace, ExtensionType);
            ChangeDeployment(extConfig);
        }

        private void EnableExtension()
        {
            int totalSwitchNum = 2;
            ExtensionConfiguration extConfig = HostedServiceExtensionHelper.GetExtensionConfig(Deployment);
            if (Roles != null && Roles.Length > 0)
            {
                foreach (string roleName in Roles)
                {
                    bool installed = false;
                    string extensionId = roleName + "-RDP-Ext";
                    for (int i = 0; i < totalSwitchNum && !installed; i++)
                    {
                        string checkExtensionId = extensionId + i;
                        if (!HostedServiceExtensionHelper.CheckExtension(extConfig, checkExtensionId))
                        {
                            WriteObject("Setting remote desktop configuration for " + roleName + ".");
                            installed = InstallExtension(Deployment, checkExtensionId);
                            extConfig = HostedServiceExtensionHelper.AddExtension(extConfig, roleName, checkExtensionId);
                        }
                    }

                    if (!installed)
                    {
                        WriteExceptionError(new Exception("Failed to set remote desktop for Role: " + roleName));
                        return;
                    }
                }
            }
            else
            {
                string extensionId = "Default-RDP-Ext";
                WriteObject("Setting default remote desktop configuration for all roles.");
                InstallExtension(Deployment, extensionId);
                extConfig = HostedServiceExtensionHelper.AddExtension(extConfig, extensionId);
            }
            ChangeDeployment(extConfig);
        }

        private void ExecuteCommand()
        {
            if (HostedServiceExtensionHelper.IsLegacySettingEnabled(Deployment, LegacySettingStr))
            {
                WriteExceptionError(new Exception("Legacy remote desktop already enabled. This cmdlet will abort."));
                return;
            }
            EnableExtension();
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
