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
    /// Update Windows Azure Remote Desktop Extensions.
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

        private const string PrivateConfigurationTemplate =   "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                                                              "<?xml-stylesheet type=\"text/xsl\" href=\"style.xsl\"?>" +
                                                              "<PrivateConfig>" +
                                                              "<Password>{0}</Password>" +
                                                              "</PrivateConfig>";

        private enum ExtensionConfigurationType { Enable, Disable };

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

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Extension Id. Default to RDPExtDefault.")]
        [ValidateNotNullOrEmpty]
        public string ExtensionId
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = RemoveExtParamSetStr, HelpMessage = "Disable Remote Desktop Extensions")]
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

        [Parameter(Position = 7, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Deployment Slot: Production | Staging. Default Production.")]
        [Parameter(Position = 3, Mandatory = false, ParameterSetName = RemoveExtParamSetStr, HelpMessage = "Deployment Slot: Production | Staging. Default Production.")]
        [ValidateSet("Staging", "Production", IgnoreCase = true)]
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
                    WriteExceptionError(new Exception("Deployment configuration parsing error: " + ex.Message));
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
            return Expiration.Equals(DateTime.MinValue) ? DateTime.Now.AddMonths(12) : Expiration;
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

        private string GetExtensionId(string inExtensionId)
        {
            return string.IsNullOrEmpty(inExtensionId) ? "RDPExtDefault" : inExtensionId;
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

        private HostedServiceExtension GetHostedServiceExtension(string extensionId)
        {
            return Channel.ListHostedServiceExtensions(CurrentSubscription.SubscriptionId, ServiceName).Find(ext => ext.Id == extensionId);
        }

        private void DeleteHostedServieExtension(string extensionId)
        {
            Channel.DeleteHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extensionId);
        }

        private void AddHostedServiceExtension(string extensionId)
        {
            HostedServiceExtensionInput hostedSvcExtInput = CreateHostedServiceExtensionInput(extensionId);
            Channel.AddHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, hostedSvcExtInput);
        }

        private ExtensionConfiguration GetExtensionConfiguration(Deployment deployment)
        {
            return deployment.ExtensionConfiguration;
        }

        private ExtensionConfiguration CreateExtensionConfiguration(Deployment deployment, ExtensionConfigurationType extConfigType)
        {
            return CreateExtensionConfiguration(deployment, null, extConfigType);
        }

        private ExtensionConfiguration CreateExtensionConfiguration(Deployment deployment, string extensionId, ExtensionConfigurationType extConfigType)
        {
            if (deployment == null)
            {
                return null;
            }

            if (deployment.ExtensionConfiguration == null)
            {
                deployment.ExtensionConfiguration = new ExtensionConfiguration();
            }

            deployment.ExtensionConfiguration.AllRoles.RemoveAll(ext => GetHostedServiceExtension(ext.Id) != null && GetHostedServiceExtension(ext.Id).Type == RdpExtensionTypeStr);
            if (extConfigType == ExtensionConfigurationType.Enable)
            {
                deployment.ExtensionConfiguration.AllRoles.Add(new Extension(extensionId));
            }

            return deployment.ExtensionConfiguration;
        }

        private ChangeConfigurationInput CreateChangeDeploymentInput(Deployment deployment, ExtensionConfiguration extConfig)
        {
            if (deployment == null)
            {
                return null;
            }

            ChangeConfigurationInput changeConfigInput = new ChangeConfigurationInput();
            changeConfigInput.Configuration = deployment.Configuration;
            changeConfigInput.TreatWarningsAsError = false;
            changeConfigInput.Mode = "Auto";
            // Extended Properties
            ExtendedPropertiesList extendedProperties = new ExtendedPropertiesList();
            if (deployment.ExtendedProperties != null)
            {
                foreach (ExtendedProperty property in deployment.ExtendedProperties)
                {
                    extendedProperties.Add(new ExtendedProperty()
                    {
                        Name = property.Name,
                        Value = property.Value,
                    });
                }
            }
            changeConfigInput.ExtendedProperties = extendedProperties;
            // Extension Configuration
            changeConfigInput.ExtensionConfiguration = extConfig;
            return changeConfigInput;
        }

        private void UpdateDeployment(Deployment deployment, ExtensionConfiguration extConfig)
        {
            if (deployment == null || extConfig == null)
            {
                return;
            }
            ChangeConfigurationInput changeConfigInput = CreateChangeDeploymentInput(deployment, extConfig);
            Channel.ChangeConfigurationBySlot(CurrentSubscription.SubscriptionId, ServiceName, GetSlot(), changeConfigInput);
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
            return "Removing Default remote desktop configuration for all roles from Cloud Service " + ServiceName;
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
                if (extension.Type == RdpExtensionTypeStr)
                {
                    WriteObject(GetOverwriteExtensionMessage());
                    DisableExtension(deployment);
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

        private bool NewExtension(Deployment deployment, string extensionId)
        {
            if (!InstallExtension(deployment, extensionId))
            {
                return false;
            }
            return EnableExtension(deployment, extensionId);
        }

        private bool EnableExtension(Deployment deployment, string extensionId)
        {
            HostedServiceExtension existingExt = GetHostedServiceExtension(extensionId);
            if (existingExt != null)
            {
                if (existingExt.Type == RdpExtensionTypeStr)
                {
                    ExtensionConfiguration extConfig = CreateExtensionConfiguration(deployment, extensionId, ExtensionConfigurationType.Enable);
                    UpdateDeployment(deployment, extConfig);
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
                WriteExceptionError(new Exception("Error: No existing hosted extension with ID \'"
                    + extensionId + "\' found."));
                return false;
            }
            return true;
        }

        private void DisableExtension(Deployment deployment)
        {
            ExtensionConfiguration extConfig = CreateExtensionConfiguration(deployment, ExtensionConfigurationType.Disable);
            UpdateDeployment(deployment, extConfig);
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

            if (string.Compare(ParameterSetName, NewExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                ExtensionId = GetExtensionId(ExtensionId);
                NewExtension(deployment, ExtensionId);
            }
            else if (string.Compare(ParameterSetName, RemoveExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                DisableExtension(deployment);
            }
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}
