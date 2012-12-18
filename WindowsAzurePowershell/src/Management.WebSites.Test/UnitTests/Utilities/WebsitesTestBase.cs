using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Management.Services;
using System.IO;
using Microsoft.WindowsAzure.Management.Test.Stubs;

namespace Microsoft.WindowsAzure.Management.Websites.Test.UnitTests.Utilities
{
    [TestClass]
    public class WebsitesTestBase
    {
        protected string subscriptionName = "foo";

        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.AzureAppDir = Path.Combine(Directory.GetCurrentDirectory(), "Windows Azure Powershell");
            Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            GlobalPathInfo.AzureAppDir = Path.Combine(Directory.GetCurrentDirectory(), "Windows Azure Powershell");

            string webSpacesFile = Path.Combine(GlobalPathInfo.AzureAppDir,
                                                          string.Format("spaces.{0}.json", subscriptionName));

            string sitesFile = Path.Combine(GlobalPathInfo.AzureAppDir,
                                                          string.Format("sites.{0}.json", subscriptionName));

            if (File.Exists(webSpacesFile))
            {
                File.Delete(webSpacesFile);
            }

            if (File.Exists(sitesFile))
            {
                File.Delete(sitesFile);
            }
        }
    }
}
