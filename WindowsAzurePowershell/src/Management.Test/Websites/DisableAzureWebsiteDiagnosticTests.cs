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
    public class DisableAzureWebsiteDiagnosticTests : WebsitesTestBase
    {
        private const string websiteName = "website1";

        private Mock<IWebsitesClient> websitesClientMock = new Mock<IWebsitesClient>();

        private DisableAzureWebsiteDiagnosticCommand disableAzureWebsiteDiagnosticCommand;

        private Mock<ICommandRuntime> commandRuntimeMock;

        [TestInitialize]
        public override void SetupTest()
        {
            websitesClientMock = new Mock<IWebsitesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
        }

        [TestMethod]
        public void DisableAzureWebsiteDiagnosticSite()
        {
            // Setup
            websitesClientMock.Setup(f => f.DisableSiteDiagnostic(
                websiteName,
                true,
                true,
                true));

            disableAzureWebsiteDiagnosticCommand = new DisableAzureWebsiteDiagnosticCommand()
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
            disableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.DisableSiteDiagnostic(
                websiteName,
                true,
                true,
                true), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }

        [TestMethod]
        public void DisableAzureWebsiteDiagnosticPassThru()
        {
            // Setup
            websitesClientMock.Setup(f => f.DisableSiteDiagnostic(
                websiteName,
                true,
                true,
                true));

            disableAzureWebsiteDiagnosticCommand = new DisableAzureWebsiteDiagnosticCommand()
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
            disableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.DisableSiteDiagnostic(
                websiteName,
                true,
                true,
                true), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Once());
        }

        [TestMethod]
        public void DisableAzureWebsiteDiagnosticSiteIgnoreSetting()
        {
            // Setup
            websitesClientMock.Setup(f => f.DisableSiteDiagnostic(
                websiteName,
                true,
                false,
                true));

            disableAzureWebsiteDiagnosticCommand = new DisableAzureWebsiteDiagnosticCommand()
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
            disableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.DisableSiteDiagnostic(
                websiteName,
                true,
                false,
                true), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }

        [TestMethod]
        public void DisableAzureWebsiteDiagnosticApplication()
        {
            // Setup
            websitesClientMock.Setup(f => f.DisableApplicationDiagnostic(
                websiteName,
                WebsiteDiagnosticOutput.FileSystem));

            disableAzureWebsiteDiagnosticCommand = new DisableAzureWebsiteDiagnosticCommand()
            {
                ShareChannel = true,
                CommandRuntime = commandRuntimeMock.Object,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName },
                WebsitesClient = websitesClientMock.Object,
                Type = WebsiteDiagnosticType.Application,
                Output = WebsiteDiagnosticOutput.FileSystem,
            };

            // Test
            disableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.DisableApplicationDiagnostic(
                websiteName,
                WebsiteDiagnosticOutput.FileSystem), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }

        [TestMethod]
        public void DisableAzureWebsiteDiagnosticApplicationTableLog()
        {
            // Setup
            websitesClientMock.Setup(f => f.DisableApplicationDiagnostic(
                websiteName,
                WebsiteDiagnosticOutput.StorageTable));

            disableAzureWebsiteDiagnosticCommand = new DisableAzureWebsiteDiagnosticCommand()
            {
                ShareChannel = true,
                CommandRuntime = commandRuntimeMock.Object,
                Name = websiteName,
                CurrentSubscription = new SubscriptionData { SubscriptionId = base.subscriptionName },
                WebsitesClient = websitesClientMock.Object,
                Type = WebsiteDiagnosticType.Application,
                Output = WebsiteDiagnosticOutput.StorageTable
            };

            // Test
            disableAzureWebsiteDiagnosticCommand.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.DisableApplicationDiagnostic(
                websiteName,
                WebsiteDiagnosticOutput.StorageTable), Times.Once());

            commandRuntimeMock.Verify(f => f.WriteObject(true), Times.Never());
        }
    }
}
