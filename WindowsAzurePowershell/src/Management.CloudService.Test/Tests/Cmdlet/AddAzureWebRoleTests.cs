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
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using Microsoft.WindowsAzure.Management.CloudService.Test.TestData;
    using Microsoft.WindowsAzure.Management.Extensions;
    using Microsoft.WindowsAzure.Management.Services;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AddAzureWebRoleTests : TestBase
    {
        private MockCommandRuntime mockCommandRuntime;

        private NewAzureServiceProjectCommand newServiceCmdlet;

        private AddAzureWebRoleCommand addWebCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();

            newServiceCmdlet = new NewAzureServiceProjectCommand();
            addWebCmdlet = new AddAzureWebRoleCommand();

            addWebCmdlet.CommandRuntime = mockCommandRuntime;
            newServiceCmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void AddAzureWebRoleProcess()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string roleName = "WebRole1";
                string rootPath = Path.Combine(files.RootPath, "AzureService");
                string expectedVerboseMessage = string.Format(Resources.AddRoleMessageCreate, rootPath, roleName);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                string originalDirectory = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(rootPath);
                addWebCmdlet.Name = roleName;
                addWebCmdlet.Instances = 1;

                addWebCmdlet.ExecuteCmdlet();

                AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", roleName), Path.Combine(Resources.GeneralScaffolding, Resources.WebRole));
                Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[1]).GetVariableValue<string>(Parameters.RoleName));
                Assert.AreEqual<string>(expectedVerboseMessage, mockCommandRuntime.VerboseStream[1]);

                Directory.SetCurrentDirectory(originalDirectory);
            }
        }

        [TestMethod]
        public void AddAzureWebRoleWillRecreateDeploymentSettings()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                string roleName = "WebRole1";
                string rootPath = Path.Combine(files.RootPath, "AzureService");
                string expectedVerboseMessage = string.Format(Resources.AddRoleMessageCreate, rootPath, roleName);
                string settingsFilePath = Path.Combine(rootPath, Resources.SettingsFileName);
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                string originalDirectory = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(rootPath);
                File.Delete(settingsFilePath);
                Assert.IsFalse(File.Exists(settingsFilePath));
                addWebCmdlet.Name = roleName;
                addWebCmdlet.Instances = 1;

                addWebCmdlet.ExecuteCmdlet();

                AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", roleName), Path.Combine(Resources.GeneralScaffolding, Resources.WebRole));
                Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[1]).GetVariableValue<string>(Parameters.RoleName));
                Assert.AreEqual<string>(expectedVerboseMessage, mockCommandRuntime.VerboseStream[1]);
                Assert.IsTrue(File.Exists(settingsFilePath));

                Directory.SetCurrentDirectory(originalDirectory);
            }
        }
    }
}
