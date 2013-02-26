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

namespace Microsoft.WindowsAzure.Management.ScenarioTest.StoreTests
{
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.ScenarioTest.Common.CustomPowerShell;
    using Microsoft.WindowsAzure.Management.Store.Model;

    [TestClass]
    public class StoreTests : WindowsAzurePowerShellTest
    {
        public static string StoreCredentialFile = "store.publishsettings";

        public static string StoreSubscriptionName = "Store";

        private CustomHost customHost;

        public StoreTests()
            : base("Store\\StoreTests.ps1")
        {
            customHost = new CustomHost();
        }

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
            customHost = new CustomHost();
        }

        private void PromptSetup(int choice)
        {
            customHost.CustomUI.PromptChoice = choice;
            powershell.Runspace = RunspaceFactory.CreateRunspace(customHost);
            powershell.Runspace.Open();
            powershell.ImportCredentials(StoreCredentialFile);
            powershell.AddScript(string.Format("Set-AzureSubscription -Default {0}", StoreSubscriptionName));
        }

        #region Get-AzureStoreAvailableAddOn Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.Store)]
        public void TestWithInvalidCredentialsWorks()
        {
            RunPowerShellTest("Test-WithInvalidCredentialsWorks");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.Store)]
        public void TestGetAzureStoreAvailableAddOnWithDefaultCountry()
        {
            RunPowerShellTest("Test-GetAzureStoreAvailableAddOnWithDefaultCountry");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.Store)]
        public void TestGetAzureStoreAvailableAddOnWithNoAddOns()
        {
            RunPowerShellTest("Test-GetAzureStoreAvailableAddOnWithNoAddOns");
        }

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.Store)]
        public void TestGetAzureStoreAvailableAddOnWithInvalidCountryName()
        {
            RunPowerShellTest("Test-GetAzureStoreAvailableAddOnWithInvalidCountryName");
        }

        #endregion

        #region Get-AzureStoreAddOn Scneario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.Store)]
        public void TestNewAzureStoreAddOnWithInvalidCredentials()
        {
            RunPowerShellTest("Test-WithInvalidCredentials { New-AzureStoreAddOn Name AddOnPlan Location}");
        }

        #endregion
    }
}
