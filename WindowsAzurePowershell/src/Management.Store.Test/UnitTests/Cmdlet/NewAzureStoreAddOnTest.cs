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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using Microsoft.WindowsAzure.Management.Store.Cmdlet;
    using Microsoft.WindowsAzure.Management.Store.Model;
    using Microsoft.WindowsAzure.Management.Store.Model.ResourceModel;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class NewAzureStoreAddOnTests : TestBase
    {
        Mock<ICommandRuntime> mockCommandRuntime;

        Mock<StoreClient> mockStoreClient;

        Mock<PSHost> mockHost;

        NewAzureStoreAddOnCommand cmdlet;

        List<PSObject> actual;

        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
            mockCommandRuntime = new Mock<ICommandRuntime>();
            mockStoreClient = new Mock<StoreClient>();
            mockHost = new Mock<PSHost>();
            cmdlet = new NewAzureStoreAddOnCommand()
            {
                StoreClient = mockStoreClient.Object,
                CommandRuntime = mockCommandRuntime.Object
            };
        }

        [TestMethod]
        public void NewAzureStoreAddOnWithSuccessful()
        {
            // Setup
            bool expected = true;
            WindowsAzureAddOn addon;
            mockHost.Setup(f => f.UI.PromptForChoice(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Collection<ChoiceDescription>>(), It.IsAny<int>())).Returns(0);
            mockStoreClient.Setup(f => f.TryGetAddOn(It.IsAny<string>(), out addon)).Returns(true);

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            //mockStoreClient.Verify(f => f.GetAddOn(new AddOnSearchOptions(null, null, null)), Times.Once());
            //mockCommandRuntime.Verify(f => f.WriteObject(expected, true), Times.Once());
        }
    }
}