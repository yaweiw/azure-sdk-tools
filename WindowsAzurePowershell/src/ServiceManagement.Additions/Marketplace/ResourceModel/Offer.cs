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

namespace Microsoft.WindowsAzure.ServiceManagement.Marketplace.ResourceModel
{
    using System;
    using Microsoft.Data.OData;
    using Microsoft.WindowsAzure.ServiceManagement.Utilities;

    public class Offer : IODataResolvable
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ShortDescription { get; set; }

        public string Description { get; set; }

        public DateTime PublishDate { get; set; }

        public string IconUrl { get; set; }

        public string MarketplaceDetailUrl { get; set; }

        public string OfferType { get; set; }

        public string ProviderName { get; set; }

        public Guid ProviderId { get; set; }

        public string WebsiteUrl { get; set; }

        public string Country { get; set; }

        public string EulaUrl { get; set; }

        public string PrivacyPolicyUrl { get; set; }

        public string ProviderIdentifier { get; set; }

        public string OfferIdentifier { get; set; }

        public bool IsAvailableInAzureStores { get; set; }

        public void Resolve(ODataEntry entry)
        {
            Id = entry.GetPropetyValue<Guid>("Id");
            
            Name = entry.GetPropetyValue<string>("Name");
            
            ShortDescription = entry.GetPropetyValue<string>("ShortDescription");
            
            Description = entry.GetPropetyValue<string>("Description");

            PublishDate = entry.GetPropetyValue<DateTime>("PublishDate");

            IconUrl = entry.GetPropetyValue<string>("IconUrl");

            MarketplaceDetailUrl = entry.GetPropetyValue<string>("MarketplaceDetailUrl");

            OfferType = entry.GetPropetyValue<string>("OfferType");

            ProviderName = entry.GetPropetyValue<string>("ProviderName");
            
            ProviderId = entry.GetPropetyValue<Guid>("ProviderId");
            
            Country = entry.GetPropetyValue<string>("Country");

            WebsiteUrl = entry.GetPropetyValue<string>("WebsiteUrl");

            EulaUrl = entry.GetPropetyValue<string>("EulaUrl");
            
            PrivacyPolicyUrl = entry.GetPropetyValue<string>("PrivacyPolicyUrl");

            ProviderIdentifier = entry.GetPropetyValue<string>("ProviderIdentifier");

            OfferIdentifier = entry.GetPropetyValue<string>("OfferIdentifier");

            IsAvailableInAzureStores = entry.GetPropetyValue<bool>("IsAvailableInAzureStores");
        }
    }
}
