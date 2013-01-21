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
    using System.Collections.Generic;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;

    [TestClass]
    public class ServiceBusNamespaceTests : PowerShellTest
    {
        public ServiceBusNamespaceTests()
            : base("ServiceBus\\Common.ps1",
                   "ServiceBus\\NamespaceScenarioTests.ps1")
        {

        }

        #region Get-AzureSBLocation Scenario Tests

        /// <summary>
        /// Test Get-AzureSBLocation with valid credentials.
        /// </summary>
        [TestMethod]
        public void ListAzureSBLocationWithValidCredentials()
        {
            RunPowerShellTest("Test-ListAzureSBLocation");
        }

        /// <summary>
        /// Tests using List-AzureSBLocation and piping it's output to New-AzureSBNamespace.
        /// </summary>
        [TestMethod]
        public void TestListAzureSBLocation1()
        {
            RunPowerShellTest("Test-ListAzureSBLocation1");
        }

        #endregion

        #region Get-AzureSBNamespace Scenario Tests

        /// <summary>
        /// Tests using Get-AzureSBNamespace cmdlet and expect to return empty collection
        /// </summary>
        [TestMethod]
        public void TestGetAzureSBNamespaceWithEmptyNamespaces()
        {
            RunPowerShellTest("Test-GetAzureSBNamespaceWithEmptyNamespaces");
        }

        [TestMethod]
        public void TestGetAzureSBNamespaceWithOneNamespace()
        {
            RunPowerShellTest("Test-GetAzureSBNamespaceWithOneNamespace");
        }

        [TestMethod]
        public void TestGetAzureSBNamespaceWithMultipleNamespaces()
        {
            RunPowerShellTest("Test-GetAzureSBNamespaceWithMultipleNamespaces");
        }

        [TestMethod]
        public void TestGetAzureSBNamespaceWithValidExisitingNamespace()
        {
            RunPowerShellTest("Test-GetAzureSBNamespaceWithValidExisitingNamespace");
        }

        [TestMethod]
        public void TestGetAzureSBNamespaceWithValidNonExisitingNamespace()
        {
            RunPowerShellTest("Test-GetAzureSBNamespaceWithValidNonExisitingNamespace");
        }

        [TestMethod]
        public void TestGetAzureSBNamespacePipedToRemoveAzureSBNamespace()
        {
            RunPowerShellTest("Test-GetAzureSBNamespacePipedToRemoveAzureSBNamespace");
        }

        [TestMethod]
        public void TestGetAzureSBNamespaceWithWebsites()
        {
            RunPowerShellTest("Test-GetAzureSBNamespaceWithWebsites");
        }

        #endregion

        #region New-AzureSBNamespace Scenario Tests

        [TestMethod]
        public void TestNewAzureSBNamespaceWithValidNewNamespace()
        {
            RunPowerShellTest("Test-NewAzureSBNamespaceWithValidNewNamespace");
        }

        [TestMethod]
        public void TestNewAzureSBNamespaceWithValidExistingNamespace()
        {
            RunPowerShellTest("Test-NewAzureSBNamespaceWithValidExistingNamespace");
        }

        [TestMethod]
        public void TestNewAzureSBNamespaceWithInvalidLocation()
        {
            RunPowerShellTest("Test-NewAzureSBNamespaceWithInvalidLocation");
        }

        /// <summary>
        /// This scenario test does the following:
        /// * Generates new name.
        /// * Uses Test-AzureName to make sure name is available
        /// * Runs Get-AzureSBLocation and pick default location object.
        /// * Creates new namespace.
        /// * Waits until it's status is Active
        /// * Setup website environment variable using Set-AzureWebsite -AppSettings
        /// </summary>
        [TestMethod]
        public void TestNewAzureSBNamespaceWithWebsite()
        {
            RunPowerShellTest("Test-NewAzureSBNamespaceWithWebsite");
        }

        #endregion
    }
}