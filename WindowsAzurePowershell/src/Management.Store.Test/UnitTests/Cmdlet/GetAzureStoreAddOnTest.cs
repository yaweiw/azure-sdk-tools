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

namespace Microsoft.WindowsAzure.Management.Store.Test.UnitTests.Cmdlet
{
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Store.Contract;
    using Microsoft.WindowsAzure.Management.Store.Cmdlet;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;
    using System;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Store.ResourceModel;
    using System.Management.Automation;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Management.Store.Model;

    [TestClass]
    public class GetAzureStoreAddOnTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithEmptyCloudService()
        {
            // Setup
            Mock<ICommandRuntime> mockCommandRuntime = new Mock<ICommandRuntime>();
            Mock<StoreClient> mockStoreClient = new Mock<StoreClient>();
            List<AddOn> expected = new List<AddOn>();
            mockStoreClient.Setup(f => f.GetAddOn(It.IsAny<AddOnSearchOptions>())).Returns(new List<AddOn>());
            GetAzureStoreAddOnCommand cmdlet = new GetAzureStoreAddOnCommand() { StoreClient = mockStoreClient.Object, CommandRuntime = mockCommandRuntime.Object };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions(null, null, null)), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected, true), Times.Once());
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithoutSearchOptions()
        {
            // Setup
            Mock<ICommandRuntime> mockCommandRuntime = new Mock<ICommandRuntime>();
            Mock<StoreClient> mockStoreClient = new Mock<StoreClient>();
            List<AddOn> expected = new List<AddOn>();
            expected.Add(new AddOn(new Resource() { Name = "BingSearchAddOn" }, "West US"));
            expected.Add(new AddOn(new Resource() { Name = "BingTranslateAddOn" }, "West US"));
            mockStoreClient.Setup(f => f.GetAddOn(It.IsAny<AddOnSearchOptions>())).Returns(expected);
            GetAzureStoreAddOnCommand cmdlet = new GetAzureStoreAddOnCommand() { StoreClient = mockStoreClient.Object, CommandRuntime = mockCommandRuntime.Object };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions(null, null, null)), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected, true), Times.Once());
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithNameFilter()
        {
            // Setup
            Mock<ICommandRuntime> mockCommandRuntime = new Mock<ICommandRuntime>();
            Mock<StoreClient> mockStoreClient = new Mock<StoreClient>();
            List<AddOn> expected = new List<AddOn>();
            expected.Add(new AddOn(new Resource() { Name = "BingTranslateAddOn" }, "West US"));
            mockStoreClient.Setup(f => f.GetAddOn(new AddOnSearchOptions("BingTranslateAddOn", null, null))).Returns(expected);
            GetAzureStoreAddOnCommand cmdlet = new GetAzureStoreAddOnCommand() { 
                StoreClient = mockStoreClient.Object,
                CommandRuntime = mockCommandRuntime.Object,
                Name = "BingTranslateAddOn"
            };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions("BingTranslateAddOn", null, null)), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected), Times.Once());
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithLocationFilter()
        {
            // Setup
            Mock<ICommandRuntime> mockCommandRuntime = new Mock<ICommandRuntime>();
            Mock<StoreClient> mockStoreClient = new Mock<StoreClient>();
            List<AddOn> expected = new List<AddOn>();
            expected.Add(new AddOn(new Resource() { Name = "BingSearchAddOn" }, "West US"));
            expected.Add(new AddOn(new Resource() { Name = "MongoDB" }, "West US"));
            expected.Add(new AddOn(new Resource() { Name = "BingTranslateAddOn" }, "West US"));
            mockStoreClient.Setup(f => f.GetAddOn(new AddOnSearchOptions(null, null, "West US"))).Returns(expected);
            GetAzureStoreAddOnCommand cmdlet = new GetAzureStoreAddOnCommand() { 
                StoreClient = mockStoreClient.Object,
                CommandRuntime = mockCommandRuntime.Object,
                Location = "West US"
            };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions(null, null, "West US")), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected, true), Times.Once());
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithProviderFilter()
        {
            // Setup
            Mock<ICommandRuntime> mockCommandRuntime = new Mock<ICommandRuntime>();
            Mock<StoreClient> mockStoreClient = new Mock<StoreClient>();
            List<AddOn> expected = new List<AddOn>();
            expected.Add(new AddOn(new Resource() { Name = "BingSearchAddOn", ResourceProviderNamespace = "Microsoft" }, "West US"));
            expected.Add(new AddOn(new Resource() { Name = "BingTranslateAddOn", ResourceProviderNamespace = "Microsoft" }, "West US"));
            mockStoreClient.Setup(f => f.GetAddOn(new AddOnSearchOptions(null, "Microsoft", null))).Returns(expected);
            GetAzureStoreAddOnCommand cmdlet = new GetAzureStoreAddOnCommand()
            {
                StoreClient = mockStoreClient.Object,
                CommandRuntime = mockCommandRuntime.Object,
                Provider = "Microsoft"
            };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions(null, "Microsoft", null)), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected, true), Times.Once());
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithCompleteSearchOptions()
        {
            // Setup
            Mock<ICommandRuntime> mockCommandRuntime = new Mock<ICommandRuntime>();
            Mock<StoreClient> mockStoreClient = new Mock<StoreClient>();
            List<AddOn> expected = new List<AddOn>();
            expected.Add(new AddOn(new Resource() { Name = "BingSearchAddOn", ResourceProviderNamespace = "Microsoft" }, "West US"));
            mockStoreClient.Setup(f => f.GetAddOn(new AddOnSearchOptions("BingSearchAddOn", "Microsoft", "West US"))).Returns(expected);
            GetAzureStoreAddOnCommand cmdlet = new GetAzureStoreAddOnCommand()
            {
                StoreClient = mockStoreClient.Object,
                CommandRuntime = mockCommandRuntime.Object,
                Provider = "Microsoft",
                Name = "BingSearchAddOn",
                Location = "West US"
            };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions("BingSearchAddOn", "Microsoft", "West US")), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected), Times.Once());
        }
    }
}