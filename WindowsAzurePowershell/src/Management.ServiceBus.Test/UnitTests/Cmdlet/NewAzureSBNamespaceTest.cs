// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.ServiceBus.Test.UnitTests.Cmdlet
{
    using System.Management.Automation;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.CloudService.Test;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using Microsoft.WindowsAzure.Management.ServiceBus.Cmdlet;
    using Microsoft.WindowsAzure.Management.ServiceBus.Properties;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class NewAzureSBNamespaceTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void NewAzureSBNamespaceSuccessfull()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            FakeWriter writer = new FakeWriter();
            string name = "test";
            string location = "location";
            NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand(channel) { Name = name, Location = location, Writer = writer };
            ServiceBusNamespace expected = new ServiceBusNamespace { Name = name, Region = location };
            channel.CreateServiceBusNamespaceThunk = csbn => { return expected; };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ServiceBusNamespace actual = writer.OutputChannel[0] as ServiceBusNamespace;
            Assert.AreEqual<ServiceBusNamespace>(expected, actual);
        }

        [TestMethod]
        public void NewAzureSBNamespaceWithInvalidNamesFail()
        {
            // Setup
            string[] invalidNames = { "1test", "test#", "test invaid", "-test", "_test" };

            foreach (string invalidName in invalidNames)
            {
                FakeWriter writer = new FakeWriter();
                NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand() { Name = invalidName, Location = "location", Writer = writer };
                ArgumentException expected = new ArgumentException(string.Format(Resources.InvalidNamespaceName, invalidName), "Name");

                // Test
                cmdlet.ExecuteCmdlet();

                // Assert
                ErrorRecord actual = writer.ErrorChannel[0];
                Assert.AreEqual<string>(expected.Message, actual.Exception.Message);
            }
        }

        [TestMethod]
        public void NewAzureSBNamespaceWithInternalServerError()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            FakeWriter writer = new FakeWriter();
            string name = "test";
            string location = "location";
            NewAzureSBNamespaceCommand cmdlet = new NewAzureSBNamespaceCommand(channel) { Name = name, Location = location, Writer = writer };
            string expected = Resources.NewNamespaceErrorMessage;
            channel.CreateServiceBusNamespaceThunk = csbn => { throw new Exception(Resources.InternalServerErrorMessage); };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ErrorRecord actual = writer.ErrorChannel[0];
            Assert.AreEqual<string>(expected, actual.Exception.Message);
        }
    }
}