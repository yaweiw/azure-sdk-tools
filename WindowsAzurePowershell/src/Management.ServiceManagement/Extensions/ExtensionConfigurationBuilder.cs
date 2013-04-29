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

    public class ExtensionConfigurationBuilder
    {
        private ExtensionManager ExtensionManager;
        private HashSet<string> AllRoles;
        private Dictionary<string, HashSet<string>> NamedRoles;

        public ExtensionConfigurationBuilder(ExtensionManager extensionManager)
        {
            if (extensionManager == null)
            {
                throw new ArgumentNullException("extensionManager");
            }
            ExtensionManager = extensionManager;
            AllRoles = new HashSet<string>();
            NamedRoles = new Dictionary<string, HashSet<string>>();
        }

        public bool ExistDefault(string nameSpace, string type)
        {
            return AllRoles.Any(id =>
            {
                HostedServiceExtension e = ExtensionManager.GetExtension(id);
                return e != null && e.ProviderNameSpace == nameSpace && e.Type == type;
            });
        }

        public bool ExistAny(string nameSpace, string type)
        {
            return ExistDefault(nameSpace, type)
                || NamedRoles.Any(r => Exist(new string[] { r.Key }, nameSpace, type));
        }

        public bool Exist(string[] roles, string nameSpace, string type)
        {
            if (roles != null && roles.Any())
            {
                return (from r in NamedRoles
                        where roles.Contains(r.Key)
                        from id in r.Value
                        select ExtensionManager.GetExtension(id)).Any(e => e != null && e.ProviderNameSpace == nameSpace && e.Type == type);
            }
            else
            {
                return ExistDefault(nameSpace, type);
            }
        }

        public bool ExistDefault(string extensionId)
        {
            return AllRoles.Any(id => id == extensionId);
        }

        public bool ExistAny(string extensionId)
        {
            return AllRoles.Any(id => id == extensionId)
                || NamedRoles.Any(r => Exist(r.Key, extensionId));
        }

        public bool Exist(string roleName, string extensionId)
        {
            return string.IsNullOrWhiteSpace(roleName) ? ExistDefault(extensionId)
                                                       : NamedRoles.Any(r => r.Key == roleName
                                                                          && r.Value.Any(id => id == extensionId));
        }

        public bool Exist(ExtensionRole role, string extensionId)
        {
            return role.RoleType == ExtensionRoleType.AllRoles ? ExistDefault(extensionId)
                                                               : Exist(role.RoleName, extensionId);
        }

        public ExtensionConfigurationBuilder RemoveDefault(string extensionId)
        {
            AllRoles.Remove(extensionId);
            return this;
        }

        public ExtensionConfigurationBuilder Remove(string roleName, string extensionId)
        {
            return Remove(new string[] { roleName }, extensionId);
        }

        public ExtensionConfigurationBuilder Remove(string[] roleNames, string extensionId)
        {
            if (roleNames != null && roleNames.Any())
            {
                foreach (var r in roleNames.Intersect(NamedRoles.Keys))
                {
                    NamedRoles[r].Remove(extensionId);
                }
                return this;
            }
            else
            {
                return RemoveDefault(extensionId);
            }
        }

        public ExtensionConfigurationBuilder RemoveDefault(string nameSpace, string type)
        {
            AllRoles.RemoveWhere(e => ExistDefault(nameSpace, type));
            return this;
        }

        public ExtensionConfigurationBuilder RemoveAny(string nameSpace, string type)
        {
            RemoveDefault(nameSpace, type);
            foreach (var r in NamedRoles)
            {
                r.Value.RemoveWhere(id => Exist(r.Key, id));
            }
            return this;
        }

        public ExtensionConfigurationBuilder Remove(string roleName, string nameSpace, string type)
        {
            return Remove(new string[] { roleName }, nameSpace, type);
        }

        public ExtensionConfigurationBuilder Remove(string[] roles, string nameSpace, string type)
        {
            if (roles != null && roles.Any())
            {
                foreach (var r in roles.Intersect(NamedRoles.Keys))
                {
                    NamedRoles[r].RemoveWhere(id =>
                    {
                        var e = ExtensionManager.GetExtension(id);
                        return e != null && e.ProviderNameSpace == nameSpace && e.Type == type;
                    });
                }
                return this;
            }
            else
            {
                return RemoveDefault(nameSpace, type);
            }
        }

        public ExtensionConfigurationBuilder AddDefault(string extensionId)
        {
            if (!ExistDefault(extensionId))
            {
                AllRoles.Add(extensionId);
            }
            return this;
        }

        public ExtensionConfigurationBuilder Add(string roleName, string extensionId)
        {
            return Add(new string[] { roleName }, extensionId);
        }

        public ExtensionConfigurationBuilder Add(string[] roleNames, string extensionId)
        {
            if (roleNames != null && roleNames.Any())
            {
                foreach (var r in roleNames)
                {
                    if (NamedRoles.ContainsKey(r))
                    {
                        NamedRoles[r].Add(extensionId);
                    }
                    else
                    {
                        NamedRoles.Add(r, new HashSet<string>(new string[] { extensionId }));
                    }
                }
                return this;
            }
            else
            {
                return AddDefault(extensionId);
            }
        }

        public ExtensionConfigurationBuilder Add(ExtensionRole role, string extensionId)
        {
            if (role != null)
            {
                if (role.RoleType == ExtensionRoleType.NamedRoles)
                {
                    Add(role.RoleName, extensionId);
                }
                else
                {
                    AddDefault(extensionId);
                }
            }
            return this;
        }

        public ExtensionConfigurationBuilder Add(ExtensionConfigurationContext context, string extensionId)
        {
            if (context != null && context.Roles != null)
            {
                context.Roles.ForEach(r => Add(r, extensionId));
            }
            return this;
        }

        public ExtensionConfigurationBuilder Add(ExtensionConfiguration config)
        {
            if (config != null)
            {
                if (config.AllRoles != null)
                {
                    config.AllRoles.ForEach(e => AddDefault(e.Id));
                }

                if (config.NamedRoles != null)
                {
                    foreach (var r in config.NamedRoles)
                    {
                        r.Extensions.ForEach(e =>
                        {
                            if (NamedRoles.ContainsKey(r.RoleName))
                            {
                                NamedRoles[r.RoleName].Add(e.Id);
                            }
                            else
                            {
                                NamedRoles.Add(r.RoleName, new HashSet<string>(new string[] { e.Id }));
                            }
                        });
                    }
                }
            }
            return this;
        }

        public ExtensionConfiguration ToConfiguration()
        {
            ExtensionConfiguration config = new ExtensionConfiguration
            {
                AllRoles = new AllRoles(),
                NamedRoles = new NamedRoles()
            };
            config.AllRoles.AddRange(from id in AllRoles select new Extension(id));
            config.NamedRoles.AddRange(from r in NamedRoles
                                       where r.Value.Any()
                                       select new RoleExtensions
                                       {
                                           RoleName = r.Key,
                                           Extensions = new ExtensionList(from id in r.Value select new Extension(id))
                                       });
            return config;
        }
    }
}
