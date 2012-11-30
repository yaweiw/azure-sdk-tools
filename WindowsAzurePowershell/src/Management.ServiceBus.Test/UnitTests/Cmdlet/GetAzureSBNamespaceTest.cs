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
    using System;
    using System.Management.Automation;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.CloudService.Test;
    using Microsoft.WindowsAzure.Management.CloudService.Test.Utilities;
    using Microsoft.WindowsAzure.Management.ServiceBus.Cmdlet;
    using Microsoft.WindowsAzure.Management.ServiceBus.Properties;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GetAzureSBNamespaceTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void GetAzureSBNamesapceSuccessfull()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            FakeWriter writer = new FakeWriter();
            string name = "test";
            GetAzureSBNamespaceCommand cmdlet = new GetAzureSBNamespaceCommand(channel) { Name = name, Writer = writer };
            Namespace expected = new Namespace { Name = name };
            channel.GetNamespaceThunk = gn => { return expected; };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            Namespace actual = writer.OutputChannel[0] as Namespace;
            Assert.AreEqual<string>(expected.Name, actual.Name);
        }

        [TestMethod]
        public void GetAzureSBNamesapceWithNotExistingNameFail()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            FakeWriter writer = new FakeWriter();
            string expected = Resources.ServiceBusNamespaceMissingMessage;
            GetAzureSBNamespaceCommand cmdlet = new GetAzureSBNamespaceCommand(channel) { Name = "not exiting name", Writer = writer };
            channel.GetNamespaceThunk = gn => {  throw new Exception("Internal Server Error"); };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            ErrorRecord error = writer.ErrorChannel[0] as ErrorRecord;
            Assert.AreEqual<string>(expected, error.Exception.Message);
        }

        [TestMethod]
        public void ListNamespacesSuccessfull()
        {
            // Setup
            SimpleServiceManagement channel = new SimpleServiceManagement();
            FakeWriter writer = new FakeWriter();
            string name1 = "test1";
            string name2 = "test2";
            GetAzureSBNamespaceCommand cmdlet = new GetAzureSBNamespaceCommand(channel) { Writer = writer };
            NamespaceList expected = new NamespaceList();
            expected.Add(new Namespace { Name = name1 });
            expected.Add(new Namespace { Name = name2 });
            channel.ListNamespacesThunk = gn => { return expected; };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            NamespaceList actual = writer.OutputChannel[0] as NamespaceList;
            Assert.AreEqual<int>(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual<string>(expected[i].Name, actual[i].Name);
            }
        }
    }
}