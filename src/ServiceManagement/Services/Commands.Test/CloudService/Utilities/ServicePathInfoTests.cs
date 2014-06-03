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

namespace Microsoft.WindowsAzure.Commands.Test.CloudService.Utilities
{
    using Commands.Utilities.CloudService;
    using Commands.Utilities.Properties;
    using System;
    using Test.Utilities.Common;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ServicePathInfoTests
    {
        [TestMethod]
        public void ServicePathInfoTest()
        {
            PowerShellProjectPathInfo paths = new PowerShellProjectPathInfo("MyService");
            AzureAssert.AreEqualServicePathInfo("MyService", paths);
        }

        [TestMethod]
        public void ServicePathInfoTestEmptyRootPathFail()
        {
            try
            {
                PowerShellProjectPathInfo paths = new PowerShellProjectPathInfo(string.Empty);
                Assert.Fail("No exception was thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
                Assert.AreEqual<string>(string.Format(Resources.InvalidOrEmptyArgumentMessage, "rootPath"), ex.Message);
            }
        }

        [TestMethod]
        public void ServicePathInfoTestNullRootPathFail()
        {
            try
            {
                PowerShellProjectPathInfo paths = new PowerShellProjectPathInfo(null);
                Assert.Fail("No exception was thrown");
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex is ArgumentException);
                Assert.AreEqual<string>(string.Format(Resources.InvalidOrEmptyArgumentMessage, "rootPath"), ex.Message);
            }
        }

        [TestMethod]
        public void ServicePathInfoTestInvalidRootPathFail()
        {
            foreach (string invalidDirectoryName in Data.InvalidServiceRootName)
            {
                try
                {
                    PowerShellProjectPathInfo paths = new PowerShellProjectPathInfo(invalidDirectoryName);
                    Assert.Fail("No exception was thrown");
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is ArgumentException);
                    Assert.AreEqual<string>(Resources.InvalidRootNameMessage, ex.Message);
                }
            }
        }
    }
}