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

    [TestClass]
    public class ScenarioTests : PowerShellTest
    {
        public ScenarioTests() 
            : base("Microsoft.WindowsAzure.Management.CloudService.dll")
        {

        }

        [TestMethod]
        public void DeployCloudServiceUsingRemoteDesktopAndSSL()
        {
            string name = "test";
            powershell.Runspace.SessionStateProxy.SetVariable("name", name);
            AddScenarioScript("DeployCloudServiceUsingRemoteDesktopAndSSL.ps1");

            Collection<PSObject> result = powershell.Invoke();

            Assert.AreEqual<string>(name, Testing.GetPSVariableValue<string>(result[0], Parameters.ServiceName));
            Assert.IsTrue(powershell.Streams.Verbose.Count.Equals(1));
        }
    }
}
