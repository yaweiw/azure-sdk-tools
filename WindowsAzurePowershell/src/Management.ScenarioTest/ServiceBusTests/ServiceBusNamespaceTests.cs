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
            : base("Microsoft.WindowsAzure.Management.ServiceBus.dll",
                   "Microsoft.WindowsAzure.Management.CloudService.dll",
                   "Assert.ps1",
                   "ServiceBus\\Common.ps1",
                   "ServiceBus\\NamespaceScenarioTests.ps1")
        {

        }

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
    }
}