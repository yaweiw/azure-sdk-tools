// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.CloudService.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography.Pkcs;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Text;
    using AzureTools;
    using Microsoft.WindowsAzure.Management.CloudService.Node.Cmdlet;
    using Model;
    using ServiceConfigurationSchema;
    using ServiceDefinitionSchema;
    using Utilities;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;

    /// <summary>
    /// Adds dedicated caching node worker role.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureCacheWorkerRole")]
    public class AddAzureCacheWorkerRoleCommand : AddRole
    {
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                string result = AddAzureCacheWorkerRoleProcess(base.Name, base.Instances, base.GetServiceRootPath());
                SafeWriteObject(result);
            }
            catch (Exception ex)
            {
                SafeWriteError(ex);
            }
        }

        /// <summary>
        /// Creates a dedicated caching node worker role by:
        /// * Create node worker role.
        /// * Add caching module to the role imports
        /// * Enable caching on the role by adding LocalResources with LocalStorage element in the role definition.
        /// * Add caching configuration settings
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public string AddAzureCacheWorkerRoleProcess(string workerRoleName, int instances, string rootPath)
        {
            RoleInfo nodeWorkerRole;

            // Create node worker role.
            string message = new AddAzureNodeWorkerRoleCommand().AddAzureNodeWorkerRoleProcess(workerRoleName, instances, rootPath, out nodeWorkerRole);

            // Fetch cache role information from service definition and service configuration files.
            AzureService azureService = new AzureService(rootPath, null);
            WorkerRole cacheWorkerRole = azureService.Components.GetWorkerRole(nodeWorkerRole.Name);
            RoleSettings cacheRoleSettings = azureService.Components.GetRole(nodeWorkerRole.Name);

            // Add caching module to the role imports
            cacheWorkerRole.Imports = General.ExtendArray<Import>(cacheWorkerRole.Imports, new Import { moduleName = Resources.CachingModuleName });

            // Enable role caching
            LocalStore localStore = new LocalStore
            {
                name = Resources.CachingFileStoreName,
                sizeInMB = int.Parse(Resources.DefaultRoleCachingInMB),
                cleanOnRoleRecycle = false
            };
            cacheWorkerRole.LocalResources = General.InitializeIfNull<LocalResources>(cacheWorkerRole.LocalResources);
            cacheWorkerRole.LocalResources.LocalStorage = General.ExtendArray<LocalStore>(cacheWorkerRole.LocalResources.LocalStorage, localStore);

            // Add caching configuration settings
            List<ConfigConfigurationSetting> cachingConfigSettings = new List<ConfigConfigurationSetting>();
            cachingConfigSettings.Add(new ConfigConfigurationSetting { name = Resources.CachingNamedCacheSettingName, value = string.Empty});
            cachingConfigSettings.Add(new ConfigConfigurationSetting { name = Resources.CachingLogLevelSettingName, value = string.Empty });
            cachingConfigSettings.Add(new ConfigConfigurationSetting { name = Resources.CachingCacheSizePercentageSettingName, value = string.Empty });
            cachingConfigSettings.Add(new ConfigConfigurationSetting { name = Resources.CachingConfigStoreConnectionStringSettingName, value = string.Empty });
            cacheRoleSettings.ConfigurationSettings = General.ExtendArray<ConfigConfigurationSetting>(cacheRoleSettings.ConfigurationSettings, cachingConfigSettings);

            // Save changes
            azureService.Components.OverrideWorkerRole(cacheWorkerRole);
            azureService.Components.OverrideRole(cacheRoleSettings);
            azureService.Components.Save(azureService.Paths);

            return message;
        }
    }
}
