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

namespace Microsoft.WindowsAzure.Management.CloudService.Cmdlet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Security;
    using System.Security.Permissions;
    using AzureTools;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Model;
    using ServiceConfigurationSchema;
    using ServiceDefinitionSchema;
    using Utilities;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;

    /// <summary>
    /// Adds dedicated caching node worker role.
    /// </summary>
    [Cmdlet(VerbsCommon.Add, "AzureCacheWorkerRole")]
    public class AddAzureCacheWorkerRoleCommand : CmdletBase
    {
        [Parameter(Position = 0, HelpMessage = "Role name")]
        [Alias("n")]
        public string Name { get; set; }

        [Parameter(Position = 1, HelpMessage = "Instances count")]
        [Alias("i")]
        public int Instances { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();
            AddAzureCacheWorkerRoleProcess(Name, Instances, base.GetServiceRootPath());
        }

        private AzureService CachingConfigurationFactoryMethod(string rootPath, RoleInfo cacheWorkerRole, string sdkVersion)
        {
            switch (sdkVersion)
            {
                case SDKVersion.Version180:
                    return Version180Configuration(rootPath, cacheWorkerRole);

                default:
                    throw new Exception(string.Format(Resources.AzureSdkVersionNotSupported,
                        Resources.MinSupportAzureSdkVersion, Resources.MaxSupportAzureSdkVersion));
            }
        }

        /// <summary>
        /// Process for creating caching worker role.
        /// </summary>
        /// <param name="workerRoleName">The cache worker role name</param>
        /// <param name="instances">The instance count</param>
        /// <param name="rootPath">The service root path</param>
        /// <returns>The added cache worker role</returns>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public WorkerRole AddAzureCacheWorkerRoleProcess(string workerRoleName, int instances, string rootPath)
        {
            // Create cache worker role.
            AzureService azureService = new AzureService(rootPath, null);
            RoleInfo nodeWorkerRole = azureService.AddWorkerRole(Path.Combine(Resources.GeneralScaffolding, RoleType.WorkerRole.ToString()), workerRoleName, instances);
            azureService = CachingConfigurationFactoryMethod(rootPath, nodeWorkerRole, new AzureTool().AzureSdkVersion);
            azureService.Components.Save(azureService.Paths);
            WorkerRole cacheWorkerRole = azureService.Components.GetWorkerRole(nodeWorkerRole.Name);

            // Write output
            SafeWriteOutputPSObject(
                cacheWorkerRole.GetType().FullName,
                Parameters.CacheWorkerRoleName, nodeWorkerRole.Name,
                Parameters.Instances, nodeWorkerRole.InstanceCount
                );

            return azureService.Components.GetWorkerRole(workerRoleName);
        }

        /// <summary>
        /// Configure the worker role for caching by:
        /// * Add caching module to the role imports.
        /// * Enable caching Diagnostic store.
        /// * Remove input endpoints.
        /// * Add caching configuration settings.
        /// </summary>
        /// <param name="rootPath"></param>
        /// <param name="nodeWorkerRole"></param>
        /// <returns></returns>
        private AzureService Version180Configuration(string rootPath, RoleInfo nodeWorkerRole)
        {
            // Fetch cache role information from service definition and service configuration files.
            AzureService azureService = new AzureService(rootPath, null);
            WorkerRole cacheWorkerRole = azureService.Components.GetWorkerRole(nodeWorkerRole.Name);
            RoleSettings cacheRoleSettings = azureService.Components.GetCloudConfigRole(nodeWorkerRole.Name);

            // Add caching module to the role imports
            cacheWorkerRole.Imports = General.ExtendArray<Import>(cacheWorkerRole.Imports, new Import { moduleName = Resources.CachingModuleName });

            // Enable caching Diagnostic store.
            LocalStore diagnosticStore = new LocalStore { name = Resources.CacheDiagnosticStoreName, cleanOnRoleRecycle = false };
            cacheWorkerRole.LocalResources = General.InitializeIfNull<LocalResources>(cacheWorkerRole.LocalResources);
            cacheWorkerRole.LocalResources.LocalStorage = General.ExtendArray<LocalStore>(cacheWorkerRole.LocalResources.LocalStorage, diagnosticStore);

            // Remove input endpoints.
            cacheWorkerRole.Endpoints.InputEndpoint = null;

            // Add caching configuration settings
            List<ConfigConfigurationSetting> cachingConfigSettings = new List<ConfigConfigurationSetting>();
            cachingConfigSettings.Add(new ConfigConfigurationSetting { name = Resources.NamedCacheSettingName, value = Resources.NamedCacheSettingValue });
            cachingConfigSettings.Add(new ConfigConfigurationSetting { name = Resources.DiagnosticLevelName, value = Resources.DiagnosticLevelValue});
            cachingConfigSettings.Add(new ConfigConfigurationSetting { name = Resources.CachingCacheSizePercentageSettingName, value = string.Empty });
            cachingConfigSettings.Add(new ConfigConfigurationSetting { name = Resources.CachingConfigStoreConnectionStringSettingName, value = string.Empty });
            cacheRoleSettings.ConfigurationSettings = General.ExtendArray<ConfigConfigurationSetting>(cacheRoleSettings.ConfigurationSettings, cachingConfigSettings);
            return azureService;
        }
    }
}
