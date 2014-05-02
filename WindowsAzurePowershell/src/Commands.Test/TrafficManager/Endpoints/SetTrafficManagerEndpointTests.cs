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

namespace Microsoft.WindowsAzure.Commands.Test.TrafficManager.Endpoints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.TrafficManager.Endpoint;
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager;
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager.Models;
    using Microsoft.WindowsAzure.Management.TrafficManager.Models;
    using Moq;

    [TestClass]
    public class SetTrafficManagerEndpointTests : TestBase
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

        private MockCommandRuntime mockCommandRuntime;

        private SetAzureTrafficManagerEndpoint cmdlet;

        private Mock<TrafficManagerClient> trafficManagerClientMock;

        [TestInitialize]
        public void TestSetup()
        {
            mockCommandRuntime = new MockCommandRuntime();
            cmdlet = new SetAzureTrafficManagerEndpoint();
            cmdlet.CommandRuntime = mockCommandRuntime;
            trafficManagerClientMock = new Mock<TrafficManagerClient>();
        }

        [TestMethod]
        public void SetTrafficManagerEndpointSucceeds()
        {
            // Setup
            ProfileWithDefinition original = GetProfileWithDefinition();

            TrafficManagerEndpoint existingEndpoint = new TrafficManagerEndpoint()
            {
                DomainName = domainName,
                Type = EndpointType.Any,
                Status = EndpointStatus.Enabled
            };

            original.Endpoints.Add(existingEndpoint);

            // Assert the endpoint exists
            Assert.IsTrue(original.Endpoints.Any(e => e.DomainName == domainName));

            cmdlet = new SetAzureTrafficManagerEndpoint()
            {
                Name = profileName,
                DomainName = domainName,
                TrafficManagerProfile = original,
                //Weight = weight,
                //Location = location,
                CommandRuntime = mockCommandRuntime
            };

            // Action
            cmdlet.ExecuteCmdlet();

            // Assert
            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            // All the properties stay the same except the endpoints
            AssertAllProfilePropertiesDontChangeExceptEndpoints(original, actual);


            // There is an endpoint with the domain name in "actual"
            Assert.IsTrue(actual.Endpoints.Any(e => e.DomainName == domainName));
            TrafficManagerEndpoint updatedEndpoint = actual.Endpoints.First(e => e.DomainName == domainName);

            // Unchanged properties
            Assert.AreEqual(EndpointType.Any, updatedEndpoint.Type);
            Assert.AreEqual(EndpointStatus.Enabled, updatedEndpoint.Status);

            // Updated properties
            Assert.AreEqual(weight, updatedEndpoint.Weight);
            Assert.AreEqual(location, updatedEndpoint.Location);
        }

        [TestMethod]
        public void SetTrafficManagerEndpointNotExisting()
        {
            // Setup
            ProfileWithDefinition original = GetProfileWithDefinition();

            cmdlet = new SetAzureTrafficManagerEndpoint()
            {
                Name = profileName,
                DomainName = domainName,
                TrafficManagerProfile = original,
                Type = EndpointType.Any.ToString(),
                //Weight = weight,
                //Location = location,
                Status = EndpointStatus.Enabled.ToString(),
                CommandRuntime = mockCommandRuntime
            };

            // Assert the endpoint doesn't exist
            Assert.IsFalse(original.Endpoints.Any(e => e.DomainName == domainName));

            // Action
            cmdlet.ExecuteCmdlet();

            ProfileWithDefinition actual = mockCommandRuntime.OutputPipeline[0] as ProfileWithDefinition;

            // There is a new endpoint with the domain name in "actual"
            Assert.IsTrue(actual.Endpoints.Any(e => e.DomainName == domainName));
            TrafficManagerEndpoint newEndpoint = actual.Endpoints.First(e => e.DomainName == domainName);

            Assert.AreEqual(EndpointType.Any, newEndpoint.Type);
            Assert.AreEqual(EndpointStatus.Enabled, newEndpoint.Status);
            Assert.AreEqual(weight, newEndpoint.Weight);
            Assert.AreEqual(location, newEndpoint.Location);
        }

        /// <summary>
        /// The Type of the endpoint is a required field for new endpoints. Since it's not provided in the arguments
        /// to the cmdlet, the cmdlet fails.
        /// </summary>
        [TestMethod]
        public void SetTrafficManagerEndpointNotExistingMissinTypeFails()
        {
            // Setup
            ProfileWithDefinition original = GetProfileWithDefinition();

            cmdlet = new SetAzureTrafficManagerEndpoint()
            {
                Name = profileName,
                DomainName = domainName,
                TrafficManagerProfile = original,
                //Weight = weight,
                //Location = location,
                Status = EndpointStatus.Enabled.ToString(),
                CommandRuntime = mockCommandRuntime
            };

            // Assert the endpoint doesn't exist
            Assert.IsFalse(original.Endpoints.Any(e => e.DomainName == domainName));

            // Action + Assert
            Testing.AssertThrows<Exception>(() => cmdlet.ExecuteCmdlet());
        }

        /// <summary>
        /// The Type of the endpoint is a required field for new endpoints. Since it's not provided in the arguments
        /// to the cmdlet, the cmdlet fails.
        /// </summary>
        [TestMethod]
        public void SetTrafficManagerEndpointNotExistingMissingStatusFails()
        {
            // Setup
            ProfileWithDefinition original = GetProfileWithDefinition();

            cmdlet = new SetAzureTrafficManagerEndpoint()
            {
                Name = profileName,
                DomainName = domainName,
                TrafficManagerProfile = original,
                //Weight = weight,
                //Location = location,
                Type = EndpointType.Any.ToString(),
                CommandRuntime = mockCommandRuntime
            };

            // Assert the endpoint doesn't exist
            Assert.IsFalse(original.Endpoints.Any(e => e.DomainName == domainName));

            // Action + Assert
            Testing.AssertThrows<Exception>(() => cmdlet.ExecuteCmdlet());
        }

        private ProfileWithDefinition GetProfileWithDefinition()
        {
            return new ProfileWithDefinition()
            {
                DomainName = profileDomainName,
                Name = profileName,
                Endpoints = new List<TrafficManagerEndpoint>(),
                LoadBalancingMethod = loadBalancingMethod,
                MonitorPort = 80,
                Status = ProfileDefinitionStatus.Enabled,
                MonitorRelativePath = "/",
                TimeToLiveInSeconds = 30
            };
        }

        private void AssertAllProfilePropertiesDontChangeExceptEndpoints(
            ProfileWithDefinition original,
            ProfileWithDefinition actual)
        {
            Assert.AreEqual(original.DomainName, actual.DomainName);
            Assert.AreEqual(original.Name, actual.Name);
            Assert.AreEqual(original.LoadBalancingMethod, actual.LoadBalancingMethod);
            Assert.AreEqual(original.MonitorPort, actual.MonitorPort);
            Assert.AreEqual(original.Status, actual.Status);
            Assert.AreEqual(original.MonitorRelativePath, actual.MonitorRelativePath);
            Assert.AreEqual(original.TimeToLiveInSeconds, actual.TimeToLiveInSeconds);
        }

    }

}
