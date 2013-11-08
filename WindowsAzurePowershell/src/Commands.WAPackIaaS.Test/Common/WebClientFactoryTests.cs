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

namespace Microsoft.WindowsAzure.Commands.WAPackIaaS.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Utilities;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS;
    using Microsoft.WindowsAzure.Commands.WAPackIaaS.Test.Mocks;
    using Microsoft.WindowsAzure.Commands.Utilities.WAPackIaaS.WebClient;
    using System;

    [TestClass]
    public class WebClientFactoryTests
    {
        [TestMethod]
        [TestCategory("Negative")]
        [TestCategory("WAPackIaaS")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowWithNullSubscription()
        {
            var factory = new WebClientFactory(null, null);
            factory.CreateClient(string.Empty);
        }

        [TestMethod]
        [TestCategory("WAPackIaaS")]
        public void ShouldCreateWAPackIaaSClient()
        {
            var factory = new WebClientFactory(new Subscription(), null);
            Assert.IsInstanceOfType(factory.CreateClient("a"), typeof(WAPackIaaSClient));
        }
    }
}
