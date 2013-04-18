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

namespace Microsoft.WindowsAzure.Management.Test.Websites
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Test.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Common;
    using Microsoft.WindowsAzure.Management.Utilities.Websites;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.DeploymentEntities;
    using Microsoft.WindowsAzure.Management.Utilities.Websites.Services.WebEntities;
    using Microsoft.WindowsAzure.Management.Websites;
    using Moq;

    [TestClass]
    public class EnableAzureWebsiteDiagnosticTests : WebsitesTestBase
    {
        private const string websiteName = "website1";

        private Mock<IWebsitesClient> websitesClientMock = new Mock<IWebsitesClient>();

        private EnableAzureWebsiteDiagnosticCommand enableAzureWebsiteDiagnosticCommand;

        private Mock<ICommandRuntime> commandRuntimeMock;

        [TestInitialize]
        public override void SetupTest()
        {
            websitesClientMock = new Mock<IWebsitesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
        }

        [TestMethod]
        public void EnableAzureWebsiteDiagnosticSite()
        {
            // Setup
            websitesClientMock.Setup(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Site,
                true,
                true,
                true,
                It.IsAny<WebsiteDiagnosticOutput>(),
                It.IsAny<LogEntryType>(),
                It.IsAny<string>()));

            enableAzureWebsiteDiagnosticCommand = new EnableAzureWebsiteDiagnosticCommand()
            {
                ShareChannel = true,
                CommandRuntime = commandRuntimeMock.Object,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName },
                WebsitesClient = websitesClientMock.Object,
                WebServerLogging = true,
                DetailedErrorMessages = true,
                FailedRequestTracing = true,
                Type = WebsiteDiagnosticType.Site
            };

            // Test
            enableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Site,
                true,
                true,
                true,
                It.IsAny<WebsiteDiagnosticOutput>(),
                It.IsAny<LogEntryType>(),
                It.IsAny<string>()), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }

        [TestMethod]
        public void EnableAzureWebsiteDiagnosticPassThru()
        {
            // Setup
            websitesClientMock.Setup(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Site,
                true,
                true,
                true,
                It.IsAny<WebsiteDiagnosticOutput>(),
                It.IsAny<LogEntryType>(),
                It.IsAny<string>()));

            enableAzureWebsiteDiagnosticCommand = new EnableAzureWebsiteDiagnosticCommand()
            {
                ShareChannel = true,
                CommandRuntime = commandRuntimeMock.Object,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName },
                WebsitesClient = websitesClientMock.Object,
                WebServerLogging = true,
                DetailedErrorMessages = true,
                FailedRequestTracing = true,
                Type = WebsiteDiagnosticType.Site,
                PassThru = true
            };

            // Test
            enableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Site,
                true,
                true,
                true,
                It.IsAny<WebsiteDiagnosticOutput>(),
                It.IsAny<LogEntryType>(),
                It.IsAny<string>()), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Once());
        }

        [TestMethod]
        public void EnableAzureWebsiteDiagnosticSiteIgnoreSetting()
        {
            // Setup
            websitesClientMock.Setup(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Site,
                true,
                new bool?(),
                true,
                It.IsAny<WebsiteDiagnosticOutput>(),
                It.IsAny<LogEntryType>(),
                It.IsAny<string>()));

            enableAzureWebsiteDiagnosticCommand = new EnableAzureWebsiteDiagnosticCommand()
            {
                ShareChannel = true,
                CommandRuntime = commandRuntimeMock.Object,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName },
                WebsitesClient = websitesClientMock.Object,
                WebServerLogging = true,
                FailedRequestTracing = true,
                Type = WebsiteDiagnosticType.Site
            };

            // Test
            enableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Site,
                true,
                new bool?(),
                true,
                It.IsAny<WebsiteDiagnosticOutput>(),
                It.IsAny<LogEntryType>(),
                It.IsAny<string>()), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }

        [TestMethod]
        public void EnableAzureWebsiteDiagnosticApplication()
        {
            // Setup
            websitesClientMock.Setup(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Application,
                new bool?(),
                new bool?(),
                new bool?(),
                WebsiteDiagnosticOutput.FileSystem,
                LogEntryType.Information,
                It.IsAny<string>()));

            enableAzureWebsiteDiagnosticCommand = new EnableAzureWebsiteDiagnosticCommand()
            {
                ShareChannel = true,
                CommandRuntime = commandRuntimeMock.Object,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName },
                WebsitesClient = websitesClientMock.Object,
                Type = WebsiteDiagnosticType.Application,
                Output = WebsiteDiagnosticOutput.FileSystem,
                LogLevel = LogEntryType.Information
            };

            // Test
            enableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Application,
                new bool?(),
                new bool?(),
                new bool?(),
                WebsiteDiagnosticOutput.FileSystem,
                LogEntryType.Information,
                It.IsAny<string>()), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }

        [TestMethod]
        public void EnableAzureWebsiteDiagnosticApplicationTableLog()
        {
            // Setup
            string storageName = "MyStorage";
            websitesClientMock.Setup(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Application,
                new bool?(),
                new bool?(),
                new bool?(),
                WebsiteDiagnosticOutput.StorageTable,
                LogEntryType.Information,
                storageName));

            enableAzureWebsiteDiagnosticCommand = new EnableAzureWebsiteDiagnosticCommand()
            {
                ShareChannel = true,
                CommandRuntime = commandRuntimeMock.Object,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName },
                WebsitesClient = websitesClientMock.Object,
                Type = WebsiteDiagnosticType.Application,
                Output = WebsiteDiagnosticOutput.StorageTable,
                LogLevel = LogEntryType.Information,
                StorageAccountName = storageName
            };

            // Test
            enableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Application,
                new bool?(),
                new bool?(),
                new bool?(),
                WebsiteDiagnosticOutput.StorageTable,
                LogEntryType.Information,
                storageName), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }

        [TestMethod]
        public void EnableAzureWebsiteDiagnosticApplicationTableLogUseCurrentStorageAccount()
        {
            // Setup
            string storageName = "MyStorage";
            websitesClientMock.Setup(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Application,
                new bool?(),
                new bool?(),
                new bool?(),
                WebsiteDiagnosticOutput.StorageTable,
                LogEntryType.Information,
                storageName));

            enableAzureWebsiteDiagnosticCommand = new EnableAzureWebsiteDiagnosticCommand()
            {
                ShareChannel = true,
                CommandRuntime = commandRuntimeMock.Object,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData
                {
                    SubscriptionId = base.subscriptionName,
                    CurrentStorageAccount = storageName
                },
                WebsitesClient = websitesClientMock.Object,
                Type = WebsiteDiagnosticType.Application,
                Output = WebsiteDiagnosticOutput.StorageTable,
                LogLevel = LogEntryType.Information,
            };

            // Test
            enableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.EnableAzureWebsiteDiagnostic(
                websiteName,
                WebsiteDiagnosticType.Application,
                new bool?(),
                new bool?(),
                new bool?(),
                WebsiteDiagnosticOutput.StorageTable,
                LogEntryType.Information,
                storageName), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }
    }
}
