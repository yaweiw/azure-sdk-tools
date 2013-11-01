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

namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.FunctionalTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Utilities.Properties;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class GetWAPackVNetTests : CmdletTestBase
    {
        public const string cmdletName = "Get-WAPackVNet";

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void GetWAPackVNetWithNoParam()
        {
            var allVNetworks = this.InvokeCmdlet(cmdletName, null);
            Assert.IsTrue(allVNetworks.Any());
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void GetWAPackWAPackVNetFromName()
        {
            string expectedVNetworkName = WAPackConfigurationFactory.AvenzVnetName;
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", expectedVNetworkName}
            };
            var vNetworkFromName = this.InvokeCmdlet(cmdletName, inputParams);

            Assert.AreEqual(1, vNetworkFromName.Count);
            var actualvNetworkFromName = vNetworkFromName.First().Properties["Name"].Value;

            Assert.AreEqual(expectedVNetworkName, actualvNetworkFromName);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void GetWAPackWAPackVNetFromIdAndName()
        {
            string expectedVNetworkName = WAPackConfigurationFactory.AvenzVnetName;
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", expectedVNetworkName}
            };
            var vNetworkFromName = this.InvokeCmdlet(cmdletName, inputParams);

            Assert.AreEqual(1, vNetworkFromName.Count);
            var expectedvNetworkId = vNetworkFromName.First().Properties["Id"].Value;

            inputParams = new Dictionary<string, object>()
            {
                {"Id", expectedvNetworkId}
            };
            var vNetworkFromId = this.PowerShell.InvokeAndAssertForNoErrors();

            var actualvNetworkFromId = vNetworkFromId[0].Properties["Id"].Value;
            Assert.AreEqual(expectedvNetworkId, actualvNetworkFromId);
        }


        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        public void GetWAPackVNetByNameDoesNotExist()
        {
            string expectedVNetworkName = "WAPackWAPackVNetDoesNotExist";
            var inputParams = new Dictionary<string, object>()
            {
                {"Name", expectedVNetworkName}
            };
            var vNetworkFromName = this.InvokeCmdlet(cmdletName, inputParams);

            Assert.AreEqual(0, vNetworkFromName.Count);
        }

        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        public void GetWAPackVNetByIdDoesNotExist()
        {
            var expectedVmId = Guid.NewGuid().ToString();
            var expectedError = string.Format(Resources.ResourceNotFound, expectedVmId);
            var inputParams = new Dictionary<string, object>()
            {
                {"Id", expectedVmId}
            };
            var vmFromName = this.InvokeCmdlet(cmdletName, inputParams, expectedError);
            Assert.AreEqual(0, vmFromName.Count);
        }
    }
}
