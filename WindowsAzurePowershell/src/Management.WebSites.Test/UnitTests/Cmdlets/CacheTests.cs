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
    using System.IO;
    using Management.Services;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Services;
    using Websites.Services.WebEntities;

    [TestClass]
    public class CacheTests
    {
        public static string SubscriptionName = "fakename";

        public static string WebSpacesFile;

        public static string SitesFile;

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.AzureAppDir = Path.Combine(Directory.GetCurrentDirectory(), "Windows Azure Powershell");

            WebSpacesFile =  Path.Combine(GlobalPathInfo.AzureAppDir,
                                                          string.Format("spaces.{0}.json", SubscriptionName));

            SitesFile = Path.Combine(GlobalPathInfo.AzureAppDir,
                                                          string.Format("sites.{0}.json", SubscriptionName));
            
            if (File.Exists(WebSpacesFile))
            {
                File.Delete(WebSpacesFile);
            }

            if (File.Exists(SitesFile))
            {
                File.Delete(SitesFile);
            }
        }

        [TestMethod]
        public void GetSetWebSpacesTest()
        {
            // Test no webspaces
            Assert.IsNull(Cache.GetWebSpaces(SubscriptionName));

            // Test valid webspaces
            WebSpaces webSpaces = new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" }});
            Cache.SaveSpaces(SubscriptionName, webSpaces);

            WebSpaces getWebSpaces = Cache.GetWebSpaces(SubscriptionName);
            Assert.IsNotNull(getWebSpaces.Find(ws => ws.Name.Equals("webspace1")));
            Assert.IsNotNull(getWebSpaces.Find(ws => ws.Name.Equals("webspace2")));
        }

        [TestMethod]
        public void GetSetSitesTest()
        {
            Assert.IsNull(Cache.GetSites(SubscriptionName));

            Sites sites = new Sites(new List<Site> { new Site { Name = "site1" }, new Site { Name = "site2" }});
            Cache.SaveSites(SubscriptionName, sites);

            Sites getSites = Cache.GetSites(SubscriptionName);
            Assert.IsNotNull(getSites.Find(s => s.Name.Equals("site1")));
            Assert.IsNotNull(getSites.Find(s => s.Name.Equals("site2")));
        }
    }
}
