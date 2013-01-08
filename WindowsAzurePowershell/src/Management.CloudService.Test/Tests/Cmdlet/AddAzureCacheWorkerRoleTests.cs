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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests
{
    using System.IO;
    using System.Management.Automation;
    using CloudService.Cmdlet;
    using CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using System;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Extensions;
    using Microsoft.WindowsAzure.Management.Services;
    using Microsoft.WindowsAzure.Management.CloudService.Test.TestData;

    [TestClass]
    public class AddAzureCacheWorkerRoleTests : TestBase
    {
        private MockCommandRuntime mockCommandRuntime;

        private NewAzureServiceProjectCommand newServiceCmdlet;

        private AddAzureCacheWorkerRoleCommand addCacheRoleCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();

            newServiceCmdlet = new NewAzureServiceProjectCommand();
            addCacheRoleCmdlet = new AddAzureCacheWorkerRoleCommand();

            newServiceCmdlet.CommandRuntime = mockCommandRuntime;
            addCacheRoleCmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void AddNewCacheWorkerRoleSuccessful()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string rootPath = Path.Combine(files.RootPath, "AzureService");
                string roleName = "WorkerRole";
                int expectedInstanceCount = 10;
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                WorkerRole cacheWorkerRole = addCacheRoleCmdlet.AddAzureCacheWorkerRoleProcess(roleName, expectedInstanceCount, rootPath);
                RoleSettings cacheRoleSettings = Testing.GetRole(rootPath, roleName);

                AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", "WorkerRole"), Path.Combine(Resources.NodeScaffolding, Resources.WorkerRole));

                AzureAssert.WorkerRoleImportsExists(new Import { moduleName = Resources.CachingModuleName }, cacheWorkerRole);

                AzureAssert.LocalResourcesLocalStoreExists(new LocalStore { name = Resources.CacheDiagnosticStoreName, cleanOnRoleRecycle = false }, 
                    cacheWorkerRole.LocalResources);

                Assert.IsNull(cacheWorkerRole.Endpoints.InputEndpoint);

                AzureAssert.ConfigurationSettingExist(new ConfigConfigurationSetting { name = Resources.NamedCacheSettingName, value = Resources.NamedCacheSettingValue }, cacheRoleSettings.ConfigurationSettings);
                AzureAssert.ConfigurationSettingExist(new ConfigConfigurationSetting { name = Resources.DiagnosticLevelName, value = Resources.DiagnosticLevelValue }, cacheRoleSettings.ConfigurationSettings);
                AzureAssert.ConfigurationSettingExist(new ConfigConfigurationSetting { name = Resources.CachingCacheSizePercentageSettingName, value = string.Empty }, cacheRoleSettings.ConfigurationSettings);
                AzureAssert.ConfigurationSettingExist(new ConfigConfigurationSetting { name = Resources.CachingConfigStoreConnectionStringSettingName, value = string.Empty }, cacheRoleSettings.ConfigurationSettings);

                PSObject actualOutput = mockCommandRuntime.OutputPipeline[1] as PSObject;
                Assert.AreEqual<string>(roleName, actualOutput.Members[Parameters.CacheWorkerRoleName].Value.ToString());
                Assert.AreEqual<int>(expectedInstanceCount, int.Parse(actualOutput.Members[Parameters.Instances].Value.ToString()));
            }
        }

        [TestMethod]
        public void AddNewCacheWorkerRoleWithInvalidNamesFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string rootPath = Path.Combine(files.RootPath, "AzureService");
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");

                foreach (string invalidName in TestData.Data.InvalidRoleNames)
                {
                    Testing.AssertThrows<ArgumentException>(() => addCacheRoleCmdlet.AddAzureCacheWorkerRoleProcess(invalidName, 1, rootPath));
                }
            }
        }
    }
}
