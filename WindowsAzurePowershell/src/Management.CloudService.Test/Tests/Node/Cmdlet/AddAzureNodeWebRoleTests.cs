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
    using CloudService.Node.Cmdlet;
    using CloudService.Properties;
    using Microsoft.WindowsAzure.Management.CloudService.Test.TestData;
    using Microsoft.WindowsAzure.Management.Extensions;
    using Microsoft.WindowsAzure.Management.Services;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AddAzureNodeWebRoleTests : TestBase
    {
        private MockCommandRuntime mockCommandRuntime;

        private NewAzureServiceProjectCommand newServiceCmdlet;

        private AddAzureNodeWebRoleCommand addNodeWebCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();

            newServiceCmdlet = new NewAzureServiceProjectCommand();
            addNodeWebCmdlet = new AddAzureNodeWebRoleCommand();

            addNodeWebCmdlet.CommandRuntime = mockCommandRuntime;
            newServiceCmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void AddAzureNodeWebRoleProcess()
        {
            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
                addNodeWebCmdlet.AddAzureNodeWebRoleProcess("WebRole", 1, Path.Combine(files.RootPath, "AzureService"));

                AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", "WebRole"), Path.Combine(Resources.NodeScaffolding, Resources.WebRole));
            }
        }
    }
}
