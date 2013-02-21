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
    using Microsoft.Samples.WindowsAzure.ServiceManagement;
    using Microsoft.WindowsAzure.Management.Cmdlets.Common;
    using Microsoft.WindowsAzure.Management.Store.Model;
    using Microsoft.WindowsAzure.Management.Store.Properties;

    /// <summary>
    /// Gets all available Windows Azure add-ons from Marketplace and gets user purchased add-ons.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureStoreAddOn"), OutputType(typeof(List<PSObject>), typeof(List<WindowsAzureOffer>))]
    public class GetAzureStoreAddOnCommand : CloudBaseCmdlet<IServiceManagement>
    {
        const string ShortType = "Microsoft.WindowsAzure.Management.Store.Model.AddOnWindowsAzureAddOn.Short";

        const string ListAvailableParameterSet = "ListAvailable";

        const string GetAddOnParameterSet = "GetAddOn";

        public StoreClient StoreClient { get; set; }

        public MarketplaceClient MarketplaceClient { get; set; }

        [Parameter(Position = 0, Mandatory = true, ValueFromPipelineByPropertyName = true, 
            ParameterSetName = ListAvailableParameterSet, HelpMessage = "List available add-ons")]
        public SwitchParameter ListAvailable { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            ParameterSetName = ListAvailableParameterSet, HelpMessage = "Country code")]
        [ValidateCountryLengthAttribute()]
        public string Country { get; set; }

        [Parameter(Position = 0, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            ParameterSetName = GetAddOnParameterSet, HelpMessage = "Add-On name")]
        public string Name { get; set; }

        [Parameter(Position = 1, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            ParameterSetName = GetAddOnParameterSet, HelpMessage = "Add-On provider")]
        public string Provider { get; set; }

        [Parameter(Position = 2, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            ParameterSetName = GetAddOnParameterSet, HelpMessage = "Add-On location")]
        public string Location { get; set; }

        [Parameter(Position = 3, Mandatory = false, ValueFromPipelineByPropertyName = true, 
            ParameterSetName = GetAddOnParameterSet, HelpMessage = "Show add-on details")]
        public SwitchParameter Detailed { get; set; }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public override void ExecuteCmdlet()
        {
            if (ListAvailable.IsPresent)
	        {
		        ListAvailableAddOns();
	        }
            else
	        {
                GetAddOn();
	        }
        }

        private void GetAddOn()
        {
 	        StoreClient = StoreClient ?? new StoreClient(
                CurrentSubscription.SubscriptionId,
                ServiceEndpoint,
                CurrentSubscription.Certificate,
                text => this.WriteDebug(text),
                Channel);
            List<WindowsAzureAddOn> addOns = StoreClient.GetAddOn(new AddOnSearchOptions(Name, Provider, Location));

            if (Detailed.IsPresent)
            {
                WriteObject(addOns, true);
            }
            else
            {
                List<PSObject> shortObjects = new List<PSObject>();
                addOns.ForEach(a =>
                {
                    PSObject psObject = new PSObject(a);
                    psObject.TypeNames.Add(ShortType);
                    psObject.TypeNames.Remove(typeof(WindowsAzureAddOn).FullName);
                    shortObjects.Add(psObject);
                });
                WriteObject(shortObjects, true);
            }
        }

        private void ListAvailableAddOns()
        {
 	        LocationList locations = Channel.ListLocations(CurrentSubscription.SubscriptionId);
            MarketplaceClient = MarketplaceClient ?? 
                new MarketplaceClient(locations.Select<Location, string>(l => l.Name));

            WriteVerbose(Resources.GetAllAddOnsWaitMessage);
            List<WindowsAzureOffer> result = MarketplaceClient.GetAvailableWindowsAzureOffers(Country);
            List<WindowsAzureOffer> knownProviders = result.Where<WindowsAzureOffer>(
                o => MarketplaceClient.IsKnownProvider(o.ProviderId)).ToList<WindowsAzureOffer>();
            WriteObject(knownProviders, true);
        }
    }
}