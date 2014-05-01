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

    //[TestClass]
    public class TrafficManagerTests : WindowsAzurePowerShellCertificateTest
    {
/*        private string currentDirectory;

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
            this.RunPowerShellTest("Test-WithInvalidCredentials {Remove-Profile -Name $(Get-ProfileName) }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestRemoveProfileWithValidName()
        {
            this.RunPowerShellTest("Test-RemoveProfile");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestRemoveProfileWithNonExistingName()
        {
            this.RunPowerShellTest("Test-RemoveProfileNonExistingName");
        }

        #endregion

        #region Get-Profile Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestGetProfileWithInvalidCredentials()
        {
            this.RunPowerShellTest("Test-WithInvalidCredentials { Get-Profile -Name $(Get-WebsiteName) }");
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
        public void TestGetProfiles()
        {
            this.RunPowerShellTest("Test-GetProfiles");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestGetProfileNonExistingName()
        {
            this.RunPowerShellTest("Test-GetProfileNonExistingName");
        }

        #endregion

        /*
        #region Enable-Disable-Profile Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestEnableProfileWithInvalidCredentials()
        {
            this.RunPowerShellTest("Test-WithInvalidCredentials { Enable-AzureTrafficManagerProfile  -Name $(Get-WebsiteName) }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestDisableProfileWithInvalidCredentials()
        {
            this.RunPowerShellTest("Test-WithInvalidCredentials { Disable-AzureTrafficManagerProfile e -Name $(Get-WebsiteName) }");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestEnableDisableProfile()
        {
            this.RunPowerShellTest("Test-EnableDisableProfile");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestEnableProfileNonExistingName()
        {
            this.RunPowerShellTest("Test-EnableProfileNonExistingName");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestDisableProfileNonExistingName()
        {
            this.RunPowerShellTest("Test-DisableProfileNonExistingName");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestDisableEnableProfilePipedWithGetProfile()
        {
            this.RunPowerShellTest("Test-DisableProfilePipedWithGetProfile");
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
            this.RunPowerShellTest("Test-NewProfileInvalidParameters");
        }

        #endregion

        #region Set-Profile Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestSetProfile()
        {
            this.RunPowerShellTest("Test-SetProfile");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestSetProfileInvalidParameters()
        {
            this.RunPowerShellTest("Test-SetProfileInvalidParameters");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.TrafficManager)]
        public void TestSetProfileWithWebsites()
        {
            this.RunPowerShellTest("Test-SetProfileWithWebsites");
        }

        #endregion
        */
    }
}
