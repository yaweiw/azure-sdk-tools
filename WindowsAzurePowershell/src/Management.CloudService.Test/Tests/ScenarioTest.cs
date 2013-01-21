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

namespace Microsoft.WindowsAzure.Management.CloudService.Test.Tests
{
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ScenarioTest : PowerShellTest
    {
        public ScenarioTest() 
            : base("Microsoft.WindowsAzure.Management.CloudService.dll")
        {

        }

        [TestCategory("Functional"), TestMethod]
        public void ComplexCachingTest()
        {
            string cloudService = "OneSDKCloudServiceTest";
            string expectedDeploymentName = "productionDeploy";
            string expectedDeploymentSlot = DeploymentSlotType.Production.ToString();
            Deployment deployment;

            powershell.Runspace.SessionStateProxy.SetVariable("cloudService", cloudService);
            AddScenarioScript("ComplexCachingTest.ps1");

            System.Collections.ObjectModel.Collection<System.Management.Automation.PSObject> res = powershell.Invoke();

            if (!powershell.Streams.Error.Count.Equals(0))
            {
                throw powershell.Streams.Error[0].Exception;
            }

            deployment = powershell.GetPowerShellVariable<Deployment>("deployment");
            Assert.AreEqual<string>(deployment.Name, expectedDeploymentName);
            Assert.AreEqual<string>(deployment.DeploymentSlot, expectedDeploymentSlot);
            Assert.AreEqual<string>(deployment.Status, DeploymentStatus.Running.ToString());
        }
    }
}
