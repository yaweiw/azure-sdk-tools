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
    using System.Management.Automation;

    public class CmdletTestNetworkingBase : CmdletTestBase
    {
        // Cmdlets definition
        protected const string GetLogicalNetworkCmdletName = "Get-WAPackLogicalNetwork";

        protected const string GetVNetCmdletName = "Get-WAPackVNet";

        protected const string NewVNetCmdletName = "New-WAPackVNet";

        protected const string NewVMSubnetCmdletName = "New-WAPackVMSubnet";

        protected const string NewStaticIPAddressPoolCmdletName = "New-WAPackStaticIPAddressPool";

        protected const string RemoveVNetCmdletName = "Remove-WAPackVNet";

        // Network properties
        protected string StaticIPAddressPoolName = "TestStaticIPAddressPoolForNetworkingTests";

        protected string VMSubnetName = "TestVMSubnetForNetworkingTests";

        protected string VNetName = "TestVNetForNetworkingTests";

        protected string VNetDescription = "Description - TestVNetForNetworkingTests";

        protected string Subnet = "192.168.1.0/24";

        protected string IPAddressRangeStart = "192.168.1.2";

        protected string IPAddressRangeEnd = "192.168.1.10";

        protected List<PSObject> CreatedVNet;

        protected List<PSObject> CreatedVMSubnet;

        protected List<PSObject> CreatedStaticIPAddressPool;

        // Error handling
        protected const string NonExistantResourceExceptionMessage = "The remote server returned an error: (404) Not Found.";

        protected const string AssertFailedNonExistantRessourceExceptionMessage = "Assert.IsFalse failed. " + NonExistantResourceExceptionMessage;

        protected CmdletTestNetworkingBase()
        {
            CreatedVNet = new List<PSObject>();
            CreatedVMSubnet = new List<PSObject>();
            CreatedStaticIPAddressPool = new List<PSObject>();
        }
        protected void CreateFullVNet()
        {
            this.CreateVNet();
            this.CreateVMSubnet();
            this.CreateStaticIPAddressPool();
        }

        protected void CreateVNet()
        {
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", WAPackConfigurationFactory.AvezLogicalNetworkName}
            };
            var existingLogicalNetwork = this.InvokeCmdlet(GetLogicalNetworkCmdletName, inputParams, null);
            Assert.AreEqual(1, existingLogicalNetwork.Count, string.Format("{0} LogicalNetwork Found, {1} LogicalNetwork Was Expected.", existingLogicalNetwork.Count, 1));

            inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VNetName},
                {"Description", this.VNetDescription},
                {"LogicalNetwork", existingLogicalNetwork.First()}
            };
            var createdVNet = this.InvokeCmdlet(NewVNetCmdletName, inputParams, null);
            Assert.AreEqual(1, createdVNet.Count, string.Format("{0} VNet Found, {1} VNet Was Expected.", createdVNet.Count, 1));
            CreatedVNet.AddRange(createdVNet);
        }

        protected void CreateVMSubnet()
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
        }

        protected void CreateStaticIPAddressPool()
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
        }

        protected void RemoveVNet()
        {
            // No need to remove individual components (StaticIPAddressPool, VMSubnet) since 
            // Remove-WAPackVNet will revove all components before removing the VNet.
            foreach (var vNet in this.CreatedVNet)
            {
                var inputParams = new Dictionary<string, object>()
                {
                    {"VNet", vNet},
                    {"Force", null},
                    {"PassThru", null}
                };
                var isDeleted = this.InvokeCmdlet(RemoveVNetCmdletName, inputParams);
                Assert.AreEqual(1, isDeleted.Count);
                Assert.AreEqual(true, isDeleted.First());

                inputParams = new Dictionary<string, object>()
                {
                    {"Name", this.VNetName}
                };
                var deletedVNet = this.InvokeCmdlet(GetVNetCmdletName, inputParams, null);
                Assert.AreEqual(0, deletedVNet.Count);
            }
            this.CreatedVNet.Clear();
        }

        protected void NetworkingPreTestCleanup()
        {
            Dictionary<string, object> inputParams = new Dictionary<string, object>()
            {
                {"Name", this.VNetName}
            };
            var existingVNet = this.InvokeCmdlet(GetVNetCmdletName, inputParams, null);

            if (existingVNet.Any())
            {
                this.CreatedVNet.AddRange(existingVNet);
                this.RemoveVNet();
            }
        }
    }
}
