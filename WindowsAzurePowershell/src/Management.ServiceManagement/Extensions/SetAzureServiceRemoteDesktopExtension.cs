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
    using System.Xml;

    using WindowsAzure.Management.Utilities.CloudService;
    using WindowsAzure.Management.Utilities.Common;
    using WindowsAzure.ServiceManagement;

    /// <summary>
    /// Set Windows Azure Remote Desktop Extensions.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceRemoteDesktopExtension"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceRemoteDesktopExtensionCommand : ServiceManagementBaseCmdlet
    {
        private const string ProductionSlotStr = "Production";
        private const string StagingSlotStr = "Staging";

        private const string NewExtParamSetStr = "NewExtension";
        private const string RemoveExtParamSetStr = "RemoveExtension";

        private const string RdpExtensionTypeStr = "RDP";
        private const string RdpExtensionNameSpaceStr = "Microsoft.Windows.Azure.Extensions";
        private const string RdpLegacySettingStr = "Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled";

        private const string PublicConfigurationTemplateStr = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
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

        [Parameter(Position = 0, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = false, ParameterSetName = RemoveExtParamSetStr, HelpMessage = "Cloud Service Name")]
        public string ServiceName
        {
            get;
            set;
        }

        private string ExtensionId
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = true, ParameterSetName = RemoveExtParamSetStr, HelpMessage = "Disable Remote Desktop Extensions")]
        public SwitchParameter Remove
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = NewExtParamSetStr, HelpMessage = "Remote Desktop User Name")]
        [ValidateNotNullOrEmpty]
        public string UserName
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ParameterSetName = NewExtParamSetStr, HelpMessage = "Remote Desktop User Password")]
        [ValidateNotNullOrEmpty]
        public string Password
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Remote Desktop User Expiration Date")]
        [ValidateNotNullOrEmpty]
        public DateTime Expiration
        {
            get;
            set;
        }

        [Parameter(Position = 5, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Deployment Slot: Production | Staging. Default Production.")]
        [Parameter(Position = 2, Mandatory = false, ParameterSetName = RemoveExtParamSetStr, HelpMessage = "Deployment Slot: Production | Staging. Default Production.")]
        [ValidateSet(ProductionSlotStr, StagingSlotStr, IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        private string GetServiceName(string rootPath, string inServiceName, string subscriptionId)
        {
            string serviceName;
            ServiceSettings settings = General.GetDefaultSettings(
                rootPath,
                inServiceName,
                null,
                null,
                null,
                null,
                subscriptionId,
                out serviceName);
            return serviceName;
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

        private bool IsLegacyRemoteDesktopEnabled(Deployment deployment)
        {
            bool enabled = false;
            if (deployment != null && deployment.Configuration != null)
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(deployment.Configuration);
                    XmlNodeList xmlNodeList = xmlDoc.GetElementsByTagName("ConfigurationSettings");
                    if (xmlNodeList.Count > 0)
                    {
                        XmlNode parentNode = xmlNodeList.Item(0);
                        foreach (XmlNode childNode in parentNode)
                        {
                            if (childNode.Attributes["name"] != null && childNode.Attributes["value"] != null)
                            {
                                string nameStr = childNode.Attributes["name"].Value;
                                string valueStr = childNode.Attributes["value"].Value;
                                if (nameStr.Equals(RdpLegacySettingStr))
                                {
                                    enabled = 0 == String.Compare(childNode.Attributes["value"].Value, "true", StringComparison.OrdinalIgnoreCase);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Error - Cannot parse the Xml of Configuration
                    WriteExceptionError(new Exception("Deployment configuration parsing error: " + ex.Message
                        + ". Cannot determine the legacy RDP settings."));
                }
            }
            return enabled;
        }

        private Deployment GetDeployment()
        {
            return Channel.GetDeploymentBySlot(CurrentSubscription.SubscriptionId, ServiceName, string.IsNullOrEmpty(Slot) ? ProductionSlotStr : Slot);
        }

        private string GetExtensionType()
        {
            return RdpExtensionTypeStr;
        }

        private string GetNameSpace()
        {
            return RdpExtensionNameSpaceStr;
        }

        private DateTime GetExpiration()
        {
            return Expiration.Equals(default(DateTime)) ? DateTime.Now.AddMonths(12) : Expiration;
        }

        private string GetPublicConfiguration()
        {
            return string.Format(PublicConfigurationTemplateStr, UserName, GetExpiration().ToString("yyyy-MM-dd"));
        }

        private string GetPrivateConfiguration()
        {
            return string.Format(PrivateConfigurationTemplate, Password);
        }

        private string GetThumbprint()
        {
            return "";
        }

        private string GetThumbprintAlgorithm()
        {
            return "";
        }

        private string GetSlot()
        {
            return string.IsNullOrEmpty(Slot) ? ProductionSlotStr : Slot;
        }

        private HostedServiceExtension GetHostedServiceExtension(string extensionId)
        {
            return Channel.ListHostedServiceExtensions(CurrentSubscription.SubscriptionId, ServiceName).Find(ext => ext.Id == extensionId);
        }

        private void DeleteHostedServieExtension(string extensionId)
        {
            Channel.DeleteHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extensionId);
        }

        private HostedServiceExtensionInput CreateHostedServiceExtensionInput(string extensionId)
        {
            return new HostedServiceExtensionInput
            {
                ProviderNameSpace = GetNameSpace(),
                Type = GetExtensionType(),
                Id = extensionId,
                Thumbprint = GetThumbprint(),
                ThumbprintAlgorithm = GetThumbprintAlgorithm(),
                PublicConfiguration = GetPublicConfiguration(),
                PrivateConfiguration = GetPrivateConfiguration()
            };
        }

        private void AddHostedServiceExtension(string extensionId)
        {
            HostedServiceExtensionInput hostedSvcExtInput = CreateHostedServiceExtensionInput(extensionId);
            Channel.AddHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, hostedSvcExtInput);
        }

        private ExtensionConfiguration CreateExtensionConfiguration(Deployment deployment, string extensionId)
        {
            if (deployment == null)
            {
                return null;
            }

            ExtensionConfiguration newExtConfig = new ExtensionConfiguration();
            newExtConfig.AllRoles = new AllRoles();
            newExtConfig.NamedRoles = new NamedRoles();

            if (deployment.ExtensionConfiguration != null)
            {
                List<Extension> allNonRdpRoles = new List<Extension>();
                if (deployment.ExtensionConfiguration.AllRoles != null)
                {
                    deployment.ExtensionConfiguration.AllRoles.FindAll(
                        ext =>
                        {
                            var hostedSvcExt = GetHostedServiceExtension(ext.Id);
                            return hostedSvcExt != null && hostedSvcExt.Type != GetExtensionType();
                        });
                }
                // Copy all non-RDP extensions
                newExtConfig.AllRoles.AddRange(allNonRdpRoles);

                if (deployment.ExtensionConfiguration.NamedRoles != null)
                {
                    // Copy original NamedRoles settings
                    newExtConfig.NamedRoles.AddRange(deployment.ExtensionConfiguration.NamedRoles);
                }
            }

            if (extensionId != null)
            {
                // To enable RDP extension, add it to AllRoles
                newExtConfig.AllRoles.Add(new Extension(extensionId));
            }

            return newExtConfig;
        }

        private ChangeConfigurationInput CreateChangeDeploymentInput(Deployment deployment)
        {
            if (deployment == null)
            {
                return null;
            }
            ChangeConfigurationInput changeConfigInput = new ChangeConfigurationInput();
            // Copy Original Settings
            changeConfigInput.Configuration = deployment.Configuration;
            changeConfigInput.TreatWarningsAsError = false;
            changeConfigInput.Mode = "Auto";
            // Copy Extended Properties
            ExtendedPropertiesList extendedProperties = new ExtendedPropertiesList();
            if (deployment.ExtendedProperties != null)
            {
                extendedProperties.AddRange(deployment.ExtendedProperties);
            }
            changeConfigInput.ExtendedProperties = extendedProperties;
            // Update Extension Configuration
            changeConfigInput.ExtensionConfiguration = deployment.ExtensionConfiguration;
            return changeConfigInput;
        }

        private string GetNewExtensionMessage()
        {
            return "Setting default remote desktop configuration for all roles using ExtensionId " + ExtensionId;
        }

        private string GetOverwriteExtensionMessage()
        {
            return "Overwriting default remote desktop settings for all roles using ExtensionId " + ExtensionId;
        }

        private string GetRemoveExtensionMessage()
        {
            return "Removing default remote desktop configuration for all roles from Cloud Service " + ServiceName;
        }

        private bool InstallExtension(Deployment deployment, string extensionId)
        {
            // Check whether there is an existing one
            HostedServiceExtension extension = GetHostedServiceExtension(extensionId);
            if (extension == null)
            {
                WriteObject(GetNewExtensionMessage());
                AddHostedServiceExtension(extensionId);
            }
            else
            {
                if (extension.Type == GetExtensionType())
                {
                    WriteObject(GetOverwriteExtensionMessage());
                    DisableExtension(deployment, false);
                    Channel.ChangeConfigurationBySlot(CurrentSubscription.SubscriptionId, ServiceName, GetSlot(), CreateChangeDeploymentInput(deployment));
                    DeleteHostedServieExtension(extensionId);
                    AddHostedServiceExtension(extensionId);
                }
                else
                {
                    // Error - An existing extension found with a non-RDP type, so we cannot overwrite it.
                    WriteExceptionError(new Exception("A non-RDP configuration already exists with ID " + extensionId
                        + " and Type " + extension.Type + ". It cannot be overwritten using this command."));
                    return false;
                }
            }
            return true;
        }

        private bool EnableExtension(Deployment deployment, string extensionId)
        {
            HostedServiceExtension existingExt = GetHostedServiceExtension(extensionId);
            if (existingExt != null)
            {
                if (existingExt.Type == GetExtensionType())
                {
                    deployment.ExtensionConfiguration = CreateExtensionConfiguration(deployment, extensionId);
                }
                else
                {
                    // Error - An existing extension found with a non-RDP type, so we cannot overwrite it.
                    WriteExceptionError(new Exception("Error: An existing extension with ID: \'" + extensionId
                        + "\' and Type: \'" + existingExt.Type + "\' is found. "
                        + "Extensions with non-RDP types cannot be overwritten by this command."));
                    return false;
                }
            }
            else
            {
                // Error - No existing extension
                WriteExceptionError(new Exception("Error: No existing service extension with ID \'"
                    + extensionId + "\' found."));
                return false;
            }
            return true;
        }

        private bool NewExtension(Deployment deployment)
        {
            if (!InstallExtension(deployment, ExtensionId))
            {
                return false;
            }
            return EnableExtension(deployment, ExtensionId);
        }

        private bool DisableExtension(Deployment deployment, bool verbose)
        {
            if (verbose)
            {
                WriteObject(GetRemoveExtensionMessage());
            }
            deployment.ExtensionConfiguration = CreateExtensionConfiguration(deployment, null);
            return true;
        }

        private void ExecuteCommand()
        {
            ServiceName = GetServiceName(General.TryGetServiceRootPath(CurrentPath()), ServiceName, CurrentSubscription.SubscriptionId);

            if (!IsServiceAvailable(ServiceName))
            {
                WriteExceptionError(new Exception("Service not found: " + ServiceName));
                return;
            }

            Deployment deployment = GetDeployment();
            if (deployment == null)
            {
                // Exception
                WriteExceptionError(new Exception("Deployment not found: " + deployment.ToString()));
                return;
            }

            if (IsLegacyRemoteDesktopEnabled(deployment))
            {
                // Exception
                WriteExceptionError(new Exception("Legacy remote desktop already enabled. This command will abort."));
                return;
            }

            bool updated = false;

            if (string.Compare(ParameterSetName, NewExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                ExtensionId = string.IsNullOrEmpty(ExtensionId) ? "RDPExtDefault" : ExtensionId;
                updated = NewExtension(deployment);
            }
            else if (string.Compare(ParameterSetName, RemoveExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                ExtensionId = null;
                updated = DisableExtension(deployment, true);
            }

            if (updated)
            {
                ExecuteClientActionInOCS(null, CommandRuntime.ToString(),
                s => this.Channel.ChangeConfigurationBySlot(s, ServiceName, GetSlot(), CreateChangeDeploymentInput(deployment)));
            }
            else
            {
                // Exception
                WriteExceptionError(new Exception("Cannot set RDP extension."));
            }
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}
