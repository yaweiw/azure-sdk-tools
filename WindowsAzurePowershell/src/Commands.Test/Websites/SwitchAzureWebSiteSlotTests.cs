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
    using Moq;
    using Utilities.Common;
    using Utilities.Websites;
    using Commands.Utilities.Websites.Services.WebEntities;
    using Commands.Websites;
    using VisualStudio.TestTools.UnitTesting;
    using System.Collections.Generic;

    [TestClass]
    public class SwitchAzureWebsiteSlotTests : WebsitesTestBase
    {
        [TestMethod]
        public void SwitchesSlots()
        {
            // Setup
            var mockClient = new Mock<IWebsitesClient>();
            string slot = "staging";

            mockClient.Setup(c => c.GetWebsiteSlots("website1"))
                .Returns(new List<Site> { 
                    new Site { Name = "website1", WebSpace = "webspace1" },
                    new Site { Name = "website1(staging)", WebSpace = "webspace1" }
                });
            mockClient.Setup(f => f.GetSlotName("website1")).Returns(WebsiteSlotName.Production.ToString());
            mockClient.Setup(f => f.GetSlotName("website1(staging)")).Returns("staging");
            mockClient.Setup(f => f.SwitchSlot("webspace1", "website1(staging)", slot)).Verifiable();
            mockClient.Setup(f => f.GetWebsiteNameFromFullName("website1")).Returns("website1");

            // Test
            SwitchAzureWebsiteSlotCommand switchAzureWebsiteCommand = new SwitchAzureWebsiteSlotCommand
            {
                CommandRuntime = new MockCommandRuntime(),
                WebsitesClient = mockClient.Object,
                Name = "website1",
                CurrentSubscription = new WindowsAzureSubscription { SubscriptionId = base.subscriptionId },
                Force = true
            };

            // Switch existing website
            switchAzureWebsiteCommand.ExecuteCmdlet();
            mockClient.Verify(c => c.SwitchSlot("webspace1", "website1", slot), Times.Once());
        }
    }
}