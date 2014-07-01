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
    public class NewWAPackVMSubnetTests : CmdletTestNetworkingBase
    {
        public const string cmdletName = "New-WAPackVMSubnet";

        [TestInitialize]
        public void TestInitialize()
        {
            // Remove any existing VNet
            this.NetworkingPreTestCleanup();

            // Create a VNet
            this.CreateVNet();
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-Networking")]
        public void NewWAPackVMSubnetDefault()
        {
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VMSubnetName},
                {"VNet", this.CreatedVNet.First()},
                {"Subnet", this.Subnet}
            };
            var createdSubnet = this.InvokeCmdlet(NewVMSubnetCmdletName, inputParams, null);
            Assert.AreEqual(1, createdSubnet.Count, string.Format("{0} VMSubnet Found, {1} VMSubnet Was Expected.", createdSubnet.Count, 1));
            CreatedVMSubnet.AddRange(createdSubnet);

            var readVMSubnetName = createdSubnet.First().Properties["Name"].Value;
            Assert.AreEqual(this.VMSubnetName, readVMSubnetName, string.Format("Actual VMSubnet Name - {0}, Expected VMSubnet Name - {1}", readVMSubnetName, this.VMSubnetName));

            var readVMSubnetSubnet = createdSubnet.First().Properties["Subnet"].Value;
            Assert.AreEqual(this.Subnet, readVMSubnetSubnet, string.Format("Actual VMSubnet Subnet - {0}, Expected VMSubnet Subnet - {1}", readVMSubnetSubnet, this.Subnet));
        
        }

        [TestCleanup]
        public void VMSubnetCleanup()
        {
            this.RemoveVNet();
        }
    }
}
