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
    public class GetWAPackVMRoleTests : CmdletTestCloudServiceBase
    {
        public const string cmdletName = "Get-WAPackVMRole";

        [TestInitialize]
        public void TestInitialize()
        {
            // Remove any existing VMRoles/CloudService
            this.VMRolePreTestCleanup();
            this.CloudServicePreTestCleanup();

            // Create a QuickCreateVMRole
            this.CreateVMRoleFromQuickCreate();
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackVMRolesWithNoParam()
        {
            var allVMRoles = this.InvokeCmdlet(cmdletName, null);
            Assert.IsTrue(allVMRoles.Count > 0);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackVMRoleFromName()
        {
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMRoleNameFromQuickCreate}
            };
            var vmRole = this.InvokeCmdlet(cmdletName, inputParams);
            Assert.AreEqual(1, vmRole.Count, string.Format("{0} VMRole Found, {1} VMRole Was Expected.", vmRole.Count, 1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackVMRoleFromCloudServiceName()
        {
            // VMRoleNameToCreate is used for both paramter since test VMRole is created using the WAP
            // way (CloudServiceName and VMRole name are identical).
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMRoleNameFromQuickCreate},
                {"CloudServiceName", this.VMRoleNameFromQuickCreate}
            };
            var vmRole = this.InvokeCmdlet(cmdletName, inputParams);
            Assert.AreEqual(1, vmRole.Count, string.Format("{0} VMRole Found, {1} VMRole Was Expected.", vmRole.Count, 1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackVMRoleFromExistingCloudServiceName()
        {
            this.CreateVMRoleFromCloudService();

            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMRoleNameFromCloudService},
                {"CloudServiceName", this.CloudServiceName}
            };
            var vmRole = this.InvokeCmdlet(cmdletName, inputParams);
            Assert.AreEqual(1, vmRole.Count, string.Format("{0} VMRole Found, {1} VMRole Was Expected.", vmRole.Count, 1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Negative")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackVMRoleFromNameDoesNotExist()
        {
            string expectedVMRoleName = "WAPackVMRoleDoesNotExist";
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", expectedVMRoleName},
            };
            var vmRole = this.InvokeCmdlet(cmdletName, inputParams, NonExistantResourceExceptionMessage);
            Assert.AreEqual(0, vmRole.Count);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Negative")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void GetWAPackVMRoleFromCloudServiceNameDoesNotExist()
        {
            string expectedVMRoleCloudServiceName = "WAPackVMRoleCloudServiceNameDoesNotExist";
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMRoleNameFromQuickCreate},
                {"CloudServiceName", expectedVMRoleCloudServiceName}
            };
            var vmRole = this.InvokeCmdlet(cmdletName, inputParams, NonExistantResourceExceptionMessage);
            Assert.AreEqual(0, vmRole.Count);
        }

        [TestCleanup]
        public void VMRoleCleanup()
        {
            this.RemoveVMRoles();
            this.RemoveCloudServices();
        }
    }
}
