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
    using System.Collections.Generic;
    using Commands.Utilities.Common;
    using Utilities.Common;
    using Utilities.Websites;
    using Commands.Utilities.Websites.Services.WebEntities;
    using Commands.Websites;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ShowAzureWebsiteTests : WebsitesTestBase
    {
        [TestMethod]
        public void ProcessShowWebsiteTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSitesThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1", HostNames = new [] {"website1.cloudapp.com" } } });
                }

                return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2", HostNames = new[] { "website2.cloudapp.com" } } });
            };
            channel.GetSiteConfigThunk = ar =>
            {
                if (ar.Values["name"].Equals("website1") && ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new SiteConfig
                    {
                        PublishingUsername = "user1"
                    };
                }

                return null;
            };

            // Test
            ShowAzureWebsiteCommand showAzureWebsiteCommand = new ShowAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                Name = "website1",
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionId }
            };

            // Show existing website
            showAzureWebsiteCommand.ExecuteCmdlet();
        }
    }
}