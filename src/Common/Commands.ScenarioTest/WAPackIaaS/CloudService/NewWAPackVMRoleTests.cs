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
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class NewWAPackVMRoleTests : CmdletTestCloudServiceBase
    {
        public const string cmdletName = "New-WAPackVMRole";

        [TestInitialize]
        public void TestInitialize()
        {
            // Remove any existing VMRoles/CloudService
            this.VMRolePreTestCleanup();
            this.CloudServicePreTestCleanup();
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void NewWAPackVMRoleFromQuickCreate()
        {
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMRoleNameFromQuickCreate},
                {"Label", this.VMRoleLabelToCreate},
                {"ResourceDefinition", GetBasicResDef()}
            };
            var createdVMRole = this.InvokeCmdlet(cmdletName, inputParams, null);

            Assert.AreEqual(1, createdVMRole.Count, string.Format("Actual VMRoles found - {0}, Expected VMRoles - {1}", createdVMRole.Count, 1));
            var createdVMRoleName = createdVMRole.First().Properties["Name"].Value;

            Assert.AreEqual(this.VMRoleNameFromQuickCreate, createdVMRoleName, string.Format("Actual VMRoles Name - {0}, Expected VMRoles Name- {1}", createdVMRoleName, this.VMRoleNameFromQuickCreate));
            this.CreatedVMRolesFromQuickCreate.AddRange(createdVMRole);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-CloudService")]
        public void NewWAPackVMRoleFromCloudService()
        {
            // Create a CloudService to place the VMRole on
            this.CreateCloudService();

            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMRoleNameFromCloudService},
                {"Label", this.VMRoleLabelToCreate},
                {"ResourceDefinition", GetBasicResDef()},
                {"CloudService", this.CreatedCloudServices.First()}
            };
            var actualCreatedVMRole = this.InvokeCmdlet(cmdletName, inputParams, null);

            Assert.AreEqual(1, actualCreatedVMRole.Count, string.Format("Actual VMRoles Found - {0}, Expected VMRoles - {1}", actualCreatedVMRole.Count, 1));
            var createdVMRoleName = actualCreatedVMRole.First().Properties["Name"].Value;

            Assert.AreEqual(this.VMRoleNameFromCloudService, createdVMRoleName, string.Format("Actual VMRoles Name - {0}, Expected VMRoles Name- {1}", createdVMRoleName, this.VMRoleNameFromCloudService));
            this.CreatedVMRolesFromCloudService.AddRange(actualCreatedVMRole);
        }

        [TestCleanup]
        public void VMRoleCleanup()
        {
            this.RemoveVMRoles();
            this.RemoveCloudServices();
        }
    }
}
