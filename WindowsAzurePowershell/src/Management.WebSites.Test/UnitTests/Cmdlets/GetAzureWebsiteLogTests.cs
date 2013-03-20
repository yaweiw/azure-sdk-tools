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
    using System.IO;
    using System.Management.Automation;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.WindowsAzure.Management.Websites.Services;
    using Microsoft.WindowsAzure.Management.Websites.Services.DeploymentEntities;
    using Microsoft.WindowsAzure.Management.Websites.Utilities;
    using Moq;
    using Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Cmdlets;
    using Websites.Services.WebEntities;
    using System.Linq;

    [TestClass]
    public class GetAzureWebsiteLogTests : WebsitesTestBase
    {
        private Mock<IWebsitesServiceManagement> websiteChannelMock;

        private Mock<IDeploymentServiceManagement> deploymentChannelMock;

        private Mock<ICommandRuntime> commandRuntimeMock;

        private Mock<RemoteLogStreamManager> remoteLogStreamManagerMock;

        private Mock<LogStreamWaitHandle> logStreamWaitHandleMock;

        private GetAzureWebsiteLogCommand getAzureWebsiteLogCmdlet;

        private string websiteName = "TestWebsiteName";

        private string repoUrl = "TheRepoUrl";

        private TaskCompletionSource<Stream> taskCompletionSource;

        private List<string> logs;

        private int logIndex;

        private Site website;

        [TestInitialize]
        public override void SetupTest()
        {
            base.SetupTest();

            websiteChannelMock = new Mock<IWebsitesServiceManagement>();
            deploymentChannelMock = new Mock<IDeploymentServiceManagement>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            taskCompletionSource = new TaskCompletionSource<Stream>();
            remoteLogStreamManagerMock = new Mock<RemoteLogStreamManager>();
            remoteLogStreamManagerMock.Setup(f => f.GetStream()).Returns(taskCompletionSource.Task);
            logs = new List<string>() { "Log1", "Error: Log2", "Log3", "Error: Log4", null };
            logIndex = 0;
            logStreamWaitHandleMock = new Mock<LogStreamWaitHandle>();
            logStreamWaitHandleMock.Setup(f => f.Dispose());
            logStreamWaitHandleMock.Setup(f => f.WaitNextLine(GetAzureWebsiteLogCommand.WaitInterval))
                .Returns(() => logs[logIndex++]);
            getAzureWebsiteLogCmdlet = new GetAzureWebsiteLogCommand(websiteChannelMock.Object, deploymentChannelMock.Object)
            {
                CommandRuntime = commandRuntimeMock.Object,
                RemoteLogStreamManager = remoteLogStreamManagerMock.Object,
                LogStreamWaitHandle = logStreamWaitHandleMock.Object,
                Name = websiteName,
                EndStreaming = (string line) => line != null,
                ShareChannel = true
            };
            website = new Site()
            {
                Name = websiteName,
                SiteProperties = new SiteProperties()
                {
                    Properties = new List<NameValuePair>()
                    {
                        new NameValuePair() { Name = UriElements.RepositoryUriProperty, Value = repoUrl }
                    }
                }
            };
            Cache.AddSite(getAzureWebsiteLogCmdlet.CurrentSubscription.SubscriptionId, website);
            websiteChannelMock.Setup(f => f.BeginGetSite(
                getAzureWebsiteLogCmdlet.CurrentSubscription.SubscriptionId,
                string.Empty,
                websiteName,
                "repositoryuri,publishingpassword,publishingusername",
                null,
                null))
                .Returns(It.IsAny<IAsyncResult>());
            websiteChannelMock.Setup(f => f.EndGetSite(It.IsAny<IAsyncResult>())).Returns(website);
        }

        [TestMethod]
        public void GetAzureWebsiteLogTest()
        {
            getAzureWebsiteLogCmdlet.Tail = true;

            getAzureWebsiteLogCmdlet.ExecuteCmdlet();

            logs.ForEach(l => commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>()), Times.AtLeastOnce()));
            logStreamWaitHandleMock.Verify(f => f.Dispose(), Times.Once());
        }

        [TestMethod]
        public void CanGetAzureWebsiteLogWithPath()
        {
            getAzureWebsiteLogCmdlet.Tail = true;
            getAzureWebsiteLogCmdlet.Path = "http";

            getAzureWebsiteLogCmdlet.ExecuteCmdlet();

            logs.ForEach(l => commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>()), Times.AtLeastOnce()));
            logStreamWaitHandleMock.Verify(f => f.Dispose(), Times.Once());
        }

        [TestMethod]
        public void TestGetAzureWebsiteLogUrlEncoding()
        {
            getAzureWebsiteLogCmdlet.Tail = true;
            string path = "my path";
            string message = "mes/a:q;";
            getAzureWebsiteLogCmdlet.Path = path;
            getAzureWebsiteLogCmdlet.Message = message;

            getAzureWebsiteLogCmdlet.ExecuteCmdlet();

            Assert.AreEqual<string>(HttpUtility.UrlEncode(path), getAzureWebsiteLogCmdlet.Path);
            Assert.AreEqual<string>(HttpUtility.UrlEncode(message), getAzureWebsiteLogCmdlet.Message);
            logs.ForEach(l => commandRuntimeMock.Verify(f => f.WriteObject(It.IsAny<object>()), Times.AtLeastOnce()));
            logStreamWaitHandleMock.Verify(f => f.Dispose(), Times.Once());
        }

        [TestMethod]
        public void TestGetAzureWebsiteLogListPath()
        {
            List<LogPath> paths = new List<LogPath>() { new LogPath() { Name = "http" }, new LogPath() { Name = "Git" } };
            List<string> expected = new List<string>() { "http", "Git" };
            List<string> actual = new List<string>();
            deploymentChannelMock.Setup(f => f.BeginListPaths(null, null));
            deploymentChannelMock.Setup(f => f.EndListPaths(It.IsAny<IAsyncResult>())).Returns(paths);
            commandRuntimeMock.Setup(f => f.WriteObject(It.IsAny<IEnumerable<string>>(), true))
                .Callback<object, bool>((o, b) => actual = actual = ((IEnumerable<string>)o).ToList<string>());
            getAzureWebsiteLogCmdlet.ListPath = true;

            getAzureWebsiteLogCmdlet.ExecuteCmdlet();

            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
