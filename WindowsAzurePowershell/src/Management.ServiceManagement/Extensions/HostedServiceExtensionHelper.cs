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
        static internal int ExtensionIdLiveCycleCount = 2;

        protected const string ExtensionIdTemplate = "{0}-{1}-Ext-{2}";

        static internal bool ExistLegacySetting(Deployment deployment, string settingAttributeName)
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
                                    //enabled = String.Equals(childNode.Attributes["value"].Value, "true", StringComparison.OrdinalIgnoreCase);
                                    enabled = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
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

        static internal bool ExistDefaultExtension(ExtensionConfiguration inConfig, IServiceManagement channel, string subscriptionId, string serviceName, string extensionNameSpace, string extensionType)
        {
            ExtensionConfiguration extConfig = NewExtensionConfig(inConfig);
            foreach (Extension ext in extConfig.AllRoles)
            {
                HostedServiceExtension extension = HostedServiceExtensionHelper.GetExtension(channel, subscriptionId, serviceName, ext.Id);
                if (extension != null && extension.Type == extensionType)
                {
                    return true;
                }
            }
            return false;
        }

        static internal bool ExistExtension(ExtensionConfiguration inConfig, string[] roles, IServiceManagement channel, string subscriptionId, string serviceName, string extensionNameSpace, string extensionType)
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
                return ExistDefaultExtension(inConfig, channel, subscriptionId, serviceName, extensionNameSpace, extensionType);
            }
            return false;
        }

        static internal ExtensionConfiguration RemoveDefaultExtension(ExtensionConfiguration inConfig,
                                                               IServiceManagement channel,
                                                               string subscriptionId,
                                                               string serviceName,
                                                               string extensionNameSpace,
                                                               string extensionType)
        {
            ExtensionConfiguration extConfig = NewExtensionConfig(inConfig);
            foreach (Extension ext in extConfig.AllRoles)
            {
                HostedServiceExtension extension = HostedServiceExtensionHelper.GetExtension(channel, subscriptionId, serviceName, ext.Id);
                if (extension != null && extension.Type == extensionType)
                {
                    extConfig = HostedServiceExtensionHelper.RemoveDefaultExtension(extConfig, extension.Id);
                }
            }
            return extConfig;
        }

        static internal ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig,
                                                               string roleName,
                                                               IServiceManagement channel,
                                                               string subscriptionId,
                                                               string serviceName,
                                                               string extensionNameSpace,
                                                               string extensionType)
        {
            return RemoveExtension(inConfig, new string[1] { roleName }, channel, subscriptionId, serviceName, extensionNameSpace, extensionType);
        }

        static internal ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig,
                                                               string[] roles,
                                                               IServiceManagement channel,
                                                               string subscriptionId,
                                                               string serviceName,
                                                               string extensionNameSpace,
                                                               string extensionType)
        {
            ExtensionConfiguration extConfig = NewExtensionConfig(inConfig);
            if (roles != null && roles.Length > 0 && inConfig != null && inConfig.NamedRoles != null)
            {
                foreach (string roleName in roles)
                {
                    RoleExtensions inRoleExtensions = inConfig.NamedRoles.Find(r => r.RoleName == roleName);
                    if (inRoleExtensions != null)
                    {
                        foreach (Extension inExt in inRoleExtensions.Extensions)
                        {
                            HostedServiceExtension inExtension = GetExtension(channel, subscriptionId, serviceName, inExt.Id);
                            if (inExtension != null && inExtension.ProviderNameSpace == extensionNameSpace && inExtension.Type == extensionType)
                            {
                                extConfig = RemoveExtension(extConfig, roleName, inExtension.Id);
                            }
                        }
                    }
                }
            }
            else
            {
                return RemoveDefaultExtension(inConfig, channel, subscriptionId, serviceName, extensionNameSpace, extensionType);
            }
            return extConfig;
        }

        static internal ExtensionConfiguration RemoveDefaultExtension(ExtensionConfiguration inConfig, string extensionId)
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
            else
            {
                RemoveDefaultExtension(inConfig, extensionId);
            }
            return outConfig;
        }

        static internal ExtensionConfiguration NewExtensionConfig()
        {
            return new ExtensionConfiguration
            {
                AllRoles = new AllRoles(),
                NamedRoles = new NamedRoles()
            };
        }

        static internal ExtensionConfiguration NewExtensionConfig(ExtensionConfiguration inConfig)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig();
            if (inConfig != null)
            {
                if (inConfig.AllRoles != null)
                {
                    outConfig.AllRoles.AddRange(inConfig.AllRoles.Select(e => new Extension(e.Id)));
                }
                if (inConfig.NamedRoles != null)
                {
                    outConfig.NamedRoles.AddRange(inConfig.NamedRoles.Select(
                    r => new RoleExtensions
                    {
                        RoleName = r.RoleName,
                        Extensions = new ExtensionList(r.Extensions.Select(e => new Extension(e.Id)))
                    }));
                }
            }
            return outConfig;
        }

        static internal ExtensionConfiguration NewExtensionConfig(Deployment deployment)
        {
            if (deployment != null && deployment.ExtensionConfiguration != null)
            {
                return NewExtensionConfig(deployment.ExtensionConfiguration);
            }
            else
            {
                return NewExtensionConfig();
            }
        }

        static internal bool ExistDefaultExtension(ExtensionConfiguration inConfig, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            return outConfig.AllRoles.Any(ext => ext.Id == extensionId);
        }

        static internal bool ExistExtension(ExtensionConfiguration inConfig, string roleName, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            RoleExtensions roleExtensions = outConfig.NamedRoles.Find(r => r.RoleName == roleName);
            return roleExtensions != null ? roleExtensions.Extensions.Any(ext => ext.Id == extensionId) : false;
        }

        static internal ExtensionConfiguration AddDefaultExtension(ExtensionConfiguration inConfig, string extensionId)
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
                            Extensions = new ExtensionList(new Extension[1] { new Extension(extensionId) })
                        });
                    }
                }
            }
            else
            {
                outConfig = AddDefaultExtension(inConfig, extensionId);
            }
            return outConfig;
        }

        static internal ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, PSExtensionConfiguration psConfig, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            if (psConfig != null)
            {
                string[] roleNames = psConfig.NamedRoles;
                if (psConfig.AllRoles || roleNames == null || !roleNames.Any())
                {
                    outConfig.AllRoles.Add(new Extension(extensionId));
                }
                else
                {
                    foreach (string roleName in roleNames)
                    {
                        outConfig.NamedRoles.Add(new RoleExtensions
                        {
                            RoleName = roleName,
                            Extensions = new ExtensionList(new Extension[1] { new Extension(extensionId) })
                        });
                    }
                }
            }
            return outConfig;
        }

        static internal ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, ExtensionConfiguration addConfig)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            if (addConfig != null)
            {
                if (addConfig.AllRoles != null)
                {
                    foreach (Extension extension in addConfig.AllRoles)
                    {
                        outConfig.AllRoles.Add(new Extension(extension.Id));
                    }
                }

                if (addConfig.NamedRoles != null)
                {
                    foreach (RoleExtensions roleExtensions in addConfig.NamedRoles)
                    {
                        outConfig.NamedRoles.Add(new RoleExtensions
                        {
                            RoleName = roleExtensions.RoleName,
                            Extensions = new ExtensionList(roleExtensions.Extensions.Select(e => new Extension(e.Id)))
                        });
                    }
                }
            }
            return outConfig;
        }

        static internal bool InstallExtension(PSExtensionConfiguration psConfig, IServiceManagement channel, string subscriptionId, string serviceName, out ExtensionConfiguration outConfig)
        {
            outConfig = NewExtensionConfig();
            bool installed = false;
            string[] roleNames = psConfig.NamedRoles;
            if (psConfig.AllRoles || roleNames == null || !roleNames.Any())
            {
                string defaultExtensionId = "";
                string thumbprint = "";
                string thumbprintAlgorithm = "";
                bool foundThumbprint = false;
                for (int i = 0; i < ExtensionIdLiveCycleCount && !foundThumbprint; i++)
                {
                    string otherExtensionId = string.Format(ExtensionIdTemplate, "Default", psConfig.Type, i);
                    HostedServiceExtension extension = GetExtension(channel, subscriptionId, serviceName, otherExtensionId);
                    if (extension != null)
                    {
                        defaultExtensionId = otherExtensionId;
                        thumbprint = extension.Thumbprint;
                        thumbprintAlgorithm = extension.ThumbprintAlgorithm;
                        foundThumbprint = true;
                    }
                }

                HostedServiceExtensionInput hostedSvcExtInput1 = new HostedServiceExtensionInput
                {
                    Id = defaultExtensionId,
                    Thumbprint = thumbprint,
                    ThumbprintAlgorithm = thumbprintAlgorithm,
                    ProviderNameSpace = psConfig.ProviderNameSpace,
                    Type = psConfig.Type,
                    PublicConfiguration = psConfig.PublicConfiguration,
                    PrivateConfiguration = psConfig.PrivateConfiguration
                };
                AddHostedServiceExtension(channel, subscriptionId, serviceName, hostedSvcExtInput1);

                outConfig = AddDefaultExtension(outConfig, defaultExtensionId);

                installed = true;
            }
            else
            {
                foreach (string roleName in roleNames)
                {
                    string roleExtensionId = "";
                    string thumbprint2 = "";
                    string thumbprintAlgorithm2 = "";
                    bool foundThumbprint2 = false;
                    for (int i = 0; i < ExtensionIdLiveCycleCount && !foundThumbprint2; i++)
                    {
                        string otherExtensionId = string.Format(ExtensionIdTemplate, roleName, psConfig.Type, i);
                        HostedServiceExtension extension = GetExtension(channel, subscriptionId, serviceName, otherExtensionId);
                        if (extension != null)
                        {
                            roleExtensionId = otherExtensionId;
                            thumbprint2 = extension.Thumbprint;
                            thumbprintAlgorithm2 = extension.ThumbprintAlgorithm;
                            foundThumbprint2 = true;
                        }
                    }

                    HostedServiceExtensionInput hostedSvcExtInput2 = new HostedServiceExtensionInput
                    {
                        Id = roleExtensionId,
                        Thumbprint = thumbprint2,
                        ThumbprintAlgorithm = thumbprintAlgorithm2,
                        ProviderNameSpace = psConfig.ProviderNameSpace,
                        Type = psConfig.Type,
                        PublicConfiguration = psConfig.PublicConfiguration,
                        PrivateConfiguration = psConfig.PrivateConfiguration
                    };
                    AddHostedServiceExtension(channel, subscriptionId, serviceName, hostedSvcExtInput2);

                    outConfig = AddExtension(outConfig, roleName, roleExtensionId);
                }
                installed = true;
            }

            return installed;
        }

        static internal void DeleteHostedServieExtension(IServiceManagement channel, string subscriptionId, string serviceName, string extensionId)
        {
            channel.DeleteHostedServiceExtension(subscriptionId, serviceName, extensionId);
        }

        static internal void AddHostedServiceExtension(IServiceManagement channel, string subscriptionId, string serviceName, HostedServiceExtensionInput hostedSvcExtInput)
        {
            channel.AddHostedServiceExtension(subscriptionId, serviceName, hostedSvcExtInput);
        }

        static internal void AddHostedServiceExtension(IServiceManagement channel,
                                                       string subscriptionId,
                                                       string serviceName,
                                                       string extensionId,
                                                       string thumbprint,
                                                       string thumbprintAlgorithm,
                                                       string extensionNameSpace,
                                                       string extensionType,
                                                       string publicConfig,
                                                       string privateConfig)
        {
            HostedServiceExtensionInput hostedSvcExtInput = new HostedServiceExtensionInput
            {
                Id = extensionId,
                Thumbprint = thumbprint,
                ThumbprintAlgorithm = thumbprintAlgorithm,
                ProviderNameSpace = extensionNameSpace,
                Type = extensionType,
                PublicConfiguration = publicConfig,
                PrivateConfiguration = privateConfig
            };
            AddHostedServiceExtension(channel, subscriptionId, serviceName, hostedSvcExtInput);
        }
    }
}
