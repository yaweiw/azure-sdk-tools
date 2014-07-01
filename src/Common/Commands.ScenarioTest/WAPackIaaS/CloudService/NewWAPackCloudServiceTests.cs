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

namespace Microsoft.WindowsAzure.Commands.ScenarioTest.WAPackIaaS.FunctionalTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class NewWAPackCloudServiceTests : CmdletTestCloudServiceBase
    {
        public const string cmdletName = "New-WAPackCloudService";

        [TestInitialize]
        public void TestInitialize()
        {
            // Remove any existing CloudService
            this.CloudServicePreTestCleanup();
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void NewWAPackCloudServiceDefault()
        {
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.CloudServiceName},
                {"Label", this.CloudServiceLabel}
            };
            var createdCloudService = this.InvokeCmdlet(cmdletName, inputParams);
            Assert.AreEqual(1, createdCloudService.Count, string.Format("Actual CloudServices Found - {0}, Expected CloudServices - {1}", createdCloudService.Count, 1));

            var readCloudServiceName = createdCloudService.First().Properties["Name"].Value;
            Assert.AreEqual(this.CloudServiceName, readCloudServiceName, string.Format("Actual CloudService Name - {0}, Expected CloudService Name- {1}", readCloudServiceName, this.CloudServiceName));

            var readCloudServiceLabel = createdCloudService.First().Properties["Label"].Value;
            Assert.AreEqual(this.CloudServiceLabel, readCloudServiceLabel, string.Format("Actual CloudService Label - {0}, Expected CloudService Label- {1}", readCloudServiceLabel, this.CloudServiceLabel));

            var readCloudServiceProvisioningState = createdCloudService.First().Properties["ProvisioningState"].Value;
            Assert.AreEqual("Provisioned", readCloudServiceProvisioningState, string.Format("Actual CloudService Provisionning State - {0}, Expected CloudService Provisionning State - {1}", readCloudServiceProvisioningState, "Provisioned"));

            this.CreatedCloudServices.AddRange(createdCloudService);
        }

        [TestCleanup]
        public void CloudServiceCleanup()
        {
            this.RemoveCloudServices();
        }
    }
}
