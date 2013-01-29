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
    using Microsoft.WindowsAzure.Management.Store.Test.Stubs;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Marketplace.ResourceModel;

    [TestClass]
    public class GetAzureStoreAvailableAddOnTests : TestBase
    {
        [TestInitialize]
        public void SetupTest()
        {
            Management.Extensions.CmdletSubscriptionExtensions.SessionManager = new InMemorySessionManager();
        }

        [TestMethod]
        public void GetAzureStoreAvailableAddOnSuccessfull()
        {
            // Setup
            MockCommandRuntime mockCommandRuntime = new MockCommandRuntime();
            SimpleMarketplaceManagement channel = new SimpleMarketplaceManagement();
            GetAzureStoreAvailableAddOnCommand cmdlet = new GetAzureStoreAvailableAddOnCommand() {
                CommandRuntime = mockCommandRuntime,
                MarketplaceChannel = channel
            };
            List<Plan> expectedPlans = new List<Plan>();
            expectedPlans.Add(new Plan() { PlanIdentifier = "Bronze" });
            expectedPlans.Add(new Plan() { PlanIdentifier = "Silver" });
            expectedPlans.Add(new Plan() { PlanIdentifier = "Gold" });
            expectedPlans.Add(new Plan() { PlanIdentifier = "Silver" });
            expectedPlans.Add(new Plan() { PlanIdentifier = "Gold" });
            string expectedPlansString = "Bronze, Silver, Gold";

            List<Offer> expectedOffers = new List<Offer>();
            expectedOffers.Add(new Offer() { ProviderIdentifier = "Microsoft", OfferIdentifier = "Bing Translate" });
            expectedOffers.Add(new Offer() { ProviderIdentifier = "NotExistingCompany", OfferIdentifier = "Not Existing Name" });
            expectedOffers.Add(new Offer() { ProviderIdentifier = "OneSDKCompany", OfferIdentifier = "Windows Azure PowerShell" });

            channel.ListWindowsAzureOffersThunk = lwao => { return expectedOffers; };
            channel.ListOfferPlansThunk = lop => { return expectedPlans; };

            // Test
            cmdlet.ExecuteCmdlet();

            // Assert
            List<PSObject> actual = mockCommandRuntime.OutputPipeline[0] as List<PSObject>;
            Assert.AreEqual<int>(expectedOffers.Count, actual.Count);

            for (int i = 0; i < expectedOffers.Count; i++)
            {
                Assert.AreEqual<string>(expectedOffers[i].ProviderIdentifier, actual[i].GetVariableValue<string>("Provider"));
                Assert.AreEqual<string>(expectedOffers[i].OfferIdentifier, actual[i].GetVariableValue<string>("Addon"));
                Assert.AreEqual<string>(expectedPlansString, actual[i].GetVariableValue<string>("Plans"));
            }
        }
    }
}