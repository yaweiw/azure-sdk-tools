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
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;

    [TestClass]
    public class ServiceBusNamespaceTests : PowerShellTest
    {
        const string CommonScript = "ServiceBus\\Common.ps1";

        const string NamespaceScenarioTestsScript = "ServiceBus\\NamespaceScenarioTests.ps1";

        private void AddScriptWithCleanup(string script)
        {
            powershell.AddScript(script);
            powershell.AddScript("Test-Cleanup");
        }

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
            AddScenarioScript(CommonScript);
            AddScenarioScript(NamespaceScenarioTestsScript);
        }

        public ServiceBusNamespaceTests()
            : base("Microsoft.WindowsAzure.Management.ServiceBus.dll",
                   "Microsoft.WindowsAzure.Management.CloudService.dll")
        {

        }

        /// <summary>
        /// Test Get-AzureSBLocation with valid credentials.
        /// </summary>
        [TestMethod]
        public void ListAzureSBLocationWithValidCredentials()
        {
            powershell.AddScript("Get-AzureSBLocation");
            int expectedLocationsCount = 8;
            List<ServiceBusRegion> actual = new List<ServiceBusRegion>();

            powershell.Invoke(null, actual);

            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
            Assert.AreEqual<int>(expectedLocationsCount, actual.Count);
        }

        /// <summary>
        /// Tests using List-AzureSBLocation and piping it's output to New-AzureSBNamespace.
        /// </summary>
        [TestMethod]
        public void TestListAzureSBLocation1()
        {
            AddScriptWithCleanup("Test-ListAzureSBLocation1");

            powershell.Invoke();

            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
        }

        /// <summary>
        /// Tests using Get-AzureSBNamespace cmdlet and expect to return empty collection
        /// </summary>
        [TestMethod]
        public void TestGetAzureSBNamespaceWithEmptyNamespaces()
        {
            powershell.AddScript("$namespaces = Get-AzureSBNamespace");

            powershell.Invoke();

            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
            List<ServiceBusNamespace> namespaces = powershell.GetPowerShellCollection<ServiceBusNamespace>("namespaces");
            Assert.IsTrue(namespaces.Count.Equals(0));
        }

        [TestMethod]
        public void TestGetAzureSBNamespace2()
        {
            AddScriptWithCleanup("New-Namespace 1; $namespaces = Get-AzureSBNamespace");

            powershell.Invoke();

            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
            List<ServiceBusNamespace> namespaces = powershell.GetPowerShellCollection<ServiceBusNamespace>("namespaces");
            Assert.IsTrue(namespaces.Count.Equals(1));
        }

        [TestMethod]
        public void TestGetAzureSBNamespace3()
        {

        }

        [TestMethod]
        public void TestGetAzureSBNamespace4()
        {

        }

        [TestMethod]
        public void TestGetAzureSBNamespace5()
        {

        }
    }
}