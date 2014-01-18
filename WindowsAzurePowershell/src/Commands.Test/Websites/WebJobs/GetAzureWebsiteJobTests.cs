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
    using Microsoft.WindowsAzure.Commands.Utilities.Websites.Services;
    using Microsoft.WindowsAzure.Commands.Websites.WebJobs;
    using Moq;
    using Utilities.Websites;

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
            List<WebJob> output = new List<WebJob>();
            websitesClientMock.Setup(f => f.FilterWebJobs(websiteName, slot, null, null)).Returns(output);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.FilterWebJobs(websiteName, slot, null, null), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(output, true), Times.Once());
        }

        [TestMethod]
        public void GetOneWebJob()
        {
            // Setup
            string jobName = "webJobName";
            string type = WebJobType.Continuous.ToString();
            List<WebJob> output = new List<WebJob>() { new WebJob() { Name = jobName, Type = type } };
            websitesClientMock.Setup(f => f.FilterWebJobs(websiteName, slot, jobName, type)).Returns(output);
            cmdlet.JobName = jobName;
            cmdlet.JobType = type;

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.FilterWebJobs(websiteName, slot, jobName, type), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(output[0]), Times.Once());
        }

        [TestMethod]
        public void GetsMultipleJobs()
        {
            // Setup
            string jobName1 = "webJobName1";
            string jobName2 = "webJobName2";
            string jobName3 = "webJobName3";
            string type1 = WebJobType.Continuous.ToString();
            string type2 = WebJobType.Continuous.ToString();
            string type3 = WebJobType.Triggered.ToString();
            List<WebJob> output = new List<WebJob>() {
                new WebJob() { Name = jobName1, Type = type1 },
                new WebJob() { Name = jobName2, Type = type2 },
                new WebJob() { Name = jobName3, Type = type3 }
            };
            websitesClientMock.Setup(f => f.FilterWebJobs(websiteName, slot, null, null)).Returns(output);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            websitesClientMock.Verify(f => f.FilterWebJobs(websiteName, slot, null, null), Times.Once());
            commandRuntimeMock.Verify(f => f.WriteObject(output, true), Times.Once());
        }
    }
}
