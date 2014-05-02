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
    public class SetTrafficManagerProfileTests
    {
        private const int monitorExpectedStatusCode = (int)HttpStatusCode.OK;
        private const string verb = "GET";

        private const string profileName = "my-profile";
        private const string profileDomainName = "my.profile.trafficmanager.net";

        // Old profile
        private const LoadBalancingMethod loadBalancingMethod = LoadBalancingMethod.Failover;
        private const int monitorPort = 80;
        private const DefinitionMonitorProtocol monitorProtocol = DefinitionMonitorProtocol.Http;
        private const string monitorRelativePath = "/";
        private const int ttl = 30;

        // New profile
        private const LoadBalancingMethod newLoadBalancingMethod = LoadBalancingMethod.Performance;
        private const int newMonitorPort = 8080;
        private const DefinitionMonitorProtocol newMonitorProtocol = DefinitionMonitorProtocol.Https;
        private const string newMonitorRelativePath = "/index.html";
        private const int newTtl = 300;

        private MockCommandRuntime mockCommandRuntime;

        private SetAzureTrafficManagerProfile cmdlet;

        private Mock<ITrafficManagerClient> clientMock;

        [TestInitialize]
        public void TestSetup()
        {
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new SetAzureTrafficManagerProfile();
            cmdlet.CommandRuntime = mockCommandRuntime;
            clientMock = new Mock<ITrafficManagerClient>();
        }

        [TestMethod]
        public void ProcessSetProfileTestAllArgs()
        {
            // Setup

            ProfileWithDefinition oldProfileWithDefinition = new ProfileWithDefinition()
            {
                DomainName = profileDomainName,
                Name = profileName,
                Endpoints = new List<TrafficManagerEndpoint>(),
                LoadBalancingMethod = loadBalancingMethod,
                MonitorPort = monitorPort,
                Status = ProfileDefinitionStatus.Enabled,
                MonitorRelativePath = monitorRelativePath,
                MonitorProtocol = monitorProtocol,
                TimeToLiveInSeconds = ttl
            };

            ProfileWithDefinition newProfileWithDefinition = new ProfileWithDefinition()
            {
                DomainName = profileDomainName,
                Name = profileName,
                Endpoints = new List<TrafficManagerEndpoint>(),
                LoadBalancingMethod = newLoadBalancingMethod,
                MonitorPort = newMonitorPort,
                Status = ProfileDefinitionStatus.Enabled,
                MonitorRelativePath = newMonitorRelativePath,
                MonitorProtocol = newMonitorProtocol,
                TimeToLiveInSeconds = newTtl
            };


            DefinitionMonitor newMonitor = new DefinitionMonitor()
            {
                HttpOptions = new DefinitionMonitorHTTPOptions()
                {
                    ExpectedStatusCode = monitorExpectedStatusCode,
                    RelativePath = newMonitorRelativePath,
                    Verb = verb
                }
            };

            DefinitionCreateParameters updateDefinitionCreateParameters = new DefinitionCreateParameters()
            {
                DnsOptions = new DefinitionDnsOptions()
                {
                    TimeToLiveInSeconds = newTtl
                },

                Policy = new DefinitionPolicyCreateParameters()
                {
                    LoadBalancingMethod = newLoadBalancingMethod,
                    Endpoints = new DefinitionEndpointCreateParameters[0]
                },

                Monitors = new[] { newMonitor }
            };

            clientMock
                .Setup(c => c.AssignDefinitionToProfile(profileName, It.IsAny<DefinitionCreateParameters>()))
                .Returns(newProfileWithDefinition);

            clientMock
                .Setup(c => c.InstantiateTrafficManagerDefinition(
                newLoadBalancingMethod.ToString(),
                newMonitorPort,
                newMonitorProtocol.ToString(),
                newMonitorRelativePath,
                newTtl,
                oldProfileWithDefinition.Endpoints))
                .Returns(updateDefinitionCreateParameters);

            cmdlet = new SetAzureTrafficManagerProfile()
            {
                Name = profileName,
                LoadBalancingMethod = newLoadBalancingMethod.ToString(),
                MonitorPort = newMonitorPort,
                MonitorProtocol = newMonitorProtocol.ToString(),
                MonitorRelativePath = newMonitorRelativePath,
                Ttl = newTtl,
                TrafficManagerClient = clientMock.Object,
                CommandRuntime = mockCommandRuntime,
                TrafficManagerProfile = oldProfileWithDefinition
            };


            // Action

            cmdlet.ExecuteCmdlet();
            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            // Assert

            Assert.AreEqual(newProfileWithDefinition.Name, actual.Name);
            Assert.AreEqual(newProfileWithDefinition.DomainName, actual.DomainName);
            Assert.AreEqual(newProfileWithDefinition.LoadBalancingMethod, actual.LoadBalancingMethod);
            Assert.AreEqual(newProfileWithDefinition.MonitorPort, actual.MonitorPort);
            Assert.AreEqual(newProfileWithDefinition.MonitorProtocol, actual.MonitorProtocol);
            Assert.AreEqual(newProfileWithDefinition.MonitorRelativePath, actual.MonitorRelativePath);
            Assert.AreEqual(newProfileWithDefinition.TimeToLiveInSeconds, actual.TimeToLiveInSeconds);

            // Most important assert; the cmdlet is passing the right parameters
            clientMock.Verify(c => c.InstantiateTrafficManagerDefinition(
                newLoadBalancingMethod.ToString(),
                newMonitorPort,
                newMonitorProtocol.ToString(),
                newMonitorRelativePath,
                newTtl,
                oldProfileWithDefinition.Endpoints), Times.Once());
        }

        [TestMethod]
        public void ProcessSetProfileTestLoadBalancingMethod()
        {
            ProfileWithDefinition oldProfileWithDefinition = new ProfileWithDefinition()
            {
                DomainName = profileDomainName,
                Name = profileName,
                Endpoints = new List<TrafficManagerEndpoint>(),
                LoadBalancingMethod = loadBalancingMethod,
                MonitorPort = monitorPort,
                Status = ProfileDefinitionStatus.Enabled,
                MonitorRelativePath = monitorRelativePath,
                MonitorProtocol = monitorProtocol,
                TimeToLiveInSeconds = ttl
            };

            DefinitionMonitor Monitor = new DefinitionMonitor()
            {
                HttpOptions = new DefinitionMonitorHTTPOptions()
                {
                    ExpectedStatusCode = monitorExpectedStatusCode,
                    RelativePath = monitorRelativePath,
                    Verb = verb
                }
            };

            DefinitionCreateParameters updateDefinitionCreateParameters = new DefinitionCreateParameters()
            {
                DnsOptions = new DefinitionDnsOptions()
                {
                    TimeToLiveInSeconds = oldProfileWithDefinition.TimeToLiveInSeconds
                },

                Policy = new DefinitionPolicyCreateParameters()
                {
                    LoadBalancingMethod = newLoadBalancingMethod,
                    Endpoints = new DefinitionEndpointCreateParameters[0]
                },

                Monitors = new[] { Monitor }
            };

            clientMock
                .Setup(c => c.InstantiateTrafficManagerDefinition(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<IList<TrafficManagerEndpoint>>()))
                .Returns(updateDefinitionCreateParameters);


            cmdlet = new SetAzureTrafficManagerProfile()
            {
                Name = profileName,
                // We only change the load balancign method
                LoadBalancingMethod = newLoadBalancingMethod.ToString(),
                TrafficManagerClient = clientMock.Object,
                CommandRuntime = mockCommandRuntime,
                TrafficManagerProfile = oldProfileWithDefinition
            };


            // Action

            cmdlet.ExecuteCmdlet();
            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            // Assert

            clientMock.Verify(
                c => c.InstantiateTrafficManagerDefinition(
                    // load balancing method is the new one
                    newLoadBalancingMethod.ToString(),
                    monitorPort,
                    monitorProtocol.ToString(),
                    monitorRelativePath,
                    ttl,
                    oldProfileWithDefinition.Endpoints),
                Times.Once());
        }

        [TestMethod]
        public void ProcessSetProfileTestMonitorPort()
        {
            ProfileWithDefinition oldProfileWithDefinition = new ProfileWithDefinition()
            {
                DomainName = profileDomainName,
                Name = profileName,
                Endpoints = new List<TrafficManagerEndpoint>(),
                LoadBalancingMethod = loadBalancingMethod,
                MonitorPort = monitorPort,
                Status = ProfileDefinitionStatus.Enabled,
                MonitorRelativePath = monitorRelativePath,
                MonitorProtocol = monitorProtocol,
                TimeToLiveInSeconds = ttl
            };

            cmdlet = new SetAzureTrafficManagerProfile()
            {
                Name = profileName,
                // We only change the monitor port
                MonitorPort = newMonitorPort,
                TrafficManagerClient = clientMock.Object,
                CommandRuntime = mockCommandRuntime,
                TrafficManagerProfile = oldProfileWithDefinition
            };


            // Action

            cmdlet.ExecuteCmdlet();
            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            // Assert

            clientMock.Verify(
                c => c.InstantiateTrafficManagerDefinition(
                    loadBalancingMethod.ToString(),
                    // monitor port is the new one
                    newMonitorPort,
                    monitorProtocol.ToString(),
                    monitorRelativePath,
                    ttl,
                    oldProfileWithDefinition.Endpoints),
                Times.Once());
        }

        [TestMethod]
        public void ProcessSetProfileTestMonitorProtocol()
        {
            ProfileWithDefinition oldProfileWithDefinition = new ProfileWithDefinition()
            {
                DomainName = profileDomainName,
                Name = profileName,
                Endpoints = new List<TrafficManagerEndpoint>(),
                LoadBalancingMethod = loadBalancingMethod,
                MonitorPort = monitorPort,
                Status = ProfileDefinitionStatus.Enabled,
                MonitorRelativePath = monitorRelativePath,
                MonitorProtocol = monitorProtocol,
                TimeToLiveInSeconds = ttl
            };

            cmdlet = new SetAzureTrafficManagerProfile()
            {
                Name = profileName,
                // We only change the monitor protocl
                MonitorProtocol = newMonitorProtocol.ToString(),
                TrafficManagerClient = clientMock.Object,
                CommandRuntime = mockCommandRuntime,
                TrafficManagerProfile = oldProfileWithDefinition
            };


            // Action

            cmdlet.ExecuteCmdlet();
            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            // Assert

            clientMock.Verify(
                c => c.InstantiateTrafficManagerDefinition(
                    loadBalancingMethod.ToString(),
                    monitorPort,
                    // monitor protocol is the new one
                    newMonitorProtocol.ToString(),
                    monitorRelativePath,
                    ttl,
                    oldProfileWithDefinition.Endpoints),
                Times.Once());
        }

        [TestMethod]
        public void ProcessSetProfileTestMonitorRelativePath()
        {
            ProfileWithDefinition oldProfileWithDefinition = new ProfileWithDefinition()
            {
                DomainName = profileDomainName,
                Name = profileName,
                Endpoints = new List<TrafficManagerEndpoint>(),
                LoadBalancingMethod = loadBalancingMethod,
                MonitorPort = monitorPort,
                Status = ProfileDefinitionStatus.Enabled,
                MonitorRelativePath = monitorRelativePath,
                MonitorProtocol = monitorProtocol,
                TimeToLiveInSeconds = ttl
            };

            cmdlet = new SetAzureTrafficManagerProfile()
            {
                Name = profileName,
                // We only change the monitor protocl
                MonitorRelativePath = newMonitorRelativePath,
                TrafficManagerClient = clientMock.Object,
                CommandRuntime = mockCommandRuntime,
                TrafficManagerProfile = oldProfileWithDefinition
            };


            // Action

            cmdlet.ExecuteCmdlet();
            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            // Assert

            clientMock.Verify(
                c => c.InstantiateTrafficManagerDefinition(
                    loadBalancingMethod.ToString(),
                    monitorPort,
                    monitorProtocol.ToString(),
                    // monitor relative path is the new one
                    newMonitorRelativePath,
                    ttl,
                    oldProfileWithDefinition.Endpoints),
                Times.Once());
        }

        [TestMethod]
        public void ProcessSetProfileTestTtl()
        {
            ProfileWithDefinition oldProfileWithDefinition = new ProfileWithDefinition()
            {
                DomainName = profileDomainName,
                Name = profileName,
                Endpoints = new List<TrafficManagerEndpoint>(),
                LoadBalancingMethod = loadBalancingMethod,
                MonitorPort = monitorPort,
                Status = ProfileDefinitionStatus.Enabled,
                MonitorRelativePath = monitorRelativePath,
                MonitorProtocol = monitorProtocol,
                TimeToLiveInSeconds = ttl
            };

            cmdlet = new SetAzureTrafficManagerProfile()
            {
                Name = profileName,
                // We only change the ttl
                Ttl = newTtl,
                TrafficManagerClient = clientMock.Object,
                CommandRuntime = mockCommandRuntime,
                TrafficManagerProfile = oldProfileWithDefinition
            };


            // Action

            cmdlet.ExecuteCmdlet();
            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            // Assert

            clientMock.Verify(
                c => c.InstantiateTrafficManagerDefinition(
                    loadBalancingMethod.ToString(),
                    monitorPort,
                    monitorProtocol.ToString(),
                    monitorRelativePath,
                    // ttl is the new one
                    newTtl,
                    oldProfileWithDefinition.Endpoints),
                Times.Once());
        }
    }
}
