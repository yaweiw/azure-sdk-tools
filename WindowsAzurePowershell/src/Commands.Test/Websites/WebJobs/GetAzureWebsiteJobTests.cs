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

namespace Microsoft.WindowsAzure.Commands.Test.Websites
{
    using System.Collections.Generic;
    using System.Management.Automation;
    using Commands.Utilities.Websites;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Utilities.Websites.Services.WebJobs;
    using Microsoft.WindowsAzure.Commands.Websites.WebJobs;
    using Moq;
    using Utilities.Websites;
    using Microsoft.WindowsAzure.WebSitesExtensions.Models;

    [TestClass]
    public class GetAzureWebsiteJobTests : WebsitesTestBase
    {
        private const string websiteName = "website1";

        private const string slot = "staging";

        private Mock<IWebsitesClient> websitesClientMock;

        private GetAzureWebsiteJobCommand cmdlet; 

        private Mock<ICommandRuntime> commandRuntimeMock;

        [TestInitialize]
        public override void SetupTest()
        {
            websitesClientMock = new Mock<IWebsitesClient>();
            commandRuntimeMock = new Mock<ICommandRuntime>();
            cmdlet = new GetAzureWebsiteJobCommand()
            {
                CommandRuntime = commandRuntimeMock.Object,
                WebsitesClient = websitesClientMock.Object,
                Name = websiteName,
                Slot = slot
            };
        }

        [TestMethod]
        public void GetEmptyWebJobList()
        {
            // Setup
            List<PSWebJob> output = new List<PSWebJob>();
            WebJobFilterOptions options = null;
            websitesClientMock.Setup(f => f.FilterWebJobs(It.IsAny<WebJobFilterOptions>()))
                .Returns(output)
                .Callback((WebJobFilterOptions actual) => options = actual)
                .Verifiable();

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.FilterWebJobs(options), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(output, true), Times.Once());
        }

        [TestMethod]
        public void GetOneWebJob()
        {
            // Setup
            string jobName = "webJobName";
            WebJobType type = WebJobType.Continuous;
            List<PSWebJob> output = new List<PSWebJob>() { new PSWebJob() { JobName = jobName, JobType = type } };
            WebJobFilterOptions options = null;
            websitesClientMock.Setup(f => f.FilterWebJobs(It.IsAny<WebJobFilterOptions>()))
                .Returns(output)
                .Callback((WebJobFilterOptions actual) => options = actual)
                .Verifiable();
            cmdlet.JobName = jobName;
            cmdlet.JobType = type.ToString();

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.FilterWebJobs(options), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(output, true), Times.Once());
        }

        [TestMethod]
        public void GetsMultipleWebJobs()
        {
            // Setup
            string jobName1 = "webJobName1";
            string jobName2 = "webJobName2";
            string jobName3 = "webJobName3";
            WebJobType type1 = WebJobType.Continuous;
            WebJobType type2 = WebJobType.Continuous;
            WebJobType type3 = WebJobType.Triggered;
            WebJobFilterOptions options = null;
            List<PSWebJob> output = new List<PSWebJob>() {
                new PSWebJob() { JobName = jobName1, JobType = type1 },
                new PSWebJob() { JobName = jobName2, JobType = type2 },
                new PSWebJob() { JobName = jobName3, JobType = type3 }
            };
            websitesClientMock.Setup(f => f.FilterWebJobs(It.IsAny<WebJobFilterOptions>()))
                .Returns(output)
                .Callback((WebJobFilterOptions actual) => options = actual)
                .Verifiable();

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.FilterWebJobs(options), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(output, true), Times.Once());
        }
    }
}
