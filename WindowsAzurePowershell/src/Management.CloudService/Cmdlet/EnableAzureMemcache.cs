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
    using System.Management.Automation;
    using System.Security;
    using System.Security.Permissions;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.CloudService.Cmdlet.Common;
    using Microsoft.WindowsAzure.Management.CloudService.Node.Cmdlet;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using Model;
    using ServiceConfigurationSchema;
    using ServiceDefinitionSchema;
    using Utilities;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;
    using System.Web.Configuration;
    using System.Configuration;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.WindowsAzure.Management.CloudService.Scaffolding;
    using Microsoft.WindowsAzure.Management.CloudService.AzureTools;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Enables memcache for specific role.
    /// </summary>
    [Cmdlet(VerbsLifecycle.Enable, "AzureMemcache")]
    public class EnableAzureMemcacheCommand : CloudCmdlet<IServiceManagement>
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
                string result = EnableAzureMemcacheProcess(this.RoleName, this.CacheWorkerRoleName, base.GetServiceRootPath());
                SafeWriteObject(result);
            }
            catch (Exception ex)
            {
                SafeWriteError(ex);
            }
        }

        /// <summary>
        /// Enables Memcached for roles by:
        /// * Add MemcacheShim runtime installation.
        /// * Add startup task to install memcache shim on the client side.
        /// * Add default memcache internal endpoint.
        /// * Adjust web.config to enable auto discovery for the caching role.
        /// </summary>
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public string EnableAzureMemcacheProcess(string roleName, string cacheWorkerRoleName, string rootPath)
        {
            string message = string.Empty;
            AzureService azureService = new AzureService(rootPath, null);

            // Verify caching worker role exists
            if (!azureService.Components.RoleExists(cacheWorkerRoleName))
            {
                return string.Format(Resources.RoleNotFoundMessage, cacheWorkerRoleName);
            }

            if (azureService.Components.RoleExists(roleName))
            {
                // Add MemcacheShim runtime installation.
                new SetAzureServiceProjectRoleCommand().SetAzureRuntimesProcess(roleName, Resources.CacheRuntimeValue, 
                    new AzureTool().AzureSdkVersion, rootPath);
                
                // Fetch web role information.
                azureService = new AzureService(rootPath, null);
                WebRole webRole = azureService.Components.GetWebRole(roleName);
                
                // Assert that cache runtime is added to the runtime startup.
                Debug.Assert(Array.Exists<Variable>(CloudRuntime.GetRuntimeStartupTask(webRole.Startup).Environment, 
                    v => v.name.Equals(Resources.RuntimeTypeKey) && v.value.Contains(Resources.CacheRuntimeValue)));

                // Generate cache caffolding for web role
                azureService.GenerateScaffolding(Path.Combine(Resources.CacheScaffolding, RoleType.WebRole.ToString()), roleName, new Dictionary<string, object>());

                // Add startup task to install memcache shim on the client side.
                Task shimStartupTask = new Task { commandLine = Resources.CacheStartupCommand, executionContext = ExecutionContext.elevated };
                webRole.Startup.Task = General.ExtendArray<Task>(webRole.Startup.Task, shimStartupTask);

                // Add default memcache internal endpoint.
                InternalEndpoint memcacheEndpoint = new InternalEndpoint { name = Resources.MemcacheEndpointName, protocol = InternalProtocol.tcp, 
                FixedPort = new Port[] { new Port { port = ushort.Parse(Resources.MemcacheEndpointPort) } } };
                webRole.Endpoints.InternalEndpoint = General.ExtendArray<InternalEndpoint>(webRole.Endpoints.InternalEndpoint, memcacheEndpoint);

                // Adjust web.config to enable auto discovery for the caching role.
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
            else
            {
                message = string.Format(Resources.RoleNotFoundMessage, roleName);
            }

            // Save changes
            azureService.Components.Save(azureService.Paths);

            return message;
        }
    }
}
