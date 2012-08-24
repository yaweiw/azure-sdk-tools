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
    using Management.Test.Stubs;
    using Management.Test.Tests.Utilities;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Cmdlets;
    using Websites.Services;

    [TestClass]
    public class RemoveAzureWebsiteTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void ProcessRemoveWebsiteTest()
        {
            // Setup
            bool deletedWebsite = false;
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebspacesThunk = ar => new WebspaceList(new[] { new Webspace { Name = "webspace1" }, new Webspace { Name = "webspace2" } });
            channel.GetWebsitesThunk = ar =>
            {
                if (ar.Values["webspace"].Equals("webspace1"))
                {
                    return new WebsiteList(new[] { new Website { Name = "website1", WebSpace = "webspace1" } });
                }

                return new WebsiteList(new[] { new Website { Name = "website2", WebSpace = "webspace2" } });
            };

            channel.DeleteWebsiteThunk = ar =>
                                             {
                                                 if (ar.Values["website"].Equals("website1"))
                                                 {
                                                     deletedWebsite = true;
                                                 }
                                             };

            // Test
            RemoveAzureWebsiteCommand removeAzureWebsiteCommand = new RemoveAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                Name = "website1"
            };

            // Delete existing website
            removeAzureWebsiteCommand.ExecuteCommand();
            Assert.IsTrue(deletedWebsite);

            // Delete unexisting website
            deletedWebsite = false;

            removeAzureWebsiteCommand.Name = "website2";
            removeAzureWebsiteCommand.ExecuteCommand();
            Assert.IsFalse(deletedWebsite);
        }
    }
}