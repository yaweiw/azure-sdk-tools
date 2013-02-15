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
    using System.Management.Automation;
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Store.Cmdlet;
    using Microsoft.WindowsAzure.Management.Store.MarketplaceServiceReference;
    using Microsoft.WindowsAzure.Management.Store.Model;
    using Microsoft.WindowsAzure.Management.Test.Stubs;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;
    using Moq;
    using VisualStudio.TestTools.UnitTesting;

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
            expectedOffers.ForEach(o => expectedWindowsAzureOffers.Add(new WindowsAzureOffer(
                o,
                plans,
                new List<string>() { "West US", "East US" })));

            Mock<MarketplaceClient> mock = new Mock<MarketplaceClient>();
            mock.Setup(f => f.GetAvailableWindowsAzureOffers(It.IsAny<string>())).Returns(expectedWindowsAzureOffers);

            Mock<IServiceManagement> mockChannel = new Mock<IServiceManagement>();
            mockChannel.Setup(
                f => f.BeginListLocations(It.IsAny<string>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()));
            mockChannel.Setup(f => f.EndListLocations(It.IsAny<IAsyncResult>()))
                .Returns(new LocationList() 
                {
                    new Location() { Name = "West US" },
                    new Location() { Name = "East US" } 
                });

            GetAzureStoreAvailableAddOnCommand cmdlet = new GetAzureStoreAvailableAddOnCommand()
            {
                MarketplaceClient = mock.Object,
                CommandRuntime = mockCommandRuntime.Object,
                Channel = mockChannel.Object
            };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            mock.Verify(f => f.GetAvailableWindowsAzureOffers(null), Times.Once());
            mockCommandRuntime.Verify(f => f.WriteObject(expectedWindowsAzureOffers, true), Times.Once());
        }
    }
}