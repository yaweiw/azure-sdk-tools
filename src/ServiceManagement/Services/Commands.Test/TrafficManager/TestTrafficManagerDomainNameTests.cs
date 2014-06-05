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

namespace Microsoft.WindowsAzure.Commands.Test.TrafficManager
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.TrafficManager;
    using Microsoft.WindowsAzure.Commands.TrafficManager.Utilities;
    using Moq;
    using System.Management.Automation;

    [TestClass]
    public class TestTrafficManagerDomainNameTests
    {
        private const string ProfileDomainName = "my.profile.trafficmanager.net";

        private Mock<ICommandRuntime> mockCommandRuntime;

        private TestAzureTrafficManagerDomainName cmdlet;

        private Mock<ITrafficManagerClient> clientMock;

        [TestInitialize]
        public void TestSetup()
        {
            mockCommandRuntime = new Mock<ICommandRuntime>();
            clientMock = new Mock<ITrafficManagerClient>();
        }

        [TestMethod]
        public void TestProfileDomainNameReturnsTrue()
        {
            // Setup
            clientMock.Setup(c => c.TestDomainAvailability(ProfileDomainName)).Returns(true);
            cmdlet = new TestAzureTrafficManagerDomainName
                {
                    DomainName = ProfileDomainName,
                    CommandRuntime = mockCommandRuntime.Object,
                    TrafficManagerClient = clientMock.Object
                };

            // Action
            cmdlet.ExecuteCmdlet();

            // Assert
            clientMock.Verify(c => c.TestDomainAvailability(ProfileDomainName), Times.Once());
            mockCommandRuntime.Verify(c => c.WriteObject(true), Times.Once());
        }

        [TestMethod]
        public void TestProfileDomainNameReturnsFalse()
        {
            // Setup
            clientMock.Setup(c => c.TestDomainAvailability(ProfileDomainName)).Returns(false);
            cmdlet = new TestAzureTrafficManagerDomainName
                {
                    DomainName = ProfileDomainName,
                    CommandRuntime = mockCommandRuntime.Object,
                    TrafficManagerClient = clientMock.Object
                };

            // Action
            cmdlet.ExecuteCmdlet();

            // Assert
            clientMock.Verify(c => c.TestDomainAvailability(ProfileDomainName), Times.Once());
            mockCommandRuntime.Verify(c => c.WriteObject(false), Times.Once());
        }
    }
}
