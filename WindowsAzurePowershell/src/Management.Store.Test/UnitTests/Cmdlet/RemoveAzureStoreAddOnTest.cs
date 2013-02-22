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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using Microsoft.WindowsAzure.Management.Store.Cmdlet;
    using Microsoft.WindowsAzure.Management.Store.Model;
    using Microsoft.WindowsAzure.Management.Store.Model.ResourceModel;
    using Microsoft.WindowsAzure.Management.Store.Properties;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RemoveAzureStoreAddOnTests : TestBase
    {
        Mock<ICommandRuntime> mockCommandRuntime;

        Mock<StoreClient> mockStoreClient;

        Mock<PowerShellCustomConfirmation> mockConfirmation;

        RemoveAzureStoreAddOnCommand cmdlet;

        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
            mockCommandRuntime = new Mock<ICommandRuntime>();
            mockStoreClient = new Mock<StoreClient>();
            mockConfirmation = new Mock<PowerShellCustomConfirmation>();
            cmdlet = new RemoveAzureStoreAddOnCommand()
            {
                StoreClient = mockStoreClient.Object,
                CommandRuntime = mockCommandRuntime.Object,
                CustomConfirmation = mockConfirmation.Object
            };
        }

        [TestMethod]
        public void RemoveAzureStoreAddOnWithSuccessful()
        {
            // Setup
            bool expected = true;
            string name = "TestAddOn";
            string addonId = "Search";
            string message = "Expected message for remove";
            cmdlet.Name = name;
            WindowsAzureAddOn addon = new WindowsAzureAddOn(new Resource() { Type = addonId }, "West US", null);
            mockConfirmation.Setup(f => f.ShouldProcess(Resources.RemoveAddOnConformation, message)).Returns(true);
            mockStoreClient.Setup(f => f.RemoveAddOn(name));
            mockStoreClient.Setup(f => f.GetConfirmationMessage(OperationType.Remove, null, null)).Returns(message);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.RemoveAddOn(name), Times.Once());
            mockConfirmation.Verify(f => f.ShouldProcess(Resources.RemoveAddOnConformation, message), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected), Times.Never());
        }

        [TestMethod]
        public void RemoveAzureStoreAddOnWithPassThru()
        {
            // Setup
            bool expected = true;
            string name = "TestAddOn";
            string addonId = "Search";
            string message = "Expected message for remove";
            cmdlet.Name = name;
            cmdlet.PassThru = true;
            WindowsAzureAddOn addon = new WindowsAzureAddOn(new Resource() { Type = addonId }, "West US", null);
            mockConfirmation.Setup(f => f.ShouldProcess(Resources.RemoveAddOnConformation, message)).Returns(true);
            mockStoreClient.Setup(f => f.TryGetAddOn(name, out addon)).Returns(true);
            mockStoreClient.Setup(f => f.RemoveAddOn(name));
            mockStoreClient.Setup(f => f.GetConfirmationMessage(OperationType.Remove, null, null)).Returns(message);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.RemoveAddOn(name), Times.Once());
            mockConfirmation.Verify(f => f.ShouldProcess(Resources.RemoveAddOnConformation, message), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected), Times.Once());
        }

        [TestMethod]
        public void RemoveAzureStoreAddOnWithNo()
        {
            // Setup
            bool expected = true;
            string name = "TestAddOn";
            string addonId = "Search";
            string message = "Expected message for remove";
            cmdlet.Name = name;
            WindowsAzureAddOn addon = new WindowsAzureAddOn(new Resource() { Type = addonId }, "West US", null);
            mockConfirmation.Setup(f => f.ShouldProcess(Resources.RemoveAddOnConformation, message)).Returns(false);
            mockStoreClient.Setup(f => f.TryGetAddOn(name, out addon)).Returns(true);
            mockStoreClient.Setup(f => f.RemoveAddOn(name));
            mockStoreClient.Setup(f => f.GetConfirmationMessage(OperationType.Remove, null, null)).Returns(message);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mockStoreClient.Verify(f => f.RemoveAddOn(name), Times.Never());
            mockConfirmation.Verify(f => f.ShouldProcess(Resources.RemoveAddOnConformation, message), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expected), Times.Never());
        }
    }
}