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
    using WebEntities;
    using Websites.Cmdlets;

    [TestClass]
    public class GetAzureWebsiteLocationTests
    {
        [TestInitialize]
        public void SetupTest()
        {
            Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void ProcessGetAzureWebsiteLocationTest()
        {
            // Setup
            SimpleWebsitesManagement channel = new SimpleWebsitesManagement();
            channel.GetWebSpacesThunk = ar => new WebSpaces(new List<WebSpace> { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });

            // Test
            GetAzureWebsiteLocationCommand getAzureWebsiteCommand = new GetAzureWebsiteLocationCommand(channel)
            {
                ShareChannel = true,
                CommandRuntime = new MockCommandRuntime()
            };

            getAzureWebsiteCommand.ExecuteCommand();
            Assert.AreEqual(2, ((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).WrittenObjects.Count);
            Assert.IsTrue(((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).WrittenObjects.Any(webspace => ((WebSpace)webspace).Name.Equals("webspace1")));
            Assert.IsTrue(((MockCommandRuntime)getAzureWebsiteCommand.CommandRuntime).WrittenObjects.Any(webspace => ((WebSpace)webspace).Name.Equals("webspace2")));
        }
    }
}
