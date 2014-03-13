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

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.ResourceManagementTests
{
    using System.IO;
    using Commands.ScenarioTest.Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ResourceManagementTests : WindowsAzurePowerShellTest
    {
        private string currentDirectory;

        public ResourceManagementTests()
            : base("ResourceManagement\\Common.ps1",
                   "ResourceManagement\\ResourceManagementTests.ps1")
        {

        }

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
            currentDirectory = Directory.GetCurrentDirectory();
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
            Directory.SetCurrentDirectory(currentDirectory);
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ResourceManagement)]
        public void TestCreatesNewSimpleResourceGroup()
        {
            RunPowerShellTest("Test-CreatesNewSimpleResourceGroup");
        }
    }
}
