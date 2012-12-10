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
    public class RemoveAzureSBNamespaceTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void RemoveAzureSBNamespaceSuccessfull()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            FakeWriter writer = new FakeWriter();
            string name = "test";
            RemoveAzureSBNamespaceCommand cmdlet = new RemoveAzureSBNamespaceCommand(channel) { Name = name, Writer = writer };
            bool deleted = false;
            string expectedVerbose = string.Format(Resources.RemovingNamespaceMessage, name);
            channel.DeleteServiceBusNamespaceThunk = dsbn => { deleted = true; };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            string actual = writer.VerboseChannel[0] as string;
            Assert.IsTrue(deleted);
            Assert.AreEqual<string>(expectedVerbose, actual);
        }

        [TestMethod]
        public void RemoveAzureSBNamespaceWithInvalidNamesFail()
        {
            // Setup
            string[] invalidNames = { "1test", "test#", "test invaid", "-test", "_test" };

            foreach (string invalidName in invalidNames)
            {
                FakeWriter writer = new FakeWriter();
                RemoveAzureSBNamespaceCommand cmdlet = new RemoveAzureSBNamespaceCommand() { Name = invalidName, Writer = writer };
                ArgumentException expected = new ArgumentException(string.Format(Resources.InvalidNamespaceName, invalidName), "Name");

                // Test
                cmdlet.ExecuteCmdlet();

                // Assert
                ErrorRecord actual = writer.ErrorChannel[0];
                Assert.AreEqual<string>(expected.Message, actual.Exception.Message);
            }
        }

        [TestMethod]
        public void RemoveAzureSBNamespaceWithInternalServerError()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            FakeWriter writer = new FakeWriter();
            string name = "test";
            RemoveAzureSBNamespaceCommand cmdlet = new RemoveAzureSBNamespaceCommand(channel) { Name = name, Writer = writer };
            string expected = Resources.RemoveNamespaceErrorMessage;
            channel.DeleteServiceBusNamespaceThunk = dsbn => { throw new Exception(Resources.InternalServerErrorMessage); };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ErrorRecord actual = writer.ErrorChannel[0];
            Assert.AreEqual<string>(expected, actual.Exception.Message);
        }
    }
}