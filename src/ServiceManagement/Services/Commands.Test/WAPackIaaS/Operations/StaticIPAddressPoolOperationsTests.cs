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
    using System.Net;
    using System.Net.Http;

    [TestClass]
    public class StaticIPAddressPoolOperationsTests
    {
        private const string baseURI = "/StaticIPAddressPools";

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void CreateStaticIPAddressPool()
        {
            const string ipAddressRangeStart = "192.168.1.2";
            const string ipAddressRangeEnd = "192.168.1.3";
            const string staticIPAddressPoolName = "StaticIPAddressPool01";

            var mockChannel = new MockRequestChannel();

            var staticIPAddressPoolToCreate = new StaticIPAddressPool()
            {
                Name = staticIPAddressPoolName,
                VMSubnetId = Guid.Empty,
                IPAddressRangeStart = ipAddressRangeStart,
                IPAddressRangeEnd = ipAddressRangeEnd,
                StampId = Guid.Empty
            };

            var staticIPAddressPoolToReturn = new StaticIPAddressPool()
            {
                Name = staticIPAddressPoolName,
                IPAddressRangeStart = ipAddressRangeStart,
                IPAddressRangeEnd = ipAddressRangeEnd,
                StampId = Guid.Empty
            };

            mockChannel.AddReturnObject(staticIPAddressPoolToReturn, new WebHeaderCollection { "x-ms-request-id:" + Guid.NewGuid() });

            Guid? jobOut;
            var staticIPAddressPoolOperations = new StaticIPAddressPoolOperations(new WebClientFactory(new Subscription(), mockChannel));
            var createdStaticIPAddressPool = staticIPAddressPoolOperations.Create(staticIPAddressPoolToCreate, out jobOut);

            Assert.IsNotNull(createdStaticIPAddressPool);
            Assert.IsInstanceOfType(createdStaticIPAddressPool, typeof(StaticIPAddressPool));
            Assert.AreEqual(staticIPAddressPoolToReturn.Name, createdStaticIPAddressPool.Name);
            Assert.AreEqual(staticIPAddressPoolToReturn.VMSubnetId, createdStaticIPAddressPool.VMSubnetId);
            Assert.AreEqual(staticIPAddressPoolToReturn.IPAddressRangeStart, createdStaticIPAddressPool.IPAddressRangeStart);
            Assert.AreEqual(staticIPAddressPoolToReturn.IPAddressRangeEnd, createdStaticIPAddressPool.IPAddressRangeEnd);
            Assert.AreEqual(staticIPAddressPoolToReturn.StampId, createdStaticIPAddressPool.StampId);

            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(1, requestList.Count);
            Assert.AreEqual(HttpMethod.Post.ToString(), requestList[0].Item1.Method);

            // Check the URI (for Azure consistency)
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void ShouldReturnOneStaticIPAddressPool()
        {
            const string ipAddressRangeStart = "192.168.1.2";
            const string ipAddressRangeEnd = "192.168.1.3";
            const string staticIPAddressPoolName = "StaticIPAddressPool01";

            var mockChannel = new MockRequestChannel();

            var staticIPAddressPoolToCreate = new StaticIPAddressPool()
            {
                Name = staticIPAddressPoolName,
                VMSubnetId = Guid.Empty,
                IPAddressRangeStart = ipAddressRangeStart,
                IPAddressRangeEnd = ipAddressRangeEnd,
                StampId = Guid.Empty
            };
            mockChannel.AddReturnObject(staticIPAddressPoolToCreate);

            var staticIPAddressPoolOperations = new StaticIPAddressPoolOperations(new WebClientFactory(new Subscription(), mockChannel));
            var readStaticIPAddressPool = staticIPAddressPoolOperations.Read(new VMSubnet(){ StampId = Guid.Empty, ID = Guid.Empty });
            Assert.AreEqual(1, readStaticIPAddressPool.Count);

            // Check the URI (for Azure consistency)
            var requestList = mockChannel.ClientRequests;
            Assert.AreEqual(1, requestList.Count);
            Assert.AreEqual(baseURI, mockChannel.ClientRequests[0].Item1.Address.AbsolutePath.Substring(1));
        }

        [TestMethod]
        [TestCategory("WAPackIaaS-All")]
        [TestCategory("WAPackIaaS-Unit")]
        public void DeleteStaticIPAddressPool()
        {
            const string ipAddressRangeStart = "192.168.1.2";
            const string ipAddressRangeEnd = "192.168.1.3";
            const string staticIPAddressPoolName = "StaticIPAddressPool01";

            var mockChannel = new MockRequestChannel();

            var existingStaticIPAddressPool = new StaticIPAddressPool()
            {
                Name = staticIPAddressPoolName,
                VMSubnetId = Guid.Empty,
                IPAddressRangeStart = ipAddressRangeStart,
                IPAddressRangeEnd = ipAddressRangeEnd,
                StampId = Guid.Empty
            };
            mockChannel.AddReturnObject(new Cloud() { StampId = Guid.NewGuid() });
            mockChannel.AddReturnObject(existingStaticIPAddressPool, new WebHeaderCollection { "x-ms-request-id:" + Guid.NewGuid() });

            Guid? jobOut;
            var staticIPAddressPoolOperations = new StaticIPAddressPoolOperations(new WebClientFactory(new Subscription(), mockChannel));
            staticIPAddressPoolOperations.Delete(Guid.Empty, out jobOut);

            Assert.AreEqual(2, mockChannel.ClientRequests.Count);
            Assert.AreEqual(HttpMethod.Delete.ToString(), mockChannel.ClientRequests[1].Item1.Method);

            // Check the URI (for Azure consistency)
            var requestList = mockChannel.ClientRequests;
            var requestURI = mockChannel.ClientRequests[1].Item1.Address.AbsolutePath;
            Assert.AreEqual(2, requestList.Count);
            Assert.AreEqual(baseURI, requestURI.Substring(1).Remove(requestURI.IndexOf('(') - 1));
        }
    }
}
