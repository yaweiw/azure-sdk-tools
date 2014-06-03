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

namespace Microsoft.WindowsAzure.Commands.Test.Websites
{
    using Commands.Utilities.Common;
    using Commands.Utilities.Websites;
    using Commands.Websites;
    using Moq;
    using Utilities.Common;
    using Utilities.Websites;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RestartAzureWebsiteTests : WebsitesTestBase
    {
        [TestMethod]
        public void ProcessRestartWebsiteTest()
        {
            // Setup
            const string websiteName = "website1";
            Mock<IWebsitesClient> websitesClientMock = new Mock<IWebsitesClient>();
            websitesClientMock.Setup(f => f.RestartWebsite(websiteName, null));

            // Test
            RestartAzureWebsiteCommand restartAzureWebsiteCommand = new RestartAzureWebsiteCommand()
            {
                CommandRuntime = new MockCommandRuntime(),
                Name = websiteName,
                CurrentSubscription = new WindowsAzureSubscription { SubscriptionId = base.subscriptionId },
                WebsitesClient = websitesClientMock.Object
            };

            restartAzureWebsiteCommand.ExecuteCmdlet();

            websitesClientMock.Verify(f => f.RestartWebsite(websiteName, null), Times.Once());
        }

        [TestMethod]
        public void RestartsWebsiteSlot()
        {
            // Setup
            const string websiteName = "website1";
            const string slot = "staging";

            Mock<IWebsitesClient> websitesClientMock = new Mock<IWebsitesClient>();
            websitesClientMock.Setup(f => f.RestartWebsite(websiteName, slot));

            // Test
            RestartAzureWebsiteCommand restartAzureWebsiteCommand = new RestartAzureWebsiteCommand()
            {
                CommandRuntime = new MockCommandRuntime(),
                Name = websiteName,
                CurrentSubscription = new WindowsAzureSubscription { SubscriptionId = base.subscriptionId },
                WebsitesClient = websitesClientMock.Object,
                Slot = slot
            };

            restartAzureWebsiteCommand.ExecuteCmdlet();

            websitesClientMock.Verify(f => f.RestartWebsite(websiteName, slot), Times.Once());
        }
    }
}
