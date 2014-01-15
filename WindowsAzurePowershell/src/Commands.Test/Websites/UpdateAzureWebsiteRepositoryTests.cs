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
    public class UpdateAzureWebsiteRepositoryTests : WebsitesTestBase
    {
        [TestMethod]
        public void UpdatesRemote()
        {
            // Setup
            var mockClient = new Mock<IWebsitesClient>();
            string slot = WebsiteSlotName.Staging.ToString();
            SiteProperties props = new SiteProperties()
            {
                Properties = new List<NameValuePair>()
                {
                    new NameValuePair() { Name = "RepositoryUri", Value = "https://test@website.scm.azurewebsites.net:443/website.git" },
                    new NameValuePair() { Name = "PublishingUsername", Value = "test" }
                }
            };

            mockClient.Setup(c => c.GetWebsiteSlots("website1"))
                .Returns(
                new List<Site> { 
                    new Site { Name = "website1", WebSpace = "webspace1", SiteProperties = props },
                    new Site { Name = "website1(staging)", WebSpace = "webspace1", SiteProperties = props }
                });
            mockClient.Setup(c => c.GetSlotName("website1"))
                .Returns(WebsiteSlotName.Production.ToString())
                .Verifiable();
            mockClient.Setup(c => c.GetSlotName("website1(staging)"))
                .Returns(WebsiteSlotName.Staging.ToString())
                .Verifiable();

            // Test
            UpdateAzureWebsiteRepositoryCommand cmdlet = new UpdateAzureWebsiteRepositoryCommand
            {
                CommandRuntime = new MockCommandRuntime(),
                WebsitesClient = mockClient.Object,
                Name = "website1",
                CurrentSubscription = new WindowsAzureSubscription { SubscriptionId = base.subscriptionId },
            };

            // Switch existing website
            cmdlet.ExecuteCmdlet();
            mockClient.Verify(c => c.GetSlotName("website1(staging)"), Times.Once());
            mockClient.Verify(c => c.GetSlotName("website1"), Times.Once());
        }
    }
}