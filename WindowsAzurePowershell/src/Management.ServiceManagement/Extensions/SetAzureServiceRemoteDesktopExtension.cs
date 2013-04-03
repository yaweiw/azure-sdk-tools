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
    using Cmdlets.Common;
    using Management.Model;
    using WindowsAzure.ServiceManagement;
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    /// Update Windows Azure Remote Desktop Extensions.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "AzureServiceRemoteDesktopExtension"), OutputType(typeof(ManagementOperationContext))]
    public class SetAzureServiceRemoteDesktopExtensionCommand : ServiceManagementBaseCmdlet
    {
        private const string ProductionSlotStr = "Production";
        private const string StagingSlotStr = "Staging";

        private const string NewExtParamSetStr = "NewExtension";
        private const string ExistingExtParamSetStr = "ExistingExtension";
        private const string RemoveExtParamSetStr = "RemoveExtension";
        private const string InstallExtParamSetStr = "InstallExtension";
        private const string UninstallExtParamSetStr = "UninstallExtension";

        private const string RemoteDesktopExtTypeStr = "RDP";

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

        [Parameter(Position = 0, Mandatory = true, ParameterSetName = NewExtParamSetStr, HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = ExistingExtParamSetStr, HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = RemoveExtParamSetStr, HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = InstallExtParamSetStr, HelpMessage = "Cloud Service Name")]
        [Parameter(Position = 0, Mandatory = true, ParameterSetName = UninstallExtParamSetStr, HelpMessage = "Cloud Service Name")]
        [ValidateNotNullOrEmpty]
        public string ServiceName
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Cloud Service Role Name")]
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = ExistingExtParamSetStr, HelpMessage = "Cloud Service Role Name")]
        [Parameter(Position = 1, Mandatory = false, ParameterSetName = RemoveExtParamSetStr, HelpMessage = "Cloud Service Role Name")]
        [ValidateNotNullOrEmpty]
        public string[] Role
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

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = InstallExtParamSetStr, HelpMessage = "Install Remote Desktop Extensions")]
        public SwitchParameter Install
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = UninstallExtParamSetStr, HelpMessage = "Uninstall Remote Desktop Extensions")]
        public SwitchParameter Uninstall
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = NewExtParamSetStr, HelpMessage = "Remote Desktop User Name")]
        [Parameter(Position = 3, Mandatory = true, ParameterSetName = InstallExtParamSetStr, HelpMessage = "Remote Desktop User Name")]
        [ValidateNotNullOrEmpty]
        public string UserName
        {
            get;
            set;
        }

        [Parameter(Position = 3, Mandatory = true, ParameterSetName = NewExtParamSetStr, HelpMessage = "Remote Desktop User Password")]
        [Parameter(Position = 4, Mandatory = true, ParameterSetName = InstallExtParamSetStr, HelpMessage = "Remote Desktop User Password")]
        [ValidateNotNullOrEmpty]
        public string Password
        {
            get;
            set;
        }

        [Parameter(Position = 4, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Remote Desktop User Expiration Date")]
        [Parameter(Position = 5, Mandatory = false, ParameterSetName = InstallExtParamSetStr, HelpMessage = "Remote Desktop User Expiration Date")]
        [ValidateNotNullOrEmpty]
        public DateTime Expiration
        {
            get;
            set;
        }

        [Parameter(Position = 5, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Remote Desktop Extension ID")]
        [Parameter(Position = 1, Mandatory = true, ParameterSetName = InstallExtParamSetStr, HelpMessage = "Remote Desktop Extension ID")]
        [ValidateNotNullOrEmpty]
        public string NewExtensionID
        {
            get;
            set;
        }

        [Parameter(Position = 2, Mandatory = true, ParameterSetName = ExistingExtParamSetStr, HelpMessage = "RDP Extension ID")]
        [ValidateNotNullOrEmpty]
        public string ExistingExtensionID
        {
            get;
            set;
        }

        [Parameter(Position = 1, Mandatory = false, ParameterSetName = UninstallExtParamSetStr, HelpMessage = "RDP Extension ID")]
        [ValidateNotNullOrEmpty]
        public string[] InstalledExtensionIDs
        {
            get;
            set;
        }

        [Parameter(Position = 6, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Overwrite Existing RDP Extension with the Same ID")]
        [Parameter(Position = 7, Mandatory = false, ParameterSetName = InstallExtParamSetStr, HelpMessage = "Overwrite Existing RDP Extension with the Same ID")]
        public SwitchParameter Overwrite
        {
            get;
            set;
        }

        [Parameter(Position = 7, Mandatory = false, ParameterSetName = NewExtParamSetStr, HelpMessage = "Deployment Slot - Production | Staging. Default Production.")]
        [Parameter(Position = 3, Mandatory = false, ParameterSetName = ExistingExtParamSetStr, HelpMessage = "Deployment Slot - Production | Staging. Default Production.")]
        [Parameter(Position = 3, Mandatory = false, ParameterSetName = RemoveExtParamSetStr, HelpMessage = "Deployment Slot - Production | Staging. Default Production.")]
        [Parameter(Position = 8, Mandatory = false, ParameterSetName = InstallExtParamSetStr, HelpMessage = "Deployment Slot - Production | Staging. Default Production.")]
        [Parameter(Position = 3, Mandatory = false, ParameterSetName = UninstallExtParamSetStr, HelpMessage = "Deployment Slot - Production | Staging. Default Production.")]
        [ValidateSet("Staging", "Production", IgnoreCase = true)]
        public string Slot
        {
            get;
            set;
        }

        private bool IsServiceAvailable()
        {
            // Check that cloud service exists
            bool found = false;
            InvokeInOperationContext(() =>
            {
                this.RetryCall(s => found = !Channel.IsDNSAvailable(CurrentSubscription.SubscriptionId, ServiceName).Result);
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
                                if (nameStr.Equals("Microsoft.WindowsAzure.Plugins.RemoteAccess.Enabled"))
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

        private DateTime GetExpiration()
        {
            return Expiration.Equals(DateTime.MinValue) ? DateTime.Now.AddMonths(12) : Expiration;
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

        private void AddHostedServiceExtension(string extensionId)
        {
            HostedServiceExtensionInput hostedSvcExtInput = CreateHostedServiceExtensionInput(extensionId);
            Channel.AddHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, hostedSvcExtInput);
        }

        private HostedServiceExtensionInput CreateHostedServiceExtensionInput(string extensionId)
        {
            // Construct the Input
            HostedServiceExtensionInput hostedSvcExtInput = null;
            
            hostedSvcExtInput = new HostedServiceExtensionInput
            {
                ProviderNameSpace = "Microsoft.Windows.Azure.Extensions",
                Type = RemoteDesktopExtTypeStr,
                Id = extensionId,
                Thumbprint = "",
                ThumbprintAlgorithm = "",
                PublicConfiguration = string.Format(PublicConfigurationTemplateStr, UserName, GetExpiration().ToString("yyyy-MM-dd")),
                PrivateConfiguration = string.Format(PrivateConfigurationTemplate, Password)
            };
            return hostedSvcExtInput;
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
            ExtensionConfiguration newConfig = new ExtensionConfiguration
            {
                AllRoles = new AllRoles(),
                NamedRoles = new NamedRoles()
            };

            ExtensionConfiguration oldConfig = deployment.ExtensionConfiguration;
            Extension oldRdpExtension = null;
            if (oldConfig != null && oldConfig.AllRoles != null)
            {
                // Record the old RDP extension on All Roles
                oldRdpExtension = oldConfig.AllRoles.Find(ext => GetHostedServiceExtension(ext.Id).Type == RemoteDesktopExtTypeStr);
                // Remove all RDP extensions, and preserve all non-RDP extensions on All Roles
                newConfig.AllRoles.AddRange(oldConfig.AllRoles.FindAll(ext => GetHostedServiceExtension(ext.Id).Type != RemoteDesktopExtTypeStr));
            }

            if (Role == null)
            {
                // Configure for All Roles
                if (oldConfig != null && oldConfig.NamedRoles != null)
                {
                    // Remove all RDP extensions from Named Roles
                    foreach (RoleExtensions roleExts in oldConfig.NamedRoles)
                    {
                        RoleExtensions newRoleExts = new RoleExtensions();
                        newRoleExts.RoleName = roleExts.RoleName;
                        newRoleExts.Extensions = new ExtensionList();
                        newRoleExts.Extensions.AddRange(roleExts.Extensions.FindAll(ext => GetHostedServiceExtension(ext.Id).Type != RemoteDesktopExtTypeStr));
                        newConfig.NamedRoles.Add(newRoleExts);
                    }
                }

                if (extConfigType == ExtensionConfigurationType.Enable)
                {
                    // To enable new extension on All Roles
                    newConfig.AllRoles.Add(new Extension(extensionId));
                }
                else
                {
                    // To disable, do not add any RDP extensions
                    // Do nothing
                }
            }
            else
            {
                // Configure for specified Named Roles
                if (oldConfig != null && oldConfig.NamedRoles != null)
                {
                    // Remove all RDP extensions from Named Roles
                    foreach (RoleExtensions roleExts in oldConfig.NamedRoles)
                    {
                        if (Role.Contains(roleExts.RoleName))
                        {
                            RoleExtensions newRoleExts1 = new RoleExtensions();
                            newRoleExts1.RoleName = roleExts.RoleName;
                            newRoleExts1.Extensions = new ExtensionList();
                            newRoleExts1.Extensions.AddRange(roleExts.Extensions.FindAll(ext => GetHostedServiceExtension(ext.Id).Type != RemoteDesktopExtTypeStr));
                            if (extConfigType == ExtensionConfigurationType.Enable)
                            {
                                // If it is to enable extension on this role, add it; otherwise, let it be removed.
                                newRoleExts1.Extensions.Add(new Extension(extensionId));
                            }
                            // Append it to new configuration
                            newConfig.NamedRoles.Add(newRoleExts1);
                        }
                        else
                        {
                            // Do not change the role that is not specified; assign the old RDP extension back, if any.
                            RoleExtensions newRoleExts2 = new RoleExtensions();
                            newRoleExts2.RoleName = roleExts.RoleName;
                            newRoleExts2.Extensions = new ExtensionList();
                            if (oldRdpExtension != null)
                            {
                                newRoleExts2.Extensions.AddRange(roleExts.Extensions.FindAll(ext => GetHostedServiceExtension(ext.Id).Type != RemoteDesktopExtTypeStr));
                                newRoleExts2.Extensions.Add(new Extension(oldRdpExtension.Id));
                            }
                            else
                            {
                                newRoleExts2.Extensions.AddRange(roleExts.Extensions);
                            }
                            // Append it to new configuration
                            newConfig.NamedRoles.Add(newRoleExts2);
                        }
                    }

                    // For all roles in deployment
                    foreach (Role role in deployment.RoleList)
                    {
                        if (Role.Contains(role.RoleName))
                        {
                            if (extConfigType == ExtensionConfigurationType.Enable)
                            {
                                // Check newly specified roles that are not in prevous configuration
                                if (!oldConfig.NamedRoles.Exists(roleExts => roleExts.RoleName == role.RoleName))
                                {
                                    // If a specified role does not exist in the current list of Name Roles, add it.
                                    RoleExtensions roleExts = new RoleExtensions();
                                    roleExts.RoleName = role.RoleName;
                                    roleExts.Extensions = new ExtensionList();
                                    roleExts.Extensions.Add(new Extension(extensionId));
                                    // Append it to new configuration
                                    newConfig.NamedRoles.Add(roleExts);
                                }
                            }
                        }
                        else if (!oldConfig.NamedRoles.Exists(roleExts => roleExts.RoleName == role.RoleName))
                        {
                            // Check roles that are neither previously specified, nor newly specified.
                            if (oldRdpExtension != null)
                            {
                                // Apply RDP extension on them, if previously enabled on All Roles
                                RoleExtensions newRoleExts3 = new RoleExtensions();
                                newRoleExts3.RoleName = role.RoleName;
                                newRoleExts3.Extensions = new ExtensionList();
                                newRoleExts3.Extensions.Add(new Extension(oldRdpExtension.Id));
                                // Append it to new configuration
                                newConfig.NamedRoles.Add(newRoleExts3);
                            }
                        }
                    }
                }
            }

            // Remove all items with empty extension list
            newConfig.NamedRoles.RemoveAll(roleExts => roleExts.Extensions.Count == 0);

            return newConfig;
        }

        private ChangeConfigurationInput CreateChangeDeploymentInput(Deployment deployment, ExtensionConfiguration extConfig)
        {
            if (deployment == null || extConfig == null)
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

        private bool InstallExtension(Deployment deployment, string extensionId)
        {
            // Check whether there is an existing one
            HostedServiceExtension extension = GetHostedServiceExtension(extensionId);
            if (extension == null)
            {
                AddHostedServiceExtension(extensionId);
            }
            else
            {
                if (extension.Type == RemoteDesktopExtTypeStr)
                {
                    if (Overwrite.IsPresent)
                    {
                        DisableExtension(deployment, Role);
                        DeleteHostedServieExtension(extensionId);
                        AddHostedServiceExtension(extensionId);
                    }
                    else
                    {
                        // Error - Need to sepcify -Overwrite
                        WriteExceptionError(new Exception("Error: A RDP extension with ID: \'" + extensionId
                            + "\' already exists. Need to specify the \"-Overwrite\" parameter to overwrite it."));
                        return false;
                    }
                }
                else
                {
                    // Error - An existing extension found with a non-RDP type, so we cannot overwrite it.
                    WriteExceptionError(new Exception("Error: An existing extension with ID: \'" + extensionId
                        + "\' and Type: \'" + extension.Type + "\' found. It cannot be overwritten using this command."));
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
                if (existingExt.Type == RemoteDesktopExtTypeStr)
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

        private void DisableExtension(Deployment deployment, string[] roleNames)
        {
            /*
            ExtensionConfiguration extConfig = GetExtensionConfiguration(deployment);
            HostedServiceExtensionList extList = Channel.ListHostedServiceExtensions(CurrentSubscription.SubscriptionId, ServiceName);

            int removedItemCount = 0;

            if (roleNames == null)
            {
                // Remove extension mappings for all roles
                removedItemCount = extConfig.AllRoles.RemoveAll(
                    delegate(Extension ext)
                    {
                        HostedServiceExtension item = extList.Find(
                            delegate(HostedServiceExtension hostedExt)
                            {
                                return hostedExt.Id == ext.Id;
                            }
                        );
                        return item != null && item.Type == RemoteDesktopExtTypeStr;
                    }
                );

                // Also remove extension mappings for named roles
                foreach (RoleExtensions namedRoleItem in extConfig.NamedRoles)
                {
                    removedItemCount += namedRoleItem.Extensions.RemoveAll(
                        delegate(Extension ext)
                        {
                            HostedServiceExtension item = extList.Find(
                                delegate(HostedServiceExtension hostedExt)
                                {
                                    return hostedExt.Id == ext.Id;
                                }
                                );
                            return item != null && item.Type == RemoteDesktopExtTypeStr;
                        }
                    );
                }

                // Remove role-extensions elements with empty extension list
                extConfig.NamedRoles.RemoveAll(roleExts => roleExts.Extensions.Count == 0);
            }
            else
            {
                // If RDP already enabled for all roles, need to break its mappings into named roles
                Extension allRolesExt = extConfig.AllRoles.Find(
                        delegate(Extension ext)
                        {
                            HostedServiceExtension item = extList.Find(hostedExt => hostedExt.Id == ext.Id);
                            return item != null && item.Type == RemoteDesktopExtTypeStr;
                        });

                if (allRolesExt != null)
                {
                    // Need to do re-mappings
                    RoleList roleList = deployment.RoleList;
                    foreach (Role role in roleList)
                    {
                        RoleExtensions foundRoleExts = null;
                        foreach (RoleExtensions roleExts in extConfig.NamedRoles)
                        {
                            if (roleExts.RoleName == role.RoleName)
                            {
                                foundRoleExts = roleExts;
                            }
                        }
                        if (foundRoleExts != null)
                        {
                            foundRoleExts.Extensions.Add(new Extension(allRolesExt.Id));
                        }
                        else
                        {
                            ExtensionList newExtList = new ExtensionList();
                            newExtList.Add(new Extension(allRolesExt.Id));
                            extConfig.NamedRoles.Add(new RoleExtensions
                            {
                                RoleName = role.RoleName,
                                Extensions = newExtList
                            });
                        }
                    }
                }

                // Remove extension mapping for named roles
                foreach (RoleExtensions namedRoleItem in extConfig.NamedRoles)
                {
                    foreach (string roleNameStr in roleNames)
                    {
                        // Only remove the one with specified role name
                        if (roleNameStr == namedRoleItem.RoleName)
                        {
                            removedItemCount += namedRoleItem.Extensions.RemoveAll(
                                delegate(Extension ext)
                                {
                                    HostedServiceExtension item = extList.Find(hostedExt => hostedExt.Id == ext.Id);
                                    return item != null && item.Type == RemoteDesktopExtTypeStr;
                                }
                            );
                        }
                    }
                }

                // Remove role-extensions elements with empty extension list
                extConfig.NamedRoles.RemoveAll(roleExts => roleExts.Extensions.Count == 0);
            }
            */
            ExtensionConfiguration extConfig = CreateExtensionConfiguration(deployment, ExtensionConfigurationType.Disable);
            UpdateDeployment(deployment, extConfig);
        }

        private void UninstallExtension(Deployment deployment, string[] installedExtIdList)
        {
            if (installedExtIdList == null)
            {
                HostedServiceExtensionList hostedSvcExtList = Channel.ListHostedServiceExtensions(CurrentSubscription.SubscriptionId, ServiceName);
                foreach (HostedServiceExtension ext in hostedSvcExtList)
                {
                    if (ext.Type == RemoteDesktopExtTypeStr)
                    {
                        Channel.DeleteHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, ext.Id);
                    }
                }
            }
            else
            {
                foreach (string extensionId in installedExtIdList)
                {
                    Channel.DeleteHostedServiceExtension(CurrentSubscription.SubscriptionId, ServiceName, extensionId);
                }
            }
        }

        private void ExecuteCommand()
        {
            if (!IsServiceAvailable())
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

            if (Role != null)
            {
                foreach (string roleName in Role)
                {
                    if (!deployment.RoleList.Exists(role => role.RoleName == roleName))
                    {
                        // Exception
                        WriteExceptionError(new Exception("Role not found: " + roleName));
                        return;
                    }

                    if (Role.Count(role => role == roleName) > 1)
                    {
                        // Exception
                        WriteExceptionError(new Exception("Cannot specify duplicate role names: " + roleName));
                        return;
                    }
                }
            }

            if (IsLegacyRemoteDesktopEnabled(deployment))
            {
                // Exception
                WriteExceptionError(new Exception("Legacy remote desktop already enabled. This command will abort."));
                return;
            }

            if (string.Compare(ParameterSetName, InstallExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                InstallExtension(deployment, NewExtensionID);
            }
            else if (string.Compare(ParameterSetName, NewExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                NewExtension(deployment, NewExtensionID);
            }
            else if (string.Compare(ParameterSetName, ExistingExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                EnableExtension(deployment, ExistingExtensionID);
            }
            else if (string.Compare(ParameterSetName, RemoveExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                DisableExtension(deployment, Role);
            }
            else if (string.Compare(ParameterSetName, UninstallExtParamSetStr, StringComparison.OrdinalIgnoreCase) == 0)
            {
                UninstallExtension(deployment, InstalledExtensionIDs);
            }
        }

        protected override void OnProcessRecord()
        {
            ExecuteCommand();
        }
    }
}
