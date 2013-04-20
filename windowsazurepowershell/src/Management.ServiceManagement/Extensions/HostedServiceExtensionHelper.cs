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
    using System.Xml;

    using WindowsAzure.ServiceManagement;

    internal class HostedServiceExtensionHelper
    {
        static internal bool IsLegacySettingEnabled(Deployment deployment, string settingAttributeName)
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
                                if (nameStr.Equals(settingAttributeName))
                                {
                                    enabled = String.Equals(childNode.Attributes["value"].Value, "true", StringComparison.OrdinalIgnoreCase);
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Error - Cannot parse the Xml of Configuration
                    throw new Exception("Cannot determine deployment legacy setting - configuration parsing error: " + ex.Message);
                }
            }
            return enabled;
        }

        static internal HostedServiceExtension GetExtension(IServiceManagement channel, string subscriptionId, string serviceName, string extensionId)
        {
            if (channel == null)
            {
                return null;
            }
            return channel.ListHostedServiceExtensions(subscriptionId, serviceName).Find(e => e.Id == extensionId);
        }

        static internal bool CheckExtension(ExtensionConfiguration inConfig, string[] roles, IServiceManagement channel, string subscriptionId, string serviceName, string extensionNameSpace, string extensionType)
        {
            ExtensionConfiguration extConfig = NewExtensionConfig(inConfig);
            if (roles != null && roles.Length > 0)
            {
                foreach (string roleName in roles)
                {
                    RoleExtensions roleExtensions = extConfig.NamedRoles.Find(r => r.RoleName == roleName);
                    if (roleExtensions != null)
                    {
                        foreach (Extension ext in roleExtensions.Extensions)
                        {
                            HostedServiceExtension extension = GetExtension(channel, subscriptionId, serviceName, ext.Id);
                            if (extension != null && extension.ProviderNameSpace == extensionNameSpace && extension.Type == extensionType)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Extension ext in extConfig.AllRoles)
                {
                    HostedServiceExtension extension = HostedServiceExtensionHelper.GetExtension(channel, subscriptionId, serviceName, ext.Id);
                    if (extension != null && extension.Type == extensionType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static internal ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig, string[] roles, IServiceManagement channel, string subscriptionId, string serviceName, string extensionNameSpace, string extensionType)
        {
            ExtensionConfiguration extConfig = NewExtensionConfig(inConfig);
            if (roles != null && roles.Length > 0)
            {
                foreach (string roleName in roles)
                {
                    RoleExtensions roleExtensions = extConfig.NamedRoles.Find(r => r.RoleName == roleName);
                    if (roleExtensions != null)
                    {
                        foreach (Extension ext in roleExtensions.Extensions)
                        {
                            HostedServiceExtension extension = GetExtension(channel, subscriptionId, serviceName, ext.Id);
                            if (extension != null && extension.ProviderNameSpace == extensionNameSpace && extension.Type == extensionType)
                            {
                                extConfig = RemoveExtension(extConfig, roleName, extension.Id);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (Extension ext in extConfig.AllRoles)
                {
                    HostedServiceExtension extension = HostedServiceExtensionHelper.GetExtension(channel, subscriptionId, serviceName, ext.Id);
                    if (extension != null && extension.Type == extensionType)
                    {
                        extConfig = HostedServiceExtensionHelper.RemoveExtension(extConfig, extension.Id);
                    }
                }
            }
            return extConfig;
        }

        static internal ExtensionConfiguration NewExtensionConfig()
        {
            return NewExtensionConfig(null);
        }

        static internal ExtensionConfiguration NewExtensionConfig(ExtensionConfiguration inConfig)
        {
            ExtensionConfiguration outConfig = new ExtensionConfiguration
            {
                AllRoles = new AllRoles(),
                NamedRoles = new NamedRoles()
            };
            if (inConfig != null)
            {
                if (inConfig.AllRoles != null)
                {
                    outConfig.AllRoles.AddRange(inConfig.AllRoles);
                }
                if (inConfig.NamedRoles != null)
                {
                    outConfig.NamedRoles.AddRange(inConfig.NamedRoles);
                }
            }
            return outConfig;
        }

        static internal ExtensionConfiguration GetExtensionConfig(Deployment deployment)
        {
            if (deployment != null && deployment.ExtensionConfiguration != null)
            {
                return NewExtensionConfig(deployment.ExtensionConfiguration);
            }
            else
            {
                return new ExtensionConfiguration
                {
                    AllRoles = new AllRoles(),
                    NamedRoles = new NamedRoles()
                };
            }
        }

        static internal bool CheckExtension(ExtensionConfiguration inConfig, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            return outConfig.AllRoles.Any(ext => ext.Id == extensionId);
        }

        static internal bool CheckExtension(ExtensionConfiguration inConfig, string roleName, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            RoleExtensions roleExtensions = outConfig.NamedRoles.Find(r => r.RoleName == roleName);
            return roleExtensions != null ? roleExtensions.Extensions.Any(ext => ext.Id == extensionId) : false;
        }

        static internal ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            outConfig.AllRoles.RemoveAll(ext => ext.Id == extensionId);
            return outConfig;
        }

        static internal ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig, string roleName, string extensionId)
        {
            return RemoveExtension(inConfig, new string[1] { roleName }, extensionId);
        }

        static internal ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig, string[] roleNames, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            if (roleNames != null && roleNames.Length > 0)
            {
                foreach (string roleName in roleNames)
                {
                    RoleExtensions roleExtensions = outConfig.NamedRoles.Find(r => r.RoleName == roleName);
                    if (roleExtensions != null)
                    {
                        roleExtensions.Extensions.RemoveAll(ext => ext.Id == extensionId);
                    }
                }
            }
            return outConfig;
        }

        static internal ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            outConfig.AllRoles.Add(new Extension(extensionId));
            return outConfig;
        }

        static internal ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, string roleName, string extensionId)
        {
            return AddExtension(inConfig, new string[1] { roleName }, extensionId);
        }

        static internal ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, string[] roleNames, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            if (roleNames != null && roleNames.Length > 0)
            {
                foreach (string roleName in roleNames)
                {
                    RoleExtensions roleExtensions = outConfig.NamedRoles.Find(r => r.RoleName == roleName);
                    if (roleExtensions != null)
                    {
                        roleExtensions.Extensions.Add(new Extension(extensionId));
                    }
                    else
                    {
                        outConfig.NamedRoles.Add(new RoleExtensions
                        {
                            RoleName = roleName,
                            Extensions = new ExtensionList(new Extension[1] { new Extension(extensionId) } )
                        });
                    }
                }
            }
            return outConfig;
        }
    }
}
