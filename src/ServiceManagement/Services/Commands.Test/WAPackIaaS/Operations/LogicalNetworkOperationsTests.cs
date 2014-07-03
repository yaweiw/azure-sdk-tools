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

namespace Microsoft.WindowsAzure.Commands.Test.WAPackIaaS.Operations
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Test.WAPackIaaS.Mocks;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.DataContract;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.Operations;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.WebClient;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [TestClass]
    public class LogicalNetworkOperationsTests
    {
        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnOneLogicalNetwork()
        {
            var mockChannel = new MockRequestChannel();
            mockChannel.AddReturnObject(new LogicalNetwork { ID = Guid.Empty, CloudId = Guid.Empty, Name = "LogicalNetwork01" });

            var logicalNetworkOperations = new LogicalNetworkOperations(new WebClientFactory(new Subscription(), mockChannel));
            Assert.AreEqual(1, logicalNetworkOperations.Read().Count);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnOneLogicalNetworkByName()
        {
            const string expectedLogicalNetworkName = "LogicalNetwork01";

            var mockChannel = new MockRequestChannel();
            mockChannel.AddReturnObject(new LogicalNetwork { ID = Guid.Empty, CloudId = Guid.Empty, Name = expectedLogicalNetworkName });

            var logicalNetworkOperations = new LogicalNetworkOperations(new WebClientFactory(new Subscription(), mockChannel));
            var logicalNetworkList = logicalNetworkOperations.Read(expectedLogicalNetworkName);

            Assert.AreEqual(1, logicalNetworkList.Count);
            Assert.AreEqual(expectedLogicalNetworkName, logicalNetworkList.First().Name);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnMultipleLogicalNetworks()
        {
            const string expectedLogicalNetworkName = "LogicalNetwork01";

            var mockChannel = new MockRequestChannel();
            var logicalNetworks = new List<object>
            {
                new LogicalNetwork { ID = Guid.Empty, CloudId = Guid.Empty, Name = expectedLogicalNetworkName },
                new LogicalNetwork { ID = Guid.Empty, CloudId = Guid.Empty, Name = expectedLogicalNetworkName }
            };
            mockChannel.AddReturnObject(logicalNetworks);

            var logicalNetworkOperations = new LogicalNetworkOperations(new WebClientFactory(new Subscription(), mockChannel));
            var logicalNetworkList = logicalNetworkOperations.Read();

            Assert.AreEqual(2, logicalNetworkList.Count);
            Assert.IsTrue(logicalNetworkList.All(logicalNetwork => logicalNetwork.Name == expectedLogicalNetworkName));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        [TestCategory("WAPackIaaS-Negative")]
        public void ShouldReturnEmptyOnNoResult()
        {
            var logicalNetworkOperations = new LogicalNetworkOperations(new WebClientFactory(new Subscription(), MockRequestChannel.Create()));
            Assert.IsFalse(logicalNetworkOperations.Read().Any());
        }
    }
}
