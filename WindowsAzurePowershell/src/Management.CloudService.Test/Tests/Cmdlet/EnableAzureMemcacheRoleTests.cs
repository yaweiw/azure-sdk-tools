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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests
{
    using System.IO;
    using CloudService.Cmdlet;
    using CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.Node.Cmdlet;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;
    using DefConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema.ConfigurationSetting;

    [TestClass]
    public class EnableAzureMemcacheRoleTests : TestBase
    {
        [TestMethod]
        public void EnableAzureMemcacheRoleProcess()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string serviceName = "AzureService";
                string servicePath = Path.Combine(files.RootPath, serviceName);
                string cacheRoleName = "WorkerRole";
                string webRoleName = "WebRole";
                new NewAzureServiceProjectCommand().NewAzureServiceProcess(files.RootPath, "AzureService");
                new AddAzureNodeWebRoleCommand().AddAzureNodeWebRoleProcess(webRoleName, 1, servicePath);
                new AddAzureCacheWorkerRoleCommand().AddAzureCacheWorkerRoleProcess(cacheRoleName, 1, servicePath);
                new EnableAzureMemcacheRoleCommand().EnableAzureMemcacheRoleProcess(webRoleName, cacheRoleName, servicePath);

                WebRole webRole = Testing.GetWebRole(servicePath, webRoleName);
                RoleSettings roleSettings = Testing.GetRole(servicePath, webRoleName);

                AzureAssert.RuntimeExists(webRole.Startup.Task, Resources.CacheRuntimeValue);

                AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, serviceName, webRoleName), Path.Combine(Resources.CacheScaffolding, Resources.WebRole));
                AzureAssert.StartupTaskExists(webRole.Startup.Task, Resources.CacheStartupCommand);
                
                AzureAssert.InternalEndpointExists(webRole.Endpoints.InternalEndpoint, 
                    new InternalEndpoint { name = Resources.MemcacheEndpointName, protocol = InternalProtocol.tcp, port = Resources.MemcacheEndpointPort});

                LocalStore localStore = new LocalStore
                {
                    name = Resources.CacheDiagnosticStoreName,
                    cleanOnRoleRecycle = false
                };
                
                AzureAssert.LocalResourcesLocalStoreExists(localStore, webRole.LocalResources);

                DefConfigurationSetting diagnosticLevel = new DefConfigurationSetting { name = Resources.CacheClientDiagnosticLevelAssemblyName };
                AzureAssert.ConfigurationSettingExist(diagnosticLevel, webRole.ConfigurationSettings);

                ConfigConfigurationSetting clientDiagnosticLevel = new ConfigConfigurationSetting { name = Resources.ClientDiagnosticLevelName, value = Resources.ClientDiagnosticLevelValue };
                AzureAssert.ConfigurationSettingExist(clientDiagnosticLevel, roleSettings.ConfigurationSettings);

                string webConfigPath = string.Format(@"{0}\{1}\{2}", servicePath, webRoleName, Resources.WebCloudConfig);
                string webCloudConfig = File.ReadAllText(webConfigPath);
                Assert.IsTrue(webCloudConfig.Contains("configSections"));
                Assert.IsTrue(webCloudConfig.Contains("dataCacheClients"));
            }
        }
    }
}
