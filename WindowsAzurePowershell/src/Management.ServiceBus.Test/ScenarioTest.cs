// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests
{
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.CloudService.Model;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using Microsoft.WindowsAzure.Management.ServiceBus.Properties;

    [TestClass]
    public class ScenarioTest : PowerShellTest
    {
        public ScenarioTest() 
            : base("Microsoft.WindowsAzure.Management.ServiceBus.dll",
                   "Microsoft.WindowsAzure.Management.CloudService.dll"
            )
        {

        }

        [TestCategory("Scenario"), TestMethod]
        public void CreateAndGetServiceBusNamespace()
        {
            string serviceBusNamespaceName = "CreateAndGetServiceBusNamespace";
            int locationIndex = 5; // North Central US
            string expectedRemoveVerbose = string.Format(Resources.RemovingNamespaceMessage, serviceBusNamespaceName);

            powershell.Runspace.SessionStateProxy.SetVariable("name", serviceBusNamespaceName);
            powershell.Runspace.SessionStateProxy.SetVariable("index", locationIndex);
            AddScenarioScript("CreateAndGetServiceBusNamespace.ps1");

            Collection<PSObject> result = powershell.Invoke();

            Assert.AreEqual<string>(serviceBusNamespaceName, Testing.GetPSVariableValue<string>(result[0], "Name"));
            Assert.AreEqual<string>(expectedRemoveVerbose, powershell.Streams.Verbose[0].Message);
            Assert.IsTrue(powershell.Streams.Error.Count.Equals(0));
        }
    }
}
