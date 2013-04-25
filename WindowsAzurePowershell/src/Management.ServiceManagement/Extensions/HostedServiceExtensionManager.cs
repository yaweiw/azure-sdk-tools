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
    using WindowsAzure.ServiceManagement;

    public class HostedServiceExtensionManager
    {
        private const int ExtensionIdLiveCycleCount = 2;
        private const string ExtensionIdTemplate = "{0}-{1}-Ext-{2}";

        public IServiceManagement Channel
        {
            get;
            private set;
        }

        public string SubscriptionId
        {
            get;
            private set;
        }

        public string ServiceName
        {
            get;
            private set;
        }

        public HostedServiceExtensionManager(IServiceManagement channel, string subscriptionId, string serviceName)
        {
            if (channel == null)
            {
                throw new ArgumentNullException("Channel cannot be null.");
            }
            Channel = channel;

            if (string.IsNullOrWhiteSpace(subscriptionId))
            {
                throw new ArgumentNullException("Subscription Id cannot be empty or null");
            }
            SubscriptionId = subscriptionId;

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentNullException("Service name cannot be empty or null");
            }
            ServiceName = serviceName;
        }

        public HostedServiceExtension GetExtension(string extensionId)
        {
            return Channel.ListHostedServiceExtensions(SubscriptionId, ServiceName).Find(e => e.Id == extensionId);
        }

        public bool ExistDefaultExtension(ExtensionConfiguration inConfig, string extensionNameSpace, string extensionType)
        {
            ExtensionConfiguration extConfig = NewExtensionConfig(inConfig);
            foreach (Extension ext in extConfig.AllRoles)
            {
                HostedServiceExtension extension = GetExtension(ext.Id);
                if (extension != null && extension.Type == extensionType)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ExistExtension(ExtensionConfiguration inConfig, string[] roles, string extensionNameSpace, string extensionType)
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
                            HostedServiceExtension extension = GetExtension(ext.Id);
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
                return ExistDefaultExtension(inConfig, extensionNameSpace, extensionType);
            }
            return false;
        }

        public void DeleteHostedServieExtension(string extensionId)
        {
            Channel.DeleteHostedServiceExtension(SubscriptionId, ServiceName, extensionId);
        }

        public void AddHostedServiceExtension(HostedServiceExtensionInput hostedSvcExtInput)
        {
            Channel.AddHostedServiceExtension(SubscriptionId, ServiceName, hostedSvcExtInput);
        }

        public bool InstallExtension(PSExtensionConfiguration psConfig, out ExtensionConfiguration outConfig)
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
                    HostedServiceExtension extension = GetExtension(otherExtensionId);
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
                AddHostedServiceExtension(hostedSvcExtInput1);

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
                        HostedServiceExtension extension = GetExtension(otherExtensionId);
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
                    AddHostedServiceExtension(hostedSvcExtInput2);

                    outConfig = AddExtension(outConfig, roleName, roleExtensionId);
                }
                installed = true;
            }

            return installed;
        }

        public ExtensionConfiguration RemoveDefaultExtension(ExtensionConfiguration inConfig, string extensionNameSpace, string extensionType)
        {
            ExtensionConfiguration extConfig = NewExtensionConfig(inConfig);
            foreach (Extension ext in extConfig.AllRoles)
            {
                HostedServiceExtension extension = GetExtension(ext.Id);
                if (extension != null && extension.Type == extensionType)
                {
                    extConfig = RemoveDefaultExtension(extConfig, extension.Id);
                }
            }
            return extConfig;
        }

        public ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig, string roleName, string extensionNameSpace, string extensionType)
        {
            return RemoveExtension(inConfig, new string[1] { roleName }, extensionNameSpace, extensionType);
        }

        public ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig, string[] roles, string extensionNameSpace, string extensionType)
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
                            HostedServiceExtension inExtension = GetExtension(inExt.Id);
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
                return RemoveDefaultExtension(inConfig, extensionNameSpace, extensionType);
            }
            return extConfig;
        }

        public ExtensionConfiguration RemoveDefaultExtension(ExtensionConfiguration inConfig, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            outConfig.AllRoles.RemoveAll(ext => ext.Id == extensionId);
            return outConfig;
        }

        public ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig, string roleName, string extensionId)
        {
            return RemoveExtension(inConfig, new string[1] { roleName }, extensionId);
        }

        public ExtensionConfiguration RemoveExtension(ExtensionConfiguration inConfig, string[] roleNames, string extensionId)
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

        public ExtensionConfiguration NewExtensionConfig()
        {
            return new ExtensionConfiguration
            {
                AllRoles = new AllRoles(),
                NamedRoles = new NamedRoles()
            };
        }

        public ExtensionConfiguration NewExtensionConfig(ExtensionConfiguration inConfig)
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

        public ExtensionConfiguration NewExtensionConfig(Deployment deployment)
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

        public bool ExistDefaultExtension(ExtensionConfiguration inConfig, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            return outConfig.AllRoles.Any(ext => ext.Id == extensionId);
        }

        public bool ExistExtension(ExtensionConfiguration inConfig, string roleName, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            RoleExtensions roleExtensions = outConfig.NamedRoles.Find(r => r.RoleName == roleName);
            return roleExtensions != null ? roleExtensions.Extensions.Any(ext => ext.Id == extensionId) : false;
        }

        public ExtensionConfiguration AddDefaultExtension(ExtensionConfiguration inConfig, string extensionId)
        {
            ExtensionConfiguration outConfig = NewExtensionConfig(inConfig);
            outConfig.AllRoles.Add(new Extension(extensionId));
            return outConfig;
        }

        public ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, string roleName, string extensionId)
        {
            return AddExtension(inConfig, new string[1] { roleName }, extensionId);
        }

        public ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, string[] roleNames, string extensionId)
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

        public ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, PSExtensionConfiguration psConfig, string extensionId)
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

        public ExtensionConfiguration AddExtension(ExtensionConfiguration inConfig, ExtensionConfiguration addConfig)
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
    }
}
