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

namespace Microsoft.WindowsAzure.Management.ScenarioTest.ServiceBusTests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.ScenarioTest.Common;

    [TestClass]
    public class ServiceBusAuthorizationRuleTests : WindowsAzurePowerShellTest
    {
        public ServiceBusAuthorizationRuleTests()
            : base("ServiceBus\\Common.ps1",
                   "ServiceBus\\AuthorizationRuleScenarioTests.ps1")
        {

        }

        #region New-AzureSBAuthorizationRule Scenario Tests

        /// <summary>
        /// Test New-AzureSBAuthorizationRule when creating queue without passing any SAS keys.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void CreatesAuthorizationRuleWithoutKeys()
        {
            RunPowerShellTest("Test-CreatesAuthorizationRuleWithoutKeys");
        }

        /// <summary>
        /// Test New-AzureSBAuthorizationRule when creating topic with passing just primary key.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void CreatesAuthorizationRuleWithPrimaryKey()
        {
            RunPowerShellTest("Test-CreatesAuthorizationRuleWithPrimaryKey");
        }

        /// <summary>
        /// Test New-AzureSBAuthorizationRule when creating relay with passing primary and secondary key.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void CreatesAuthorizationRuleWithPrimaryAndSecondaryKey()
        {
            RunPowerShellTest("Test-CreatesAuthorizationRuleWithPrimaryAndSecondaryKey");
        }

        /// <summary>
        /// Test New-AzureSBAuthorizationRule on notification hub scope.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void CreatesAuthorizationRuleForNotificationHub()
        {
            RunPowerShellTest("Test-CreatesAuthorizationRuleForNotificationHub");
        }

        /// <summary>
        /// Test New-AzureSBAuthorizationRule on namespace scope.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void CreatesAuthorizationRuleForNamespace()
        {
            RunPowerShellTest("Test-CreatesAuthorizationRuleForNamespace");
        }

        #endregion

        #region Set-AzureSBAuthorizationRule Scenario Tests

        /// <summary>
        /// Test Sets-AzureSBAuthorizationRule when creating queue and renewing primary key.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void SetsAuthorizationRuleRenewPrimaryKey()
        {
            RunPowerShellTest("Test-SetsAuthorizationRuleRenewPrimaryKey");
        }

        /// <summary>
        /// Test Sets-AzureSBAuthorizationRule when creating topic and setting secondary key.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void SetsAuthorizationRuleSecondaryKey()
        {
            RunPowerShellTest("Test-SetsAuthorizationRuleSecondaryKey");
        }

        /// <summary>
        /// Test Sets-AzureSBAuthorizationRule when creating notification hub and changing the permissions.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void SetsAuthorizationRuleForPermission()
        {
            RunPowerShellTest("Test-SetsAuthorizationRuleForPermission");
        }

        /// <summary>
        /// Test Set-AzureSBAuthorizationRule on namespace level.
        /// </summary>
        [TestMethod]
        [TestCategory(Category.All)]
        [TestCategory(Category.ServiceBus)]
        public void SetsAuthorizationRuleOnNamespace()
        {
            RunPowerShellTest("Test-SetsAuthorizationRuleOnNamespace");
        }
         
        #endregion
    }
}