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

namespace Microsoft.WindowsAzure.Management.Websites.Test.UnitTests.Cmdlets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Management.Services;
    using Management.Test.Tests.Utilities;
    using Microsoft.WindowsAzure.Management.CloudService.Test;
    using Microsoft.WindowsAzure.Management.Cmdlets;
    using Microsoft.WindowsAzure.Management.Websites.Properties;
    using Model;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Cmdlets;
    using Websites.Services.WebEntities;

    [TestClass]
    public class GetAzureWebsiteTests : WebsitesTestBase
    {
        [TestMethod]
        public void ProcessGetWebsiteTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSitesThunk = ar =>
                                           {
                                               if (ar.Values["webspaceName"].Equals("webspace1"))
                                               {
                                                   return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1" }});
                                               }

                                               return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2" } });
                                           };

            // Test
            GetAzureWebsiteCommand getAzureWebsiteCommand = new GetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName }
            };

            getAzureWebsiteCommand.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline.Count);
            var sites = (IEnumerable<Site>)((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.IsNotNull(sites);
            Assert.IsTrue(sites.Any(website => (website).Name.Equals("website1") && (website).WebSpace.Equals("webspace1")));
            Assert.IsTrue(sites.Any(website => (website).Name.Equals("website2") && (website).WebSpace.Equals("webspace2")));
        }

        [TestMethod]
        public void GetWebsiteProcessShowTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });
            channel.GetSiteThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Site { Name = "website1", WebSpace = "webspace1" };
                }

                return new Site { Name = "website2", WebSpace = "webspace2" };
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

            channel.GetSitesThunk = ar =>
            {
                if (ar.Values["webspaceName"].Equals("webspace1"))
                {
                    return new Sites(new List<Site> { new Site { Name = "website1", WebSpace = "webspace1" } });
                }

                return new Sites(new List<Site> { new Site { Name = "website2", WebSpace = "webspace2" } });
            };

            // Test
            GetAzureWebsiteCommand getAzureWebsiteCommand = new GetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName },
                Name = "website1"
            };

            getAzureWebsiteCommand.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).OutputPipeline.Count);

            var website = ((MockCommandRuntime) getAzureWebsiteCommand.CommandRuntime).OutputPipeline[0] as SiteWithConfig;
            Assert.IsNotNull(website);
            Assert.IsNotNull(website);
            Assert.AreEqual("website1", website.Name);
            Assert.AreEqual("webspace1", website.WebSpace);
            Assert.AreEqual("user1", website.PublishingUsername);
        }

        [TestMethod]
        public void ProcessGetWebsiteWithNullSubscription()
        {
            // Setup
            GlobalComponents globalComponents = GlobalComponents.CreateFromPublishSettings(
                GlobalPathInfo.GlobalSettingsDirectory,
                null,
                Microsoft.WindowsAzure.Management.CloudService.Test.TestData.Data.ValidPublishSettings[0]);
            RemoveAzureSubscriptionCommand removeCmdlet = new RemoveAzureSubscriptionCommand();
            removeCmdlet.CommandRuntime = new MockCommandRuntime();
            removeCmdlet.RemoveSubscriptionProcess("Windows Azure Sandbox 9-220", null);
            removeCmdlet.RemoveSubscriptionProcess("TestSubscription1", null);
            removeCmdlet.RemoveSubscriptionProcess("TestSubscription2", null);
            
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

            // Test
            GetAzureWebsiteCommand getAzureWebsiteCommand = new GetAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = null
            };

            Testing.AssertThrows<Exception>(() => getAzureWebsiteCommand.ExecuteCmdlet(), Resources.NoDefaultSubscriptionMessage);
        }
    }
}
