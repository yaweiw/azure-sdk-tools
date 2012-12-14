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
        public AddAzureCacheWorkerRoleCommand()
        {
            SkipChannelInit = true;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();
            AddAzureCacheWorkerRoleProcess(base.Name, base.Instances, base.GetServiceRootPath());
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
            RoleInfo nodeWorkerRole;

            // Create cache worker role.
            new AddAzureNodeWorkerRoleCommand().AddAzureNodeWorkerRoleProcess(workerRoleName, instances, rootPath, out nodeWorkerRole);
            AzureService azureService = CachingConfigurationFactoryMethod(rootPath, nodeWorkerRole, new AzureTool().AzureSdkVersion);
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
