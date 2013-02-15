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
    using System.Management.Automation;
    using Microsoft.WindowsAzure.Management.Store.Cmdlet;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.ServiceManagement.Marketplace.ResourceModel;
    using Microsoft.WindowsAzure.Management.Store.Model;
    using Moq;

    [TestClass]
    public class GetAzureStoreAvailableAddOnTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
            new FileSystemHelper(this).CreateAzureSdkDirectoryAndImportPublishSettings();
        }

        [TestMethod]
        public void GetAzureStoreAvailableAddOnSuccessfull()
        {
            // Setup
            Mock<ICommandRuntime> mockCommandRuntime = new Mock<ICommandRuntime>();
            List<Plan> plans = new List<Plan>();
            plans.Add(new Plan() { PlanIdentifier = "Bronze" });
            plans.Add(new Plan() { PlanIdentifier = "Silver" });
            plans.Add(new Plan() { PlanIdentifier = "Gold" });
            plans.Add(new Plan() { PlanIdentifier = "Silver" });
            plans.Add(new Plan() { PlanIdentifier = "Gold" });

            List<Offer> expectedOffers = new List<Offer>()
            {
                new Offer() { ProviderIdentifier = "Microsoft", OfferIdentifier = "Bing Translate" },
                new Offer() { ProviderIdentifier = "NotExistingCompany", OfferIdentifier = "Not Existing Name" },
                new Offer() { ProviderIdentifier = "OneSDKCompany", OfferIdentifier = "Windows Azure PowerShell" }
            };
            List<WindowsAzureOffer> expectedWindowsAzureOffers = new List<WindowsAzureOffer>();
            expectedOffers.ForEach(o => expectedWindowsAzureOffers.Add(new WindowsAzureOffer(o, plans)));

            Mock<StoreClient> mock = new Mock<StoreClient>();
            mock.Setup(f => f.GetAvailableWindowsAzureAddOns(It.IsAny<string>())).Returns(expectedWindowsAzureOffers);

            GetAzureStoreAvailableAddOnCommand cmdlet = new GetAzureStoreAvailableAddOnCommand()
            {
                StoreClient = mock.Object,
                CommandRuntime = mockCommandRuntime.Object
            };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mock.Verify(f => f.GetAvailableWindowsAzureAddOns("US"), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expectedWindowsAzureOffers, true), Times.Once());
        }
    }
}