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
        SimpleServiceManagement channel;
        FakeWriter writer;
        GetAzureSBNamespaceCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            channel = new SimpleServiceManagement();
            writer = new FakeWriter();
            cmdlet = new GetAzureSBNamespaceCommand(channel) { Writer = writer };
        }

        [TestMethod]
        public void GetAzureSBNamespaceSuccessfull()
        {
            // Setup
            string name = "test";
            cmdlet.Name = name;
            Namespace expected = new Namespace { Name = name };
            channel.GetNamespaceThunk = gn => { return expected; };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            Namespace actual = writer.OutputChannel[0] as Namespace;
            Assert.AreEqual<string>(expected.Name, actual.Name);
        }

        [TestMethod]
        public void GetAzureSBNamespaceWithNotExistingNameFail()
        {
            // Setup
            string expected = Resources.ServiceBusNamespaceMissingMessage;
            cmdlet.Name = "not existing name";
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
            string name1 = "test1";
            string name2 = "test2";
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