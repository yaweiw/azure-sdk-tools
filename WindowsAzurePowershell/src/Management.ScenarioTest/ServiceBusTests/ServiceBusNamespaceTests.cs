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
    using System.Text;
    using System;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using System.Management.Automation;
    using System.Collections.ObjectModel;
    using System.IO;

    [TestClass]
    public class ServiceBusNamespaceTests : PowerShellTest
    {
        const string RemoveNamespaceScript = "ServiceBus\\RemoveNamespace.ps1";

        const string GetNamespaceNameScript = "ServiceBus\\GetNamespaceName.ps1";

        const string ListAzureSBLocation1Script = "ServiceBus\\ListAzureSBLocation1.ps1";
        

        string createdNamespace;

        [TestInitialize]
        public void TestSetup()
        {
            createdNamespace = string.Empty;
        }

        [TestCleanup]
        public override void TestCleanup()
        {
            base.TestCleanup();
            base.CreatePowerShell();
            AddScenarioScript(RemoveNamespaceScript);
            powershell.SetVariable("name", createdNamespace);
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

            Assert.AreEqual<int>(expectedLocationsCount, actual.Count);
            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
        }

        /// <summary>
        /// Tests Get-AzureSBLocation cmdlet piped to New-AzureSBNamespace cmdlets.
        /// </summary>
        [TestMethod]
        public void ListAzureSBLocationPipeToNewAzureSBNamespace()
        {
            string location = "North Central US";
            AddScenarioScript(GetNamespaceNameScript);
            AddScenarioScript(ListAzureSBLocation1Script);
            powershell.SetVariable("location", location);
            
            powershell.Invoke();

            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
            ServiceBusNamespace serviceBusNamespace = powershell.GetPowerShellVariable<ServiceBusNamespace>("namespace");
            string name = powershell.GetPowerShellVariable<string>("name");
            Assert.AreEqual<string>(name, serviceBusNamespace.Name);
            Assert.AreEqual<string>(location, serviceBusNamespace.Region);
            Assert.IsTrue(serviceBusNamespace.Status.Equals("Activating") || serviceBusNamespace.Status.Equals("Active"));
            createdNamespace = name;
        }
    }
}