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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests.Python.Cmdlet
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using CloudService.Cmdlet;
    using CloudService.Properties;
    using CloudService.Python.Cmdlet;
    using Management.Utilities;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AddAzurePythonWebRoleTests : TestBase
    {

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

            using (FileSystemHelper files = new FileSystemHelper(this))
            {
                var curDir = Environment.CurrentDirectory;
                try
                {
                    new NewAzureServiceProjectCommand().NewAzureServiceProcess(files.RootPath, "AzureService");
                    new AddAzureDjangoWebRoleCommand().AddAzureDjangoWebRoleProcess("WebRole", 1, Path.Combine(files.RootPath, "AzureService"));
                }
                finally
                {
                    Environment.CurrentDirectory = curDir;
                }

                AzureAssert.ScaffoldingExists(Path.Combine(files.RootPath, "AzureService", "WebRole"), Path.Combine(Resources.PythonScaffolding, Resources.WebRole));
            }
        }
    }
}