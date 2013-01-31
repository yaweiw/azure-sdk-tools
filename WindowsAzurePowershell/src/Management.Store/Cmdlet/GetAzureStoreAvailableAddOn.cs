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

namespace Microsoft.WindowsAzure.Management.Store.Cmdlet
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using System.Security.Permissions;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Marketplace.Contract;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.Marketplace.ResourceModel;
    using Microsoft.WindowsAzure.Management.Store.Cmdlet.Common;

    /// <summary>
    /// Create scaffolding for a new node web role, change cscfg file and csdef to include the added web role
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureStoreAvailableAddOn"), OutputType(typeof(List<PSObject>))]
    public class GetAzureStoreAvailableAddOnCommand : StoreBaseCmdlet
    {
        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Country code")]
        public string Country { get; set; }

        /// <summary>
        /// Creates new instance from GetAzureStoreAvailableAddOnCommand
        /// </summary>
        public GetAzureStoreAvailableAddOnCommand()
        {
            Country = "US";
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            List<Offer> offers = MarketplaceChannel.ListWindowsAzureOffers();
            List<PSObject> output = new List<PSObject>();

            foreach (Offer offer in offers)
            {
                string plansQuery = string.Format("CountryCode eq '{0}'", Country);
                List<Plan> plans = MarketplaceChannel.ListOfferPlans(offer.Id.ToString(), plansQuery);

                if (plans.Count > 0)
                {
                    IEnumerable<string> planIdentifiers = plans.Select<Plan, string>(p => p.PlanIdentifier).Distinct<string>();
                    string joinResult = string.Join<string>(", ", planIdentifiers);
                    PSObject obj = ConstructPSObject(null,
                        Parameter.Provider, offer.ProviderIdentifier,
                        Parameter.Addon, offer.OfferIdentifier,
                        Parameter.Plans, joinResult);

                    output.Add(obj);
                }
            }

            if (output.Count > 0)
            {
                WriteObject(output, true);                
            }
        }
    }
}