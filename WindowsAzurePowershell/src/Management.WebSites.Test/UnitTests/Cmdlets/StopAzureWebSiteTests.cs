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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Cmdlets;
    using Websites.Services.WebEntities;

    [TestClass]
    public class StopAzureWebsiteTests : WebsitesTestBase
    {
        [TestMethod]
        public void ProcessStopWebsiteTest()
        {
            const string websiteName = "website1";
            const string webspaceName = "webspace";

            // Setup
            bool updated = true;
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = webspaceName } });
            channel.GetSitesThunk = ar => new Sites(new List<Site> { new Site { Name = websiteName, WebSpace = webspaceName } });

            channel.UpdateSiteThunk = ar =>
            {
                Assert.AreEqual(webspaceName, ar.Values["webspaceName"]);
                Site website = ar.Values["site"] as Site;
                Assert.IsNotNull(website);
                Assert.AreEqual(websiteName, website.Name);
                Assert.IsNotNull(website.HostNames.FirstOrDefault(hostname => hostname.Equals(websiteName + General.AzureWebsiteHostNameSuffix)));
                Assert.AreEqual(website.State, "Stopped");
                updated = true;
            };

            // Test
            StopAzureWebsiteCommand stopAzureWebsiteCommand = new StopAzureWebsiteCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime(),
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName }
            };

            stopAzureWebsiteCommand.ExecuteCmdlet();
            Assert.IsTrue(updated);
        }
    }
}
