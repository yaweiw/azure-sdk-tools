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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Python.Cmdlet
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation;
    using CloudService.Cmdlet;
    using CloudService.Properties;
    using CloudService.Python.Cmdlet;
    using Management.Utilities;
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using Microsoft.WindowsAzure.Management.CloudService.Test.TestData;
    using Microsoft.WindowsAzure.Management.Extensions;
    using Microsoft.WindowsAzure.Management.Services;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AddAzurePythonWebRoleTests : TestBase
    {
        private MockCommandRuntime mockCommandRuntime;

        private NewAzureServiceProjectCommand newServiceCmdlet;

        private AddAzureDjangoWebRoleCommand addPythonWebCmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.GlobalSettingsDirectory = Data.AzureSdkAppDir;
            CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            mockCommandRuntime = new MockCommandRuntime();

            newServiceCmdlet = new NewAzureServiceProjectCommand();
            addPythonWebCmdlet = new AddAzureDjangoWebRoleCommand();

            addPythonWebCmdlet.CommandRuntime = mockCommandRuntime;
            newServiceCmdlet.CommandRuntime = mockCommandRuntime;
        }

        [TestMethod]
        public void AddAzurePythonWebRoleProcess()
        {
            var pyInstall = AddAzureDjangoWebRoleCommand.FindPythonInterpreterPath();
            if (pyInstall == null)
            {
                Assert.Inconclusive("Python is not installed on this machine and therefore the Python tests cannot be run");
                return;
            }

            string stdOut, stdErr;
            ProcessHelper.StartAndWaitForProcess(
                    new ProcessStartInfo(
                        Path.Combine(pyInstall, "python.exe"),
                        String.Format("-m django.bin.django-admin")
                    ),
                    out stdOut,
                    out stdErr
            );

            if (stdOut.IndexOf("django-admin.py") == -1)
            {
                Assert.Inconclusive("Django is not installed on this machine and therefore the Python tests cannot be run");
                return;
            }

            FileSystemHelper files = new FileSystemHelper(this);
            string roleName = "WebRole1";
            string rootPath = Path.Combine(files.RootPath, "AzureService");
            string expectedVerboseMessage = string.Format(Resources.AddRoleMessageCreatePython, rootPath, roleName);
            newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
            mockCommandRuntime.ResetPipelines();
            addPythonWebCmdlet.AddAzureDjangoWebRoleProcess(roleName, 1, rootPath);

            AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", roleName), Path.Combine(Resources.PythonScaffolding, Resources.WebRole));
            Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[0]).GetVariableValue<string>(Parameters.RoleName));
            Assert.AreEqual<string>(expectedVerboseMessage, mockCommandRuntime.VerboseStream[0]);
        }

        [TestMethod]
        public void AddAzurePythonWebRoleWillRecreateDeploymentSettings()
        {
            FileSystemHelper files = new FileSystemHelper(this);
            string roleName = "WebRole1";
            string rootPath = Path.Combine(files.RootPath, "AzureService");
            string expectedVerboseMessage = string.Format(Resources.AddRoleMessageCreate, rootPath, roleName);
            string settingsFilePath = Path.Combine(rootPath, Resources.SettingsFileName);
            newServiceCmdlet.NewAzureServiceProcess(files.RootPath, "AzureService");
            File.Delete(settingsFilePath);
            Assert.IsFalse(File.Exists(settingsFilePath));

            addPythonWebCmdlet.AddAzureDjangoWebRoleProcess(roleName, 1, rootPath);

            AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", roleName), Path.Combine(Resources.PythonScaffolding, Resources.WebRole));
            Assert.AreEqual<string>(roleName, ((PSObject)mockCommandRuntime.OutputPipeline[1]).GetVariableValue<string>(Parameters.RoleName));
            Assert.IsTrue(File.Exists(settingsFilePath));
        }
    }
}