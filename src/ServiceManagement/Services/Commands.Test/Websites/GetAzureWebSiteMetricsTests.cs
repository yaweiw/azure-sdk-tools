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

    [TestClass]
    public class GetAzureWebsiteMetricsTests : WebsitesTestBase
    {
        [TestMethod]
        public void GetWebsiteMetricsBasicTest()
        {
            // Setup
            var clientMock = new Mock<IWebsitesClient>();
            clientMock.Setup(c => c.ListWebSpaces())
                .Returns(new[] {new WebSpace {Name = "webspace1"}, new WebSpace {Name = "webspace2"}});

            clientMock.Setup(c => c.ListSitesInWebSpace("webspace1"))
                .Returns(new[] {new Site {Name = "website1", WebSpace = "webspace1"}});

            clientMock.Setup(c => c.GetHistoricalUsageMetrics("website1", null, null, null, null, null))
                .Returns(new[] {new MetricResponse() {Code = "Success", 
                    Data = new MetricSet()
                    {
                        Name = "CPU Time",
                        StartTime = DateTime.Parse("7/28/2014 1:00:00 AM"),
                        EndTime = DateTime.Parse("7/28/2014 2:00:00 AM"),
                        Values = new List<MetricSample>
                        {
                            new MetricSample
                            {
                                TimeCreated = DateTime.Parse("7/28/2014 1:00:00 AM"),
                                Total = 201,
                            }
                        }
                    }}});
            
            // Test
            var command = new GetAzureWebsiteMetricCommand
            {
                Name = "website1",
                CommandRuntime = new MockCommandRuntime(),
                CurrentSubscription = new WindowsAzureSubscription { SubscriptionId = subscriptionId },
                WebsitesClient = clientMock.Object
            };

            command.ExecuteCmdlet();
            Assert.AreEqual(1, ((MockCommandRuntime)command.CommandRuntime).OutputPipeline.Count);
            var metrics = (MetricResponse)((MockCommandRuntime)command.CommandRuntime).OutputPipeline.FirstOrDefault();
            Assert.IsNotNull(metrics);
            Assert.AreEqual("CPU Time", metrics.Data.Name);
            Assert.IsNotNull(metrics.Data.Values);
            Assert.IsNotNull(metrics.Data.Values[0]);
            Assert.AreEqual(201, metrics.Data.Values[0].Total);
        }
    }
}
