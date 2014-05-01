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

namespace Microsoft.WindowsAzure.Commands.Test.TrafficManager.Profiles
{
    using System.Collections.Generic;
    using System.Net;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.TrafficManager.Profile;
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager;
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager.Models;
    using Microsoft.WindowsAzure.Management.TrafficManager.Models;
    using Moq;

    [TestClass]
    public class NewTrafficManagerProfileTests
    {
        private const string profileName = "my-profile";
        private const string profileDomainName = "my.profile.trafficmanager.net";
        private const LoadBalancingMethod loadBalancingMethod = LoadBalancingMethod.Failover;
        private const string domainName = "www.example.com";
        private const int weight = 3;
        private const string cloudServiceType = "CloudService";
        private const string azureWebsiteType = "AzureWebsite";
        private const string anyType = "Any";
        private const string location = "West US";
        private const EndpointStatus status = EndpointStatus.Enabled;
        private const int monitorPort = 80;
        private const DefinitionMonitorProtocol monitorProtocol = DefinitionMonitorProtocol.Http;
        private const string monitorRelativePath = "/";
        private const int ttl = 30;
        private const int monitorExpectedStatusCode = (int)HttpStatusCode.OK;

        private MockCommandRuntime mockCommandRuntime;

        private NewAzureTrafficManagerProfile cmdlet;

        private Mock<ITrafficManagerClient> clientMock;

        [TestInitialize]
        public void TestSetup()
        {
            mockCommandRuntime = new MockCommandRuntime();
            clientMock = new Mock<ITrafficManagerClient>();
        }

        [TestMethod]
        public void ProcessNewProfileTest()
        {
            // Setup
            DefinitionMonitor monitor = new DefinitionMonitor()
            {
                HttpOptions = new DefinitionMonitorHTTPOptions()
                {
                    ExpectedStatusCode = monitorExpectedStatusCode,
                    RelativePath = monitorRelativePath,
                    Verb = "GET"
                }
            };

            DefinitionCreateParameters expectedParameters = new DefinitionCreateParameters()
            {
                DnsOptions = new DefinitionDnsOptions()
                {
                    TimeToLiveInSeconds = ttl
                },

                Policy = new DefinitionPolicyCreateParameters()
                {
                    LoadBalancingMethod = loadBalancingMethod,
                    Endpoints = new DefinitionEndpointCreateParameters[0]
                },

                Monitors = new [] { monitor }
            };

            clientMock.Setup(c => c.NewAzureTrafficManagerProfile(
                            profileName,
                            profileDomainName,
                            loadBalancingMethod.ToString(),
                            monitorPort,
                            monitorProtocol.ToString(),
                            monitorRelativePath,
                            ttl))
                      .Returns(new ProfileWithDefinition()
                      {
                          DomainName = profileDomainName,
                          Name = profileName,
                          Endpoints = new List<TrafficManagerEndpoint>(),
                          LoadBalancingMethod = loadBalancingMethod,
                          MonitorPort = 80,
                          Status = ProfileDefinitionStatus.Enabled,
                          MonitorRelativePath = monitorRelativePath,
                          TimeToLiveInSeconds = ttl
                      });


            cmdlet = new NewAzureTrafficManagerProfile()
            {
                Name = profileName,
                DomainName = profileDomainName,
                LoadBalancingMethod = loadBalancingMethod.ToString(),
                MonitorPort = monitorPort,
                MonitorProtocol = monitorProtocol.ToString(),
                MonitorRelativePath = monitorRelativePath,
                Ttl = ttl,
                TrafficManagerClient = clientMock.Object,
                CommandRuntime = mockCommandRuntime
            };

            // Action
            cmdlet.ExecuteCmdlet();

            // Assert
            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            Assert.AreEqual(profileName, actual.Name);
            Assert.AreEqual(profileDomainName, actual.DomainName);
            Assert.AreEqual(monitorRelativePath, actual.MonitorRelativePath);
            Assert.AreEqual(ttl, actual.TimeToLiveInSeconds);
            Assert.AreEqual(loadBalancingMethod, actual.LoadBalancingMethod);
            Assert.IsTrue(actual.Endpoints.Count == 0);
        }
    }
}
