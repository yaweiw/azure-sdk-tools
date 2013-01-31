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
    using Common;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;

    [TestClass]
    public class StoreTests : WindowsAzurePowerShellTest
    {
        public StoreTests()
            : base("Store\\StoreTests.ps1")
        {

        }

        #region Get-AzureStoreAvailableAddOn Scenario Tests

        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.Store)]
        public void TestGetAzureStoreAvailableAddOnWithInvalidCredentials()
        {
            RunPowerShellTest("Test-WithInvalidCredentials { Get-AzureStoreAvailableAddOn }");
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

        #endregion
    }
}
