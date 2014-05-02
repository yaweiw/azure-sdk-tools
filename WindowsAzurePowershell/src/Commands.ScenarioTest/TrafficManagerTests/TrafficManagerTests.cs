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

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.TrafficManagerTests
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.ScenarioTest.Common;

    [TestClass]
    public class TrafficManagerTests : WindowsAzurePowerShellCertificateTest
    {
        private string currentDirectory;

        public TrafficManagerTests()
            : base("TrafficManager\\Common.ps1",
                   "TrafficManager\\TrafficManagerTests.ps1")
        {
        }

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
            this.powershell.AddScript("Initialize-TrafficManagerTest");
            this.currentDirectory = Directory.GetCurrentDirectory();
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
            Directory.SetCurrentDirectory(this.currentDirectory);
        }

        #region Remove-Profile Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestRemoveProfileWithInvalidCredentials()
        {
            this.RunPowerShellTest("Test-WithInvalidCredentials { Test-CreateAndRemoveProfile }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestCreateAndRemoveProfile()
        {
            this.RunPowerShellTest("Test-CreateAndRemoveProfile");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestRemoveProfileWithNonExistingName()
        {
            this.RunPowerShellTest("Test-RemoveProfileWithNonExistingName");
        }

        #endregion

        #region Get-Profile Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestGetProfileWithInvalidCredentials()
        {
            this.RunPowerShellTest("Test-WithInvalidCredentials { Test-GetProfile }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestGetProfile()
        {
            this.RunPowerShellTest("Test-GetProfile");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestGetAndRemoveMultipleProfiles()
        {
            this.RunPowerShellTest("Test-GetAndRemoveMultipleProfiles");
        }

        #endregion

        #region Enable-Disable-Profile Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestEnableProfileWithInvalidCredentials()
        {
            this.RunPowerShellTest("Test-WithInvalidCredentials { Test-EnableProfile }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestDisableProfileWithInvalidCredentials()
        {
            this.RunPowerShellTest("Test-WithInvalidCredentials { Test-DisableProfile }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestEnableProfile()
        {
            this.RunPowerShellTest("Test-EnableProfile");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestDisableProfile()
        {
            this.RunPowerShellTest("Test-DisableProfile");
        }

        #endregion

        #region New-Profile Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestNewProfile()
        {
            this.RunPowerShellTest("Test-NewProfile");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestNewProfileInvalidParameters()
        {
            this.RunPowerShellTest("Test-NewProfileWithInvalidParameter");
        }

        #endregion

        #region Set-Profile Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestSetProfileProperty()
        {
            this.RunPowerShellTest("Test-SetProfileProperty");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestAddAzureTrafficManagerEndpoint()
        {
            this.RunPowerShellTest("Test-AddAzureTrafficManagerEndpoint");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestRemoveAzureTrafficManagerEndpoint()
        {
            this.RunPowerShellTest("Test-RemoveAzureTrafficManagerEndpoint");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestSetAzureTrafficManagerEndpoint()
        {
            this.RunPowerShellTest("Test-SetAzureTrafficManagerEndpoint");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestAddMultipleAzureTrafficManagerEndpoint()
        {
            this.RunPowerShellTest("Test-AddMultipleAzureTrafficManagerEndpoint");
        }

        #endregion
    }
}
