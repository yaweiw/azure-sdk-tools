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
        Mock<ICommandRuntime> mockCommandRuntime;

        Mock<StoreClient> mockStoreClient;

        GetAzureStoreAddOnCommand cmdlet;

        List<PSObject> actual;

        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
            mockCommandRuntime = new Mock<ICommandRuntime>();
            mockStoreClient = new Mock<StoreClient>();
            cmdlet = new GetAzureStoreAddOnCommand()
            {
                StoreClient = mockStoreClient.Object,
                CommandRuntime = mockCommandRuntime.Object
            };
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithEmptyCloudService()
        {
            // Setup
            List<WindowsAzureAddOn> expected = new List<WindowsAzureAddOn>();
            mockStoreClient.Setup(f => f.GetAddOn(It.IsAny<AddOnSearchOptions>())).Returns(expected);

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
            List<WindowsAzureAddOn> expected = new List<WindowsAzureAddOn>()
            { 
                new WindowsAzureAddOn(new Resource() { Name = "BingSearchAddOn" }, "West US"),
                new WindowsAzureAddOn(new Resource() { Name = "BingTranslateAddOn" }, "West US")
            };
            mockCommandRuntime.Setup(f => f.WriteObject(It.IsAny<object>(), true))
                .Callback<object, bool>((o, b) => actual = (List<PSObject>)o);
            mockStoreClient.Setup(f => f.GetAddOn(It.IsAny<AddOnSearchOptions>())).Returns(expected);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions(null, null, null)), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(It.IsAny<object>(), true), Times.Once());
            Assert.AreEqual<int>(expected.Count, actual.Count);
            Assert.IsTrue(AddOnListEqualsPSObjectList(expected, actual));
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithNameFilter()
        {
            // Setup
            List<WindowsAzureAddOn> expected = new List<WindowsAzureAddOn>()
            {
                new WindowsAzureAddOn(new Resource() { Name = "BingTranslateAddOn" }, "West US")
            };
            mockCommandRuntime.Setup(f => f.WriteObject(It.IsAny<object>(), true))
                .Callback<object, bool>((o, b) => actual = (List<PSObject>)o);
            mockStoreClient.Setup(f => f.GetAddOn(new AddOnSearchOptions("BingTranslateAddOn", null, null)))
                .Returns(expected);
            cmdlet.Name = "BingTranslateAddOn";

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(
                f => f.GetAddOn(new AddOnSearchOptions("BingTranslateAddOn", null, null)),
                Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(It.IsAny<object>(), true), Times.Once());
            Assert.AreEqual<int>(expected.Count, actual.Count);
            Assert.IsTrue(AddOnListEqualsPSObjectList(expected, actual));
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithLocationFilter()
        {
            // Setup
            List<WindowsAzureAddOn> expected = new List<WindowsAzureAddOn>()
            {
                new WindowsAzureAddOn(new Resource() { Name = "BingSearchAddOn" }, "West US"),
                new WindowsAzureAddOn(new Resource() { Name = "MongoDB" }, "West US"),
                new WindowsAzureAddOn(new Resource() { Name = "BingTranslateAddOn" }, "West US")
            };
            mockCommandRuntime.Setup(f => f.WriteObject(It.IsAny<object>(), true))
                .Callback<object, bool>((o, b) => actual = (List<PSObject>)o);
            mockStoreClient.Setup(f => f.GetAddOn(new AddOnSearchOptions(null, null, "West US"))).Returns(expected);
            cmdlet.Location = "West US";

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions(null, null, "West US")), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(It.IsAny<object>(), true), Times.Once());
            Assert.AreEqual<int>(expected.Count, actual.Count);
            Assert.IsTrue(AddOnListEqualsPSObjectList(expected, actual));
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithProviderFilter()
        {
            // Setup
            List<WindowsAzureAddOn> expected = new List<WindowsAzureAddOn>()
            {
                new WindowsAzureAddOn(new Resource() { 
                    Name = "BingSearchAddOn", 
                    ResourceProviderNamespace = "Microsoft" }, 
                    "West US"),
                new WindowsAzureAddOn(new Resource() { 
                    Name = "BingTranslateAddOn", 
                    ResourceProviderNamespace = "Microsoft" }, 
                    "West US")
            };
            mockCommandRuntime.Setup(f => f.WriteObject(It.IsAny<object>(), true))
                .Callback<object, bool>((o, b) => actual = (List<PSObject>)o);
            mockStoreClient.Setup(f => f.GetAddOn(new AddOnSearchOptions(null, "Microsoft", null))).Returns(expected);
            cmdlet.Provider = "Microsoft";

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions(null, "Microsoft", null)), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(It.IsAny<object>(), true), Times.Once());
            Assert.AreEqual<int>(expected.Count, actual.Count);
            Assert.IsTrue(AddOnListEqualsPSObjectList(expected, actual));
        }

        [TestMethod]
        public void GetAzureStoreAddOnWithCompleteSearchOptions()
        {
            // Setup
            List<WindowsAzureAddOn> expected = new List<WindowsAzureAddOn>()
            {
                new WindowsAzureAddOn(new Resource() { 
                    Name = "BingSearchAddOn", 
                    ResourceProviderNamespace = "Microsoft" }, 
                    "West US")
            };
            mockCommandRuntime.Setup(f => f.WriteObject(It.IsAny<object>(), true))
                .Callback<object, bool>((o, b) => actual = (List<PSObject>)o);
            mockStoreClient.Setup(f => f.GetAddOn(new AddOnSearchOptions("BingSearchAddOn", "Microsoft", "West US")))
                .Returns(expected);
            cmdlet.Provider = "Microsoft";
            cmdlet.Name = "BingSearchAddOn";
            cmdlet.Location = "West US";

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(
                f => f.GetAddOn(new AddOnSearchOptions("BingSearchAddOn", "Microsoft", "West US")), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(It.IsAny<object>(), true), Times.Once());
            Assert.AreEqual<int>(expected.Count, actual.Count);
            Assert.IsTrue(AddOnListEqualsPSObjectList(expected, actual));
        }

        private bool AddOnListEqualsPSObjectList(List<WindowsAzureAddOn> expectedList, List<PSObject> actualList)
        {
            bool equals = true;

            for (int i = 0; i < expectedList.Count && equals; i++)
            {
                WindowsAzureAddOn expected = expectedList[i];
                WindowsAzureAddOn actual = (WindowsAzureAddOn)actualList[i].BaseObject;

                equals &= expected.Name == actual.Name;
            }

            return equals;
        }
    }
}