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
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml.Linq;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.CloudService.AzureTools;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.Scaffolding;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Model;
    using ServiceDefinitionSchema;
    using Utilities;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;
    using DefConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema.ConfigurationSetting;

    /// <summary>
    /// Enables memcache for specific role.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Enable, "AzureMemcacheRole")]
    public class EnableAzureMemcacheRoleCommand : CmdletBase
    {
        /// <summary>
        /// The role name to edit.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("rn")]
        [ValidateNotNullOrEmpty]
        public string RoleName { get; set; }

        /// <summary>
        /// The dedicated caching worker role name.
        /// </summary>
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("cn")]
        [ValidateNotNullOrEmpty]
        public string CacheWorkerRoleName { get; set; }

        [Parameter(Position = 2, Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();
            EnableAzureMemcacheRoleProcess(this.RoleName, this.CacheWorkerRoleName, base.GetServiceRootPath());
        }

        /// <summary>
        /// Process for enabling memcache for web roles.
        /// </summary>
        /// <param name="roleName">The web role name</param>
        /// <param name="cacheWorkerRoleName">The cache worker role name</param>
        /// <param name="rootPath">The root path of the services</param>
        /// <returns>The resulted message</returns>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public WebRole EnableAzureMemcacheRoleProcess(string roleName, string cacheWorkerRoleName, string rootPath)
        {
            AzureService azureService = new AzureService(rootPath, null);

            // Verify cache worker role exists
            if (!azureService.Components.RoleExists(cacheWorkerRoleName))
            {
                throw new Exception(string.Format(Resources.RoleNotFoundMessage, cacheWorkerRoleName));
            }

            WorkerRole cacheWorkerRole = azureService.Components.GetWorkerRole(cacheWorkerRoleName);

            // Verify that the cache worker role has proper caching configuration.
            if (!IsCacheWorkerRole(cacheWorkerRole))
            {
                throw new Exception(string.Format(Resources.NotCacheWorkerRole, cacheWorkerRoleName));
            }

            // Verify role to enable cache on exists
            if (!azureService.Components.RoleExists(roleName))
            {
                throw new Exception(string.Format(Resources.RoleNotFoundMessage, roleName));
            }

            // Verify that caching is not enabled for the role
            if (IsCacheEnabled(azureService.Components.GetRoleStartup(roleName)))
            {
                throw new Exception(string.Format(Resources.CacheAlreadyEnabledMessage, roleName));
            }

            // All validations passed, enable caching.
            string message = string.Empty;
            EnableMemcache(roleName, cacheWorkerRoleName, ref message, ref azureService);

            WriteVerbose(message);

            if (PassThru)
            {
                SafeWriteOutputPSObject(typeof(RoleSettings).FullName, Parameters.RoleName, roleName);
            }

            return azureService.Components.GetWebRole(roleName);
        }

        /// <summary>
        /// Main entry for enabling memcache.
        /// </summary>
        /// <param name="roleName">The web role name</param>
        /// <param name="cacheWorkerRoleName">The cache worker role name</param>
        /// <param name="rootPath">The service root path</param>
        /// <param name="message">The resulted message</param>
        /// <param name="azureService">The azure service instance</param>
        /// <param name="webRole">The web role to enable caching one</param>
        private void EnableMemcache(string roleName, string cacheWorkerRoleName, ref string message, ref AzureService azureService)
        {
            string currentVersion = new AzureTool().AzureSdkVersion;

            // Add MemcacheShim runtime installation.
            azureService.AddRoleRuntime(azureService.Paths, roleName, Resources.CacheRuntimeValue, currentVersion);

            // Fetch web role information.
            Startup startup = azureService.Components.GetRoleStartup(roleName);

            // Assert that cache runtime is added to the runtime startup.
            Debug.Assert(Array.Exists<Variable>(CloudRuntime.GetRuntimeStartupTask(startup).Environment,
                v => v.name.Equals(Resources.RuntimeTypeKey) && v.value.Contains(Resources.CacheRuntimeValue)));

            if (azureService.Components.IsWebRole(roleName))
            {
                WebRole webRole = azureService.Components.GetWebRole(roleName);
                webRole.LocalResources = General.InitializeIfNull<LocalResources>(webRole.LocalResources);
                DefConfigurationSetting[] configurationSettings = webRole.ConfigurationSettings;

                CachingConfigurationFactoryMethod(
                        azureService,
                        roleName,
                        true,
                        cacheWorkerRoleName,
                        webRole.Startup,
                        webRole.Endpoints,
                        webRole.LocalResources,
                        ref configurationSettings,
                        currentVersion);
                webRole.ConfigurationSettings = configurationSettings;
            }
            else
            {
                WorkerRole workerRole = azureService.Components.GetWorkerRole(roleName);
                workerRole.LocalResources = General.InitializeIfNull<LocalResources>(workerRole.LocalResources);
                DefConfigurationSetting[] configurationSettings = workerRole.ConfigurationSettings;

                CachingConfigurationFactoryMethod(
                        azureService,
                        roleName,
                        false,
                        cacheWorkerRoleName,
                        workerRole.Startup,
                        workerRole.Endpoints,
                        workerRole.LocalResources,
                        ref configurationSettings,
                        currentVersion);
                workerRole.ConfigurationSettings = configurationSettings;
            }

            // Save changes
            azureService.Components.Save(azureService.Paths);

            message = string.Format(Resources.EnableMemcacheMessage, roleName, cacheWorkerRoleName, Resources.MemcacheEndpointPort);
        }

        /// <summary>
        /// Factory method to apply memcache required configuration based on the installed SDK version.
        /// </summary>
        /// <param name="azureService">The azure service instance</param>
        /// <param name="webRole">The web role to enable caching on</param>
        /// <param name="isWebRole">Flag indicating if the provided role is web or not</param>
        /// <param name="cacheWorkerRole">The memcache worker role name</param>
        /// <param name="startup">The role startup</param>
        /// <param name="endpoints">The role endpoints</param>
        /// <param name="localResources">The role local resources</param>
        /// <param name="configurationSettings">The role configuration settings</param>
        /// <param name="sdkVersion">The current SDK version</param>
        private void CachingConfigurationFactoryMethod(
            AzureService azureService,
            string roleName,
            bool isWebRole,
            string cacheWorkerRole,
            Startup startup,
            Endpoints endpoints,
            LocalResources localResources,
            ref DefConfigurationSetting[] configurationSettings,
            string sdkVersion)
        {
            switch (sdkVersion)
            {
                case SDKVersion.Version180:
                    Version180Configuration(
                        azureService,
                        roleName,
                        isWebRole,
                        cacheWorkerRole,
                        startup,
                        endpoints,
                        localResources,
                        ref configurationSettings);
                    break;

                default:
                    throw new Exception(string.Format(Resources.AzureSdkVersionNotSupported, 
                        Resources.MinSupportAzureSdkVersion, Resources.MaxSupportAzureSdkVersion));
            }
        }

        /// <summary>
        /// Applies required configuration for enabling cache in SDK 1.8.0 version by:
        /// * Add MemcacheShim runtime installation.
        /// * Add startup task to install memcache shim on the client side.
        /// * Add default memcache internal endpoint.
        /// * Add cache diagnostic to local resources.
        /// * Add ClientDiagnosticLevel setting to service configuration.
        /// * Adjust web.config to enable auto discovery for the caching role.
        /// </summary>
        /// <param name="azureService">The azure service instance</param>
        /// <param name="webRole">The web role to enable caching on</param>
        /// <param name="isWebRole">Flag indicating if the provided role is web or not</param>
        /// <param name="cacheWorkerRole">The memcache worker role name</param>
        /// <param name="startup">The role startup</param>
        /// <param name="endpoints">The role endpoints</param>
        /// <param name="localResources">The role local resources</param>
        /// <param name="configurationSettings">The role configuration settings</param>
        private void Version180Configuration(
            AzureService azureService,
            string roleName,
            bool isWebRole,
            string cacheWorkerRole,
            Startup startup,
            Endpoints endpoints,
            LocalResources localResources,
            ref DefConfigurationSetting[] configurationSettings)
        {

            if (isWebRole)
            {
                // Generate cache scaffolding for web role
                azureService.GenerateScaffolding(Path.Combine(Resources.CacheScaffolding, RoleType.WebRole.ToString()),
                    roleName, new Dictionary<string, object>());

                // Adjust web.config to enable auto discovery for the caching role.
                UpdateWebCloudConfig(roleName, cacheWorkerRole, azureService);
            }
            else
            {
                // Generate cache scaffolding for worker role
                Dictionary<string, object> parameters = new Dictionary<string, object>();
                parameters[ScaffoldParams.RoleName] = cacheWorkerRole;

                azureService.GenerateScaffolding(Path.Combine(Resources.CacheScaffolding, RoleType.WorkerRole.ToString()),
                    roleName, parameters);
            }

            // Add startup task to install memcache shim on the client side.
            Task shimStartupTask = new Task { commandLine = Resources.CacheStartupCommand, executionContext = ExecutionContext.elevated };
            startup.Task = General.ExtendArray<Task>(startup.Task, shimStartupTask);
                
            // Add default memcache internal endpoint.
            InternalEndpoint memcacheEndpoint = new InternalEndpoint
            {
                name = Resources.MemcacheEndpointName,
                protocol = InternalProtocol.tcp,
                port = Resources.MemcacheEndpointPort
            };
            endpoints.InternalEndpoint = General.ExtendArray<InternalEndpoint>(endpoints.InternalEndpoint, memcacheEndpoint);

            // Enable cache diagnostic
            LocalStore localStore = new LocalStore
            {
                name = Resources.CacheDiagnosticStoreName,
                cleanOnRoleRecycle = false
            };
            localResources.LocalStorage = General.ExtendArray<LocalStore>(localResources.LocalStorage, localStore);

            DefConfigurationSetting diagnosticLevel = new DefConfigurationSetting { name = Resources.CacheClientDiagnosticLevelAssemblyName };
            configurationSettings = General.ExtendArray<DefConfigurationSetting>(configurationSettings, diagnosticLevel);

            // Add ClientDiagnosticLevel setting to service configuration.
            RoleSettings roleSettings = azureService.Components.GetCloudConfigRole(roleName);
            ConfigConfigurationSetting clientDiagnosticLevel = new ConfigConfigurationSetting { name = Resources.ClientDiagnosticLevelName, value = Resources.ClientDiagnosticLevelValue };
            roleSettings.ConfigurationSettings = General.ExtendArray<ConfigConfigurationSetting>(roleSettings.ConfigurationSettings, clientDiagnosticLevel);
        }

        /// <summary>
        /// Updates the web.cloud.config with to auto-discover the cache role.
        /// </summary>
        /// <param name="roleName">The role name</param>
        /// <param name="cacheWorkerRoleName">The cache worker role name</param>
        /// <param name="azureService">The azure service instance for the role</param>
        private void UpdateWebCloudConfig(string roleName, string cacheWorkerRoleName, AzureService azureService)
        {
            string webConfigPath = string.Format(@"{0}\{1}\{2}", azureService.Paths.RootPath, roleName, Resources.WebCloudConfig);
            XDocument webConfig = XDocument.Load(webConfigPath);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters[ScaffoldParams.RoleName] = cacheWorkerRoleName;
            string autoDiscoveryConfig = Scaffold.ReplaceParameter(Resources.CacheAutoDiscoveryConfig, parameters);

            // Adding the auto-discovery is sensetive to the placement of the nodes. The first node which is <configSections>
            // must be added at the first and the last node which is dataCacheClients must be added as last element.
            XElement autoDiscoverXElement = XElement.Parse(autoDiscoveryConfig);
            webConfig.Element("configuration").AddFirst(autoDiscoverXElement.FirstNode);
            webConfig.Element("configuration").Add(autoDiscoverXElement.LastNode);
            Debug.Assert(webConfig.Element("configuration").FirstNode.Ancestors("section").Attributes("name") != null);
            Debug.Assert(webConfig.Element("configuration").LastNode.Ancestors("tracing").Attributes("sinkType") != null);
            webConfig.Save(webConfigPath);
        }

        /// <summary>
        /// Checks if memcache is already enabled or not for the given role startup.
        /// It does this by checking the role startup task.
        /// </summary>
        /// <param name="startup">The role startup</param>
        /// <returns>Either enabled or not</returns>
        private bool IsCacheEnabled(Startup startup)
        {
            if (startup.Task != null)
            {
                return Array.Exists<Variable>(CloudRuntime.GetRuntimeStartupTask(startup).Environment,
                v => v.name.Equals(Resources.RuntimeTypeKey) && v.value.Contains(Resources.CacheRuntimeValue));
            }

            return false;
        }

        /// <summary>
        /// Checks if the worker role is configured as caching worker role.
        /// </summary>
        /// <param name="workerRole">The worker role object</param>
        /// <returns>True if its caching worker role, false if not</returns>
        private bool IsCacheWorkerRole(WorkerRole workerRole)
        {
            if (workerRole.Imports != null)
            {
                return Array.Exists<Import>(workerRole.Imports, i => i.moduleName == Resources.CachingModuleName);
            }

            return false;
        }
    }
}
