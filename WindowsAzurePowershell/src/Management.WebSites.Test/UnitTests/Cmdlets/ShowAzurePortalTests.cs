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
    using System;
    using System.IO;
    using Management.Services;
    using Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Utilities;
    using Properties;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Cmdlets;

    [TestClass]
    public class ShowAzurePortalTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            GlobalPathInfo.AzureAppDir = Path.Combine(Directory.GetCurrentDirectory(), "Windows Azure Powershell");
            Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void ProcessGetAzurePublishSettingsTest()
        {
            new ShowAzurePortalCommand().ProcessShowAzurePortal(General.AzurePortalUrl, null);
        }

        /// <summary>
        /// Happy case, user has internet connection and uri specified is valid.
        /// </summary>
        [TestMethod]
        public void ProcessShowAzurePortalTestFail()
        {
            Assert.IsFalse(string.IsNullOrEmpty(General.AzurePortalUrl));
        }

        /// <summary>
        /// The url doesn't exist.
        /// </summary>
        [TestMethod]
        public void ProcessShowAzurePortalTestEmptyDnsFail()
        {
            string emptyDns = string.Empty;
            string expectedMsg = string.Format(Resources.InvalidOrEmptyArgumentMessage, "azure portal url");

            try
            {
                new ShowAzurePortalCommand().ProcessShowAzurePortal(emptyDns, null);
                Assert.Fail("No exception was thrown");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
                Assert.IsTrue(string.Compare(expectedMsg, ex.Message, StringComparison.OrdinalIgnoreCase) == 0);
            }
        }
    }
}