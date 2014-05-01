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
    using System.Management.Automation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Commands.Test.Utilities.Common;
    using Microsoft.WindowsAzure.Commands.TrafficManager.Profile;
    using Microsoft.WindowsAzure.Commands.Utilities.TrafficManager;
    using Moq;

    [TestClass]
    public class DisableTrafficManagerProfileTests
    {
        private const string profileName = "my-profile";

        private Mock<ICommandRuntime> mockCommandRuntime;

        private DisableAzureTrafficManagerProfile cmdlet;

        private Mock<ITrafficManagerClient> clientMock;

        [TestInitialize]
        public void TestSetup()
        {
            mockCommandRuntime = new Mock<ICommandRuntime>();
            clientMock = new Mock<ITrafficManagerClient>();
        }

        [TestMethod]
        public void DisableProfileSucceedsNoPassThru()
        {
            clientMock.Setup(c => c.ListProfiles()).Verifiable();
            // Setup
            cmdlet = new DisableAzureTrafficManagerProfile()
            {
                Name = profileName,
                CommandRuntime = mockCommandRuntime.Object,
                TrafficManagerClient = clientMock.Object
            };

            // Action
            cmdlet.ExecuteCmdlet();

            // Assert
            clientMock.Verify(c => c.ListProfiles(), Times.Once());
            mockCommandRuntime.Verify(c => c.WriteObject(true), Times.Never());
        }

        [TestMethod]
        public void DisableProfileSucceedsPassThru()
        {
            clientMock.Setup(c => c.ListProfiles()).Verifiable();

            // Setup
            cmdlet = new DisableAzureTrafficManagerProfile()
            {
                Name = profileName,
                CommandRuntime = mockCommandRuntime.Object,
                TrafficManagerClient = clientMock.Object,
                PassThru = new SwitchParameter(true)
            };


            // Action
            cmdlet.ExecuteCmdlet();

            // Assert
            clientMock.Verify(c => c.ListProfiles(), Times.Once());
            mockCommandRuntime.Verify(c => c.WriteObject(true), Times.Once());
            Assert.AreEqual(true, (bool)((MockCommandRuntime)cmdlet.CommandRuntime).OutputPipeline[0]);
        }
    }
}
