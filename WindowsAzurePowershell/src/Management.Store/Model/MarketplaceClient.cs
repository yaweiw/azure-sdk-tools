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

namespace Microsoft.WindowsAzure.Management.Store.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Linq;
    using Microsoft.WindowsAzure.Management.Store.MarketplaceServiceReference;
    using Microsoft.WindowsAzure.Management.Store.Properties;

    public class MarketplaceClient
    {
        public List<string> SubscriptionLocations { get; private set; }

        private const string DataMarket = "DataMarket";

        public MarketplaceClient(IEnumerable<string> subscriptionLocations)
        {
            SubscriptionLocations = new List<string>(subscriptionLocations);
        }

        /// <summary>
        /// Lists all available Windows Azure offers in the Marketplace.
        /// </summary>
        /// <param name="country">The country code</param>
        /// <returns>The available Windows Azure offers in Marketplace</returns>
        //public virtual List<WindowsAzureOffer> GetAvailableWindowsAzureAddOns(string country)
        //{
        //    List<WindowsAzureOffer> result = new List<WindowsAzureOffer>();
        //    List<Offer> offers = marketplaceChannel.ListWindowsAzureOffers(country);

        //    //foreach (Offer offer in offers)
        //    //{
        //    //    string plansQuery = string.Format("CountryCode eq '{0}'", country);
        //    //    List<Plan> plans = marketplaceChannel.ListOfferPlans(offer.Id.ToString(), plansQuery);

        //    //    if (plans.Count > 0)
        //    //    {
        //    //        result.Add(new WindowsAzureOffer(offer, plans));
        //    //    }
        //    //}

        //    return result;
        //}

        public List<WindowsAzureOffer> GetAvailableWindowsAzureOffers(string countryCode)
        {
            List<WindowsAzureOffer> result = new List<WindowsAzureOffer>();
            List<Offer> windowsAzureOffers = new List<Offer>();
            CatalogServiceContext context = new CatalogServiceContext(new Uri(Resources.MarketplaceEndpoint));
            IQueryable<Offer> offers = from o in context.Offers.Expand("Plans").Expand("Categories")
                         where o.IsAvailableInAzureStores
                         select o;
            //context.Execute(context.Offers.Expand("Plans, Categories")).get

            foreach (Offer offer in offers.AsEnumerable())
            {
                IEnumerable<Plan> validPlans = offer.Plans.Where<Plan>(p => p.CountryCode == countryCode);
                IEnumerable<string> offerLocations = offer.Categories.Select<Category, string>(c => c.Name)
                    .Intersect<string>(SubscriptionLocations);
                result.Add(new WindowsAzureOffer(
                    offer,
                    validPlans,
                    offerLocations.Count() == 0 ? SubscriptionLocations : offerLocations));
            }

            return result;
        }
    }
}
