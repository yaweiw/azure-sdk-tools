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
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;

    [TestClass]
    public class AddAzureCacheWorkerRoleTests : TestBase
    {
        [TestMethod]
        public void AddAzureCacheWorkerRoleProcess()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string servicePath = Path.Combine(files.RootPath, "AzureService");
                string roleName = "WorkerRole";
                new NewAzureServiceProjectCommand().NewAzureServiceProcess(files.RootPath, "AzureService");
                new AddAzureCacheWorkerRoleCommand().AddAzureCacheWorkerRoleProcess(roleName, 1, servicePath);

                WorkerRole cacheWorkerRole = Testing.GetWorkerRole(servicePath, roleName);
                RoleSettings cacheRoleSettings = Testing.GetRole(servicePath, roleName);

                AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", "WorkerRole"), Path.Combine(Resources.NodeScaffolding, Resources.WorkerRole));

                AzureAssert.WorkerRoleImportsExists(new Import { moduleName = Resources.CachingModuleName }, cacheWorkerRole);

                AzureAssert.LocalResourcesLocalStoreExists(new LocalStore { name = Resources.CacheDiagnosticStoreName, cleanOnRoleRecycle = false }, 
                    cacheWorkerRole.LocalResources);

                Assert.IsNull(cacheWorkerRole.Endpoints.InputEndpoint);

                AzureAssert.ConfigurationSettingExist(new ConfigConfigurationSetting { name = Resources.NamedCacheSettingName, value = Resources.NamedCacheSettingValue }, cacheRoleSettings.ConfigurationSettings);
                AzureAssert.ConfigurationSettingExist(new ConfigConfigurationSetting { name = Resources.DiagnosticLevelName, value = Resources.DiagnosticLevelValue }, cacheRoleSettings.ConfigurationSettings);
                AzureAssert.ConfigurationSettingExist(new ConfigConfigurationSetting { name = Resources.CachingCacheSizePercentageSettingName, value = string.Empty }, cacheRoleSettings.ConfigurationSettings);
                AzureAssert.ConfigurationSettingExist(new ConfigConfigurationSetting { name = Resources.CachingConfigStoreConnectionStringSettingName, value = string.Empty }, cacheRoleSettings.ConfigurationSettings);
            }
        }
    }
}
