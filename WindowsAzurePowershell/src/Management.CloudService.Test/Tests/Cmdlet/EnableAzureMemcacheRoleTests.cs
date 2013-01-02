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
    using System;
    using System.IO;
    using CloudService.Cmdlet;
    using CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.Node.Cmdlet;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema;
    using Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema;
    using Microsoft.WindowsAzure.Management.CloudService.Test.TestData;
    using Microsoft.WindowsAzure.Management.Extensions;
    using Microsoft.WindowsAzure.Management.Services;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using ConfigConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceConfigurationSchema.ConfigurationSetting;
    using DefConfigurationSetting = Microsoft.WindowsAzure.Management.CloudService.ServiceDefinitionSchema.ConfigurationSetting;

    [TestClass]
    public class EnableAzureMemcacheRoleTests : TestBase
    {
        private MockCommandRuntime mockCommandRuntime;

        private NewAzureServiceProjectCommand newServiceCmdlet;

        private AddAzureNodeWebRoleCommand addNodeWebCmdlet;

        private AddAzureNodeWorkerRoleCommand addNodeWorkerCmdlet;

        private AddAzureCacheWorkerRoleCommand addCacheRoleCmdlet;

        private EnableAzureMemcacheRoleCommand enableCacheCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();

            enableCacheCmdlet = new EnableAzureMemcacheRoleCommand();
            newServiceCmdlet = new NewAzureServiceProjectCommand();
            addNodeWebCmdlet = new AddAzureNodeWebRoleCommand();
            addNodeWorkerCmdlet = new AddAzureNodeWorkerRoleCommand();
            addCacheRoleCmdlet = new AddAzureCacheWorkerRoleCommand();

            addCacheRoleCmdlet.CommandRuntime = mockCommandRuntime;
            addNodeWorkerCmdlet.CommandRuntime = mockCommandRuntime;
            addNodeWebCmdlet.CommandRuntime = mockCommandRuntime;
            newServiceCmdlet.CommandRuntime = mockCommandRuntime;
            enableCacheCmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void EnableAzureMemcacheRoleProcess()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string serviceName = "AzureService";
                string servicePath = Path.Combine(files.RootPath, serviceName);
                string cacheRoleName = "WorkerRole";
                string webRoleName = "WebRole";
                string expectedMessage = string.Format(Resources.EnableMemcacheMessage, webRoleName, cacheRoleName, Resources.MemcacheEndpointPort);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                addNodeWebCmdlet.AddAzureNodeWebRoleProcess(webRoleName, 1, servicePath);
                addCacheRoleCmdlet.AddAzureCacheWorkerRoleProcess(cacheRoleName, 1, servicePath);
                mockCommandRuntime.ResetPipelines();
                enableCacheCmdlet.PassThru = true;
                enableCacheCmdlet.EnableAzureMemcacheRoleProcess(webRoleName, cacheRoleName, servicePath);

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

                Assert.AreEqual<string>(expectedMessage, mockCommandRuntime.VerboseChannel[0]);
                Assert.AreEqual<string>(webRoleName, (mockCommandRuntime.WrittenObjects[0] as RoleSettings).name);
            }
        }

        /// <summary>
        /// Verify that enabling cache with non-existing cache worker role will fail.
        /// </summary>
        [TestMethod]
        public void EnableAzureMemcacheRoleProcessCacheRoleDoesNotExistFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string serviceName = "AzureService";
                string servicePath = Path.Combine(files.RootPath, serviceName);
                string cacheRoleName = "WorkerRole";
                string webRoleName = "WebRole";
                string expected = string.Format(Resources.RoleNotFoundMessage, cacheRoleName);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                addNodeWebCmdlet.AddAzureNodeWebRoleProcess(webRoleName, 1, servicePath);

                Testing.AssertThrows<Exception>(() => enableCacheCmdlet.EnableAzureMemcacheRoleProcess(webRoleName, cacheRoleName, servicePath));
            }
        }

        /// <summary>
        /// Verify that enabling cache with non-existing role to enable on will fail.
        /// </summary>
        [TestMethod]
        public void EnableAzureMemcacheRoleProcessRoleDoesNotExistFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string serviceName = "AzureService";
                string servicePath = Path.Combine(files.RootPath, serviceName);
                string cacheRoleName = "WorkerRole";
                string webRoleName = "WebRole";
                string expected = string.Format(Resources.RoleNotFoundMessage, webRoleName);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                addCacheRoleCmdlet.AddAzureCacheWorkerRoleProcess(cacheRoleName, 1, servicePath);
                
                Testing.AssertThrows<Exception>(() => enableCacheCmdlet.EnableAzureMemcacheRoleProcess(webRoleName, cacheRoleName, servicePath));
            }
        }

        /// <summary>
        /// Verify that enabling cache using same cache worker role on role with cache will fail.
        /// </summary>
        [TestMethod]
        public void EnableAzureMemcacheRoleProcessAlreadyEnabledFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string serviceName = "AzureService";
                string servicePath = Path.Combine(files.RootPath, serviceName);
                string cacheRoleName = "WorkerRole";
                string webRoleName = "WebRole";
                string expected = string.Format(Resources.CacheAlreadyEnabledMsg, webRoleName);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                addNodeWebCmdlet.AddAzureNodeWebRoleProcess(webRoleName, 1, servicePath);
                addCacheRoleCmdlet.AddAzureCacheWorkerRoleProcess(cacheRoleName, 1, servicePath);
                enableCacheCmdlet.EnableAzureMemcacheRoleProcess(webRoleName, cacheRoleName, servicePath);
                
                Testing.AssertThrows<Exception>(() => enableCacheCmdlet.EnableAzureMemcacheRoleProcess(webRoleName, cacheRoleName, servicePath));
            }
        }

        /// <summary>
        /// Verify that enabling cache using different cache worker role on role with cache will fail.
        /// </summary>
        [TestMethod]
        public void EnableAzureMemcacheRoleProcessAlreadyEnabledNewCacheRoleFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string serviceName = "AzureService";
                string servicePath = Path.Combine(files.RootPath, serviceName);
                string cacheRoleName = "WorkerRole";
                string newCacheRoleName = "NewCacheWorkerRole";
                string webRoleName = "WebRole";
                string expected = string.Format(Resources.CacheAlreadyEnabledMsg, webRoleName);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                addNodeWebCmdlet.AddAzureNodeWebRoleProcess(webRoleName, 1, servicePath);
                addCacheRoleCmdlet.AddAzureCacheWorkerRoleProcess(cacheRoleName, 1, servicePath);
                enableCacheCmdlet.EnableAzureMemcacheRoleProcess(webRoleName, cacheRoleName, servicePath);
                addCacheRoleCmdlet.AddAzureCacheWorkerRoleProcess(newCacheRoleName, 1, servicePath);

                Testing.AssertThrows<Exception>(() => enableCacheCmdlet.EnableAzureMemcacheRoleProcess(webRoleName, cacheRoleName, servicePath));
            }
        }

        /// <summary>
        /// Verify that enabling cache on worker role will fail.
        /// </summary>
        [TestMethod]
        public void EnableAzureMemcacheRoleProcessOnWorkerRoleWillFail()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string serviceName = "AzureService";
                string servicePath = Path.Combine(files.RootPath, serviceName);
                string cacheRoleName = "WorkerRole";
                string workerRoleName = "WebRole";
                string expected = string.Format(Resources.EnableMemcacheOnWorkerRoleErrorMsg, workerRoleName);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                addNodeWorkerCmdlet.AddAzureNodeWorkerRoleProcess(workerRoleName, 1, servicePath);
                addCacheRoleCmdlet.AddAzureCacheWorkerRoleProcess(cacheRoleName, 1, servicePath);

                Testing.AssertThrows<Exception>(() => enableCacheCmdlet.EnableAzureMemcacheRoleProcess(workerRoleName, cacheRoleName, servicePath));
            }
        }

        /// <summary>
        /// Verify that enabling cache using non-cache worker role will fail.
        /// </summary>
        [TestMethod]
        public void EnableAzureMemcacheRoleProcessUsingNonCacheWorkerRole()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string serviceName = "AzureService";
                string servicePath = Path.Combine(files.RootPath, serviceName);
                string workerRoleName = "WorkerRole";
                string webRoleName = "WebRole";
                string expected = string.Format(Resources.NotCacheWorkerRole, workerRoleName);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                addNodeWebCmdlet.AddAzureNodeWebRoleProcess(webRoleName, 1, servicePath);
                addNodeWorkerCmdlet.AddAzureNodeWorkerRoleProcess(workerRoleName, 1, servicePath);

                Testing.AssertThrows<Exception>(() => enableCacheCmdlet.EnableAzureMemcacheRoleProcess(webRoleName, workerRoleName, servicePath));
            }
        }
    }
}
