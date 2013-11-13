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

namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Test.WebClient
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.WebClient;
    using Microsoft.WindowsAzure.Commands.WAPackIaaS.Test.Mocks;
    using System;
    using System.Collections.Generic;
    using System.Net;

    [TestClass]
    public class WAPackWebClientTests
    {
        private MockRequestChannel channel;

        private WAPackIaaSClient client;

        private WebHeaderCollection responseHeaders;

        [TestInitialize]
        public void Initialize()
        {
            this.channel = MockRequestChannel.Create();

            var subscription = new Subscription
            {
                ServiceEndpoint = new Uri("http://localhost:8090/"),
                SubscriptionId = Guid.NewGuid().ToString(),
            };
            
            this.client = new WAPackIaaSClient(subscription, channel);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void ExpectLanguageAndContentShouldBeSetToJson()
        {
            this.channel.AddExpectedValue("Accept", "application/json");
            this.channel.AddExpectedValue("ContentType", "application/json");
            this.client.Get<string>(out responseHeaders);
        }

        #region Test request.Method
        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void MethodShouldBeSetToGetOnGet()
        {
            this.channel.AddExpectedValue("Method", "GET");
            this.client.Get<string>(out responseHeaders);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void MethodShouldBeSetToPostOnCreate()
        {
            this.channel.AddExpectedValue("Method", "POST");
            this.client.Create<string>(null, out responseHeaders);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void MethodShouldBeSetToDeleteOnDelete()
        {
            this.channel.AddExpectedValue("Method", "DELETE");
            this.client.Delete<string>(out responseHeaders);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void MethodShouldBeSetToPutOnUpdate()
        {
            this.channel.AddExpectedValue("Method", "PUT");
            this.client.Update<string>(null, out responseHeaders);
        }
        #endregion

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void PreferContentShouldBeSetOnUpdate()
        {
            this.channel.AddExpectedHeader("Prefer", "return-content");
            this.client.Update<string>(null, out responseHeaders);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void CanAddCustomerHeaders()
        {
            var customHeaders = new Dictionary<string, string>();
            customHeaders.Add("Header1", "value1");
            customHeaders.Add("Header2", "value2");

            foreach (var header in customHeaders)
            {
                this.client.AddHeaders(header.Key, header.Value);
                this.channel.AddExpectedHeader(header.Key, header.Value);
            }
        }
    }
}
