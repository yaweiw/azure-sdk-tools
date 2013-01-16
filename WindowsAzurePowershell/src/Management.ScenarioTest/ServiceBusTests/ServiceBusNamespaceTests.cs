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
        

        string createdNamespace;

        [TestInitialize]
        public override void TestSetup()
        {
            base.TestSetup();
            createdNamespace = string.Empty;
            AddScenarioScript(CommonScript);
            AddScenarioScript(NamespaceScenarioTestsScript);
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            string name = createdNamespace;
            base.TestCleanup();
            TestSetup();
            powershell.AddScript(string.Format("Remove-Namespace {0}", name));

            powershell.Invoke();

            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
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
            powershell.AddScript("$namespace = Test-ListAzureSBLocation1");

            powershell.Invoke();

            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
            string location = powershell.GetPowerShellVariable<string>("location");
            ServiceBusNamespace serviceBusNamespace = powershell.GetPowerShellVariable<ServiceBusNamespace>("namespace");

            Assert.IsTrue(!string.IsNullOrEmpty(serviceBusNamespace.Name));
            Assert.AreEqual<string>(location, serviceBusNamespace.Region);
            Assert.IsTrue(serviceBusNamespace.Status.Equals("Activating") || serviceBusNamespace.Status.Equals("Active"));
            createdNamespace = serviceBusNamespace.Name;
        }
    }
}