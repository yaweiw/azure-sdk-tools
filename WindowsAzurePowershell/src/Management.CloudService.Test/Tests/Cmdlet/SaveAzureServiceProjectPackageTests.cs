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

namespace Microsoft.WindowsAzure.Management.CloudService.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;
    using CloudService.Model;
    using Cmdlet;
    using Extensions;
    using Management.Services;
    using Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using Node.Cmdlet;
    using TestData;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using System.IO;
    using Microsoft.WindowsAzure.Management.CloudService.Properties;
    using System.Management.Automation;

    /// <summary>
    /// Basic unit tests for the Enable-AzureServiceProjectRemoteDesktop enableRDCmdlet.
    /// </summary>
    [TestClass]
    public class SaveAzureServiceProjectPackageCommandTest : TestBase
    {
        static private MockCommandRuntime mockCommandRuntime;

        static private SaveAzureServiceProjectPackageCommand cmdlet;

        private AddAzureNodeWebRoleCommand addNodeWebCmdlet;

        private AddAzureNodeWorkerRoleCommand addNodeWorkerCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();

            addNodeWebCmdlet = new AddAzureNodeWebRoleCommand();
            addNodeWorkerCmdlet = new AddAzureNodeWorkerRoleCommand();
            cmdlet = new SaveAzureServiceProjectPackageCommand();

            addNodeWorkerCmdlet.CommandRuntime = mockCommandRuntime;
            addNodeWebCmdlet.CommandRuntime = mockCommandRuntime;
            cmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void TestCreatePackageSuccessfull()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                files.CreateNewService("NEW_SERVICE");
                string rootPath = Path.Combine(files.RootPath, "NEW_SERVICE");
                string packagePath = Path.Combine(rootPath, Resources.CloudPackageFileName);

                AzureService service = new AzureService(rootPath, null);
                service.AddWebRole(Resources.NodeScaffolding);

                cmdlet.ExecuteCmdlet();

                PSObject obj = mockCommandRuntime.OutputPipeline[0] as PSObject;
                Assert.AreEqual<string>(string.Format(Resources.PackageCreated, packagePath), mockCommandRuntime.VerboseStream[0]);
                Assert.AreEqual<string>(packagePath, obj.GetVariableValue<string>(Parameters.PackagePath));
                Assert.IsTrue(File.Exists(packagePath));
            }
        }

        [TestMethod]
        public void TestCreatePackageWithEmptyServiceSuccessfull()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                files.CreateNewService("NEW_SERVICE");
                string rootPath = Path.Combine(files.RootPath, "NEW_SERVICE");
                string packagePath = Path.Combine(rootPath, Resources.CloudPackageFileName);

                cmdlet.ExecuteCmdlet();

                PSObject obj = mockCommandRuntime.OutputPipeline[0] as PSObject;
                Assert.AreEqual<string>(string.Format(Resources.PackageCreated, packagePath), mockCommandRuntime.VerboseStream[0]);
                Assert.AreEqual<string>(packagePath, obj.GetVariableValue<string>(Parameters.PackagePath));
                Assert.IsTrue(File.Exists(packagePath));
            }
        }

        [TestMethod]
        public void TestCreatePackageWithMultipleRolesSuccessfull()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                files.CreateAzureSdkDirectoryAndImportPublishSettings();
                files.CreateNewService("NEW_SERVICE");
                string rootPath = Path.Combine(files.RootPath, "NEW_SERVICE");
                string packagePath = Path.Combine(rootPath, Resources.CloudPackageFileName);

                AzureService service = new AzureService(rootPath, null);
                service.AddWebRole(Resources.NodeScaffolding);
                service.AddWorkerRole(Resources.NodeScaffolding);
                service.AddWorkerRole(Resources.NodeScaffolding);
                service.AddWebRole(Resources.NodeScaffolding);

                cmdlet.ExecuteCmdlet();

                PSObject obj = mockCommandRuntime.OutputPipeline[0] as PSObject;
                Assert.AreEqual<string>(string.Format(Resources.PackageCreated, packagePath), mockCommandRuntime.VerboseStream[0]);
                Assert.AreEqual<string>(packagePath, obj.GetVariableValue<string>(Parameters.PackagePath));
                Assert.IsTrue(File.Exists(packagePath));
            }
        }
    }
}
