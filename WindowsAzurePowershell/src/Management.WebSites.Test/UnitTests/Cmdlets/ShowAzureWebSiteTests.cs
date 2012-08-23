// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.WebSites.Test.UnitTests.Cmdlets
{
    using System.Linq;
    using Management.Test.Stubs;
    using Management.Test.Tests.Utilities;
    using Services;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using WebSites.Cmdlets;

    [TestClass]
    public class ShowAzureWebSiteTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void ProcessShowWebsiteTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebspacesThunk = ar => new WebspaceList(new[] { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetWebsitesThunk = ar =>
            {
                if (ar.Values["webspace"].Equals("webspace1"))
                {
                    return new WebsiteList(new[] { new Website { Name = "website1", WebSpace = "webspace1" } });
                }

                return new WebsiteList(new[] { new Website { Name = "website2", WebSpace = "webspace2" } });
            };
            channel.GetWebsiteConfigurationThunk = ar =>
                                                       {
                                                           if (ar.Values["website"].Equals("website1") && ar.Values["webspace"].Equals("webspace1"))
                                                           {
                                                               return new WebsiteConfig
                                                               {
                                                                   PublishingUsername = "user1"
                                                               };
                                                           }

                                                           return null;
                                                       };

            // Test
            ShowAzureWebSiteCommand showAzureWebSiteCommand = new ShowAzureWebSiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                Name = "website1"
            };

            // Show existing website
            showAzureWebSiteCommand.ExecuteCommand();
            Assert.AreEqual(1, ((MockCommandRuntime)showAzureWebSiteCommand.CommandRuntime).WrittenObjects.Count);

            var website = ((MockCommandRuntime)showAzureWebSiteCommand.CommandRuntime).WrittenObjects.First() as WebsiteConfig;
            Assert.IsNotNull(website);
            Assert.AreEqual("website1", website.Name);
            Assert.AreEqual("webspace1", website.WebSpace);
            Assert.AreEqual("user1", website.PublishingUsername);
        }
    }
}
