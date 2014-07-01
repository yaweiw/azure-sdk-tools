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
    public class NewWAPackStaticIPAddressPoolTests : CmdletTestNetworkingBase
    {
        public const string cmdletName = "New-WAPackStaticIPAddressPool";

        [TestInitialize]
        public void TestInitialize()
        {
            // Remove any existing VNet
            this.NetworkingPreTestCleanup();

            // Create a VNnet/VMSubnet
            this.CreateVNet();
            this.CreateVMSubnet();
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Functional")]
        [TestCategory("WAPackIaaS-Networking")]
        public void NewWAPackStaticIPAddressPoolDefault()
        {
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", this.StaticIPAddressPoolName},
                {"VMSubnet", this.CreatedVMSubnet.First()},
                {"IPAddressRangeStart", this.IPAddressRangeStart},
                {"IPAddressRangeEnd", this.IPAddressRangeEnd}
            };
            var createdStaticIPAddressPool = this.InvokeCmdlet(NewStaticIPAddressPoolCmdletName, inputParams, null);
            Assert.AreEqual(1, createdStaticIPAddressPool.Count, string.Format("{0} StaticIPAddressPool Found, {1} StaticIPAddressPool Was Expected.", createdStaticIPAddressPool.Count, 1));
            CreatedStaticIPAddressPool.AddRange(createdStaticIPAddressPool);

            var readStaticIPAddressPoolName = createdStaticIPAddressPool.First().Properties["Name"].Value;
            Assert.AreEqual(this.StaticIPAddressPoolName, readStaticIPAddressPoolName, string.Format("Actual StaticIPAddressPool Name - {0}, Expected StaticIPAddressPool Name - {1}", readStaticIPAddressPoolName, this.StaticIPAddressPoolName));

            var readStaticIPAddressPoolIPAddressRangeStart = createdStaticIPAddressPool.First().Properties["IPAddressRangeStart"].Value;
            Assert.AreEqual(this.IPAddressRangeStart, readStaticIPAddressPoolIPAddressRangeStart, string.Format("Actual StaticIPAddressPool IPAddressRangeStart - {0}, Expected StaticIPAddressPool IPAddressRangeStart - {1}", readStaticIPAddressPoolIPAddressRangeStart, this.IPAddressRangeStart));

            var readStaticIPAddressPoolIPAddressRangeEnd = createdStaticIPAddressPool.First().Properties["IPAddressRangeEnd"].Value;
            Assert.AreEqual(this.IPAddressRangeEnd, readStaticIPAddressPoolIPAddressRangeEnd, string.Format("Actual StaticIPAddressPool IPAddressRangeEnd - {0}, Expected StaticIPAddressPool IPAddressRangeEnd - {1}", readStaticIPAddressPoolIPAddressRangeEnd, this.IPAddressRangeEnd));
        }

        [TestCleanup]
        public void StaticIPAddressPoolCleanup()
        {
            this.RemoveVNet();
        }
    }
}
