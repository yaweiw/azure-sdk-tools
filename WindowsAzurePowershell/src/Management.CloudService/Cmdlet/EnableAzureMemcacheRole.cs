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
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml.Linq;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.CloudService.AzureTools;
    using Microsoft.WindowsAzure.Management.CloudService.Cmdlet.Common;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.Scaffolding;
    using Model;
    using ServiceDefinitionSchema;
    using Utilities;
    using DefConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema.ConfigurationSetting;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;

    /// <summary>
    /// Enables memcache for specific role.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Enable, "AzureMemcache")]
    public class EnableAzureMemcacheRoleCommand : CloudCmdlet<IServiceManagement>
    {
        /// <summary>
        /// The role name to edit.
        /// </summary>
        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("rn")]
        public string RoleName { get; set; }

        /// <summary>
        /// The dedicated caching worker role name.
        /// </summary>
        [Parameter(Position = 1, Mandatory = true, ValueFromPipelineByPropertyName = true)]
        [Alias("cn")]
        public string CacheWorkerRoleName { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        protected override void ProcessRecord()
        {
            try
            {
                base.ProcessRecord();
                string result = EnableAzureMemcacheRoleProcess(this.RoleName, this.CacheWorkerRoleName, base.GetServiceRootPath());
                SafeWriteObject(result);
            }
            catch (Exception ex)
            {
                SafeWriteError(ex);
            }
        }

        /// <summary>
        /// Process for enabling memcache for web roles.
        /// </summary>
        /// <param name="roleName">The web role name</param>
        /// <param name="cacheWorkerRoleName">The cache worker role name</param>
        /// <param name="rootPath">The root path of the services</param>
        /// <returns>The resulted message</returns>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public string EnableAzureMemcacheRoleProcess(string roleName, string cacheWorkerRoleName, string rootPath)
        {
            string message = string.Empty;
            AzureService azureService = new AzureService(rootPath, null);

            // Verify cache worker role exists
            if (!azureService.Components.RoleExists(cacheWorkerRoleName))
            {
                return string.Format(Resources.RoleNotFoundMessage, cacheWorkerRoleName);
            }

            WebRole webRole = azureService.Components.GetWebRole(roleName);

            if (webRole != null)
            {
                if (!IsCacheEnabled(webRole))
                {
                    EnableMemcacheForWebRole(roleName, cacheWorkerRoleName, ref message, ref azureService);
                }
                else
                {
                    message = string.Format(Resources.CacheAlreadyEnabledMsg, roleName);
                }
            }
            else
            {
                message = string.Format(Resources.RoleNotFoundMessage, roleName);
            }

            return message;
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
        private void EnableMemcacheForWebRole(string roleName, string cacheWorkerRoleName, ref string message, ref AzureService azureService)
        {
            string currentVersion = new AzureTool().AzureSdkVersion;

            // Add MemcacheShim runtime installation.
            new SetAzureServiceProjectRoleCommand().SetAzureRuntimesProcess(roleName, Resources.CacheRuntimeValue, currentVersion, azureService.Paths.RootPath);

            // Fetch web role information.
            azureService = new AzureService(azureService.Paths.RootPath, null);
            WebRole webRole = azureService.Components.GetWebRole(roleName);

            // Assert that cache runtime is added to the runtime startup.
            Debug.Assert(Array.Exists<Variable>(CloudRuntime.GetRuntimeStartupTask(webRole.Startup).Environment,
                v => v.name.Equals(Resources.RuntimeTypeKey) && v.value.Contains(Resources.CacheRuntimeValue)));

            CachingConfigurationFactoryMethod(azureService, webRole, cacheWorkerRoleName, currentVersion);

            // Save changes
            azureService.Components.Save(azureService.Paths);

            message = string.Format(Resources.EnableMemcacheMessage, roleName, cacheWorkerRoleName, Resources.MemcacheEndpointPort);
        }

        /// <summary>
        /// Factory method to apply memcache required configuration based on the installed SDK version.
        /// </summary>
        /// <param name="azureService">The azure service instance</param>
        /// <param name="webRole">The web role to enable caching on</param>
        /// <param name="cacheWorkerRole">The memcache worker role name</param>
        /// <param name="sdkVersion">The current SDK version</param>
        private void CachingConfigurationFactoryMethod(AzureService azureService, WebRole webRole, string cacheWorkerRole, string sdkVersion)
        {
            switch (sdkVersion)
            {
                case SDKVersion.Version180:
                    Version180Configuration(azureService, webRole, cacheWorkerRole);
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
        /// <param name="cacheWorkerRole">The memcache worker role name</param>
        private void Version180Configuration(AzureService azureService, WebRole webRole, string cacheWorkerRole)
        {
            string roleName = webRole.name;

            // Generate cache caffolding for web role
            azureService.GenerateScaffolding(Path.Combine(Resources.CacheScaffolding, RoleType.WebRole.ToString()),
                roleName, new Dictionary<string, object>());

            // Add startup task to install memcache shim on the client side.
            Task shimStartupTask = new Task { commandLine = Resources.CacheStartupCommand, executionContext = ExecutionContext.elevated };
            webRole.Startup.Task = General.ExtendArray<Task>(webRole.Startup.Task, shimStartupTask);
                
            // Add default memcache internal endpoint.
            InternalEndpoint memcacheEndpoint = new InternalEndpoint
            {
                name = Resources.MemcacheEndpointName,
                protocol = InternalProtocol.tcp,
                port = Resources.MemcacheEndpointPort
            };
            webRole.Endpoints.InternalEndpoint = General.ExtendArray<InternalEndpoint>(webRole.Endpoints.InternalEndpoint, memcacheEndpoint);

            // Enable cache diagnostic
            LocalStore localStore = new LocalStore
            {
                name = Resources.CacheDiagnosticStoreName,
                cleanOnRoleRecycle = false
            };
            webRole.LocalResources = General.InitializeIfNull<LocalResources>(webRole.LocalResources);
            webRole.LocalResources.LocalStorage = General.ExtendArray<LocalStore>(webRole.LocalResources.LocalStorage, localStore);

            DefConfigurationSetting diagnosticLevel = new DefConfigurationSetting { name = Resources.CacheClientDiagnosticLevelAssemblyName };
            webRole.ConfigurationSettings = General.ExtendArray<DefConfigurationSetting>(webRole.ConfigurationSettings, diagnosticLevel);

            // Add ClientDiagnosticLevel setting to service configuration.
            RoleSettings roleSettings = azureService.Components.GetCloudConfigRole(roleName);
            ConfigConfigurationSetting clientDiagnosticLevel = new ConfigConfigurationSetting { name = Resources.ClientDiagnosticLevelName, value = Resources.ClientDiagnosticLevelValue };
            roleSettings.ConfigurationSettings = General.ExtendArray<ConfigConfigurationSetting>(roleSettings.ConfigurationSettings, clientDiagnosticLevel);

            // Adjust web.config to enable auto discovery for the caching role.
            UpdateWebCloudConfig(roleName, cacheWorkerRole, azureService);
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
        /// Checks if memcache is already enabled or not for the web role. It does the check by looking for the memcache internal endpoint.
        /// </summary>
        /// <param name="webRole">Web role to check</param>
        /// <returns>Either enabled or not</returns>
        private bool IsCacheEnabled(WebRole webRole)
        {
            if (webRole.Endpoints.InternalEndpoint != null)
            {
                return Array.Exists<InternalEndpoint>(webRole.Endpoints.InternalEndpoint, i => i.name == Resources.MemcacheEndpointName);
            }

            return false;
        }
    }
}
