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

namespace Microsoft.WindowsAzure.Management.Websites.Test.UnitTests.Cmdlets
{
    using System.Collections.Generic;
    using System.Linq;
    using Management.Test.Stubs;
    using Management.Test.Tests.Utilities;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Cmdlets;
    using Websites.Services;
    using Websites.Services.WebEntities;

    [TestClass]
    public class ShowAzureWebsiteTests
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
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSitesThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1" } });
                }

                return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2" } });
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
                Name = "website1"
            };

            // Show existing website
            showAzureWebsiteCommand.ExecuteCommand();
            Assert.AreEqual(2, ((MockCommandRuntime)showAzureWebsiteCommand.CommandRuntime).WrittenObjects.Count);

            var website = ((MockCommandRuntime)showAzureWebsiteCommand.CommandRuntime).WrittenObjects[0] as Site;
            var websiteConfig = ((MockCommandRuntime)showAzureWebsiteCommand.CommandRuntime).WrittenObjects[1] as SiteConfig;
            Assert.IsNotNull(website);
            Assert.IsNotNull(websiteConfig);
            Assert.AreEqual("website1", website.Name);
            Assert.AreEqual("webspace1", website.WebSpace);
            Assert.AreEqual("user1", websiteConfig.PublishingUsername);
        }
    }
}
