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

    [TestClass]
    public class GetWAPackCloudServiceTests : CmdletTestCloudServiceBase
    {
        public const string cmdletName = "Get-WAPackCloudService";

        [TestInitialize]
        public void TestInitialize()
        {
            // Remove any existing CloudService
            this.CloudServicePreTestCleanup();

            // Create a CloudService
            this.CreateCloudService();
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackCloudServiceWithNoParam()
        {
            var allCloudServices = this.InvokeCmdlet(cmdletName, null);
            Assert.IsTrue(allCloudServices.Count > 0);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackCloudServiceFromName()
        {
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.CloudServiceName}
            };
            var cloudService = this.InvokeCmdlet(cmdletName, inputParams);
            Assert.AreEqual(1, cloudService.Count, string.Format("{0} CloudService Found, {1} CloudService Was Expected.", cloudService.Count, 1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Negative")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackCloudServiceFromNameDoesNotExist()
        {
            string expectedCloudServiceName = "WAPackCloudServiceDoesNotExist";
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", expectedCloudServiceName}
            };
            var cloudService = this.InvokeCmdlet(cmdletName, inputParams, NonExistantResourceExceptionMessage);
            Assert.AreEqual(0, cloudService.Count, string.Format("{0} CloudService Found, {1} CloudService Was Expected.", cloudService.Count, 0));
        }

        [TestCleanup]
        public void CloudServiceCleanup()
        {
            this.RemoveCloudServices();
        }
    }
}
