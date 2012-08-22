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
    using Services;
    using Management.Test.Stubs;
    using Management.Test.Tests.Utilities;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using WebSites.Cmdlets;

    [TestClass]
    public class StopAzureWebSiteTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void StopWebsiteProcessTest()
        {
            const string websiteName = "website1";
            const string webspaceName = "webspace";

            // Setup
            bool created = true;
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebspacesThunk = ar => new WebspaceList(new[] { new WebSpace { Name = webspaceName } });
            channel.GetWebsitesThunk = ar => new WebsiteList(new[] { new Website { Name = websiteName, WebSpace = webspaceName } });

            channel.UpdateWebsiteThunk = ar =>
            {
                Assert.AreEqual(webspaceName, ar.Values["webspace"]);
                UpdateWebsite website = ar.Values["website"] as UpdateWebsite;
                Assert.IsNotNull(website);
                Assert.AreEqual(websiteName, website.Name);
                Assert.IsNotNull(website.HostNames.FirstOrDefault(hostname => hostname.Equals(websiteName + ".azurewebsites.net")));
                Assert.AreEqual(website.State, "Stopped");
                created = true;
            };

            // Test
            StopAzureWebSiteCommand stopAzureWebSiteCommand = new StopAzureWebSiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime()
            };

            stopAzureWebSiteCommand.StopWebsiteProcess(websiteName);
            Assert.IsTrue(created);
        }
    }
}
