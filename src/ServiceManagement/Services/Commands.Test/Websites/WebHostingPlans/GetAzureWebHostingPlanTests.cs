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


namespace Microsoft.WindowsAzure.Commands.Test.Websites.WebHostingPlans
{
    using Commands.Common.Properties;
    using Commands.Utilities.Common;
    using Commands.Utilities.Websites;
    using Commands.Utilities.Websites.Services.WebEntities;
    using Commands.Websites;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Utilities.Common;
    using Utilities.Websites;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Websites.WebHostingPlan;

    [TestClass]
    public class GetAzureWebHostingPlanTests : WebsitesTestBase
    {
        [TestMethod]
        public void ListWebHostingPlansTest()
        {
            // Setup
            var clientMock = new Mock<IWebsitesClient>();
            clientMock.Setup(c => c.ListWebSpaces())
                .Returns(new[] {new WebSpace {Name = "webspace1"}, new WebSpace {Name = "webspace2"}});

            clientMock.Setup(c => c.ListWebHostingPlans())
                .Returns(new List<WebHostingPlan>
                {
                    new WebHostingPlan {Name = "Plan1", WebSpace = "webspace1"},
                    new WebHostingPlan { Name = "Plan2", WebSpace = "webspace2" }
                });
             
            // Test
            var command = new GetAzureWebHostingPlanCommand
            {
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new WindowsAzureSubscription { SubscriptionId = subscriptionId },
                WebsitesClient = clientMock.Object
            };

            command.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            var plans = (IEnumerable<WebHostingPlan>)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.IsNotNull(plans);
            Assert.IsTrue(plans.Any(p => (p).Name.Equals("Plan1") && (p).WebSpace.Equals("webspace1")));
            Assert.IsTrue(plans.Any(p => (p).Name.Equals("Plan2") && (p).WebSpace.Equals("webspace2")));
        }

        [TestMethod]
        public void GetAzureWebHostingPlanBasicTest()
        {
            // Setup
            var clientMock = new Mock<IWebsitesClient>();
            clientMock.Setup(c => c.ListWebSpaces())
                .Returns(new[] { new WebSpace { Name = "webspace1" }, new WebSpace { Name = "webspace2" } });

            clientMock.Setup(c => c.ListWebHostingPlans("webspace1"))
                .Returns(new List<WebHostingPlan> { new WebHostingPlan { Name = "Plan1", WebSpace = "webspace1" } });
             
            // Test
            var command = new GetAzureWebHostingPlanCommand
            {
                WebSpaceName = "webspace1",
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new WindowsAzureSubscription { SubscriptionId = subscriptionId },
                WebsitesClient = clientMock.Object
            };

            command.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            var plans = (IEnumerable<WebHostingPlan>)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.IsNotNull(plans);
            Assert.IsTrue(plans.Any(p => (p).Name.Equals("Plan1") && (p).WebSpace.Equals("webspace1")));
        }
    }
}
