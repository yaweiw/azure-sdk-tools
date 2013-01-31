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

namespace Microsoft.Samples.WindowsAzure.ServiceManagement.Marketplace.ResourceModel
{
    using System;
    using Microsoft.Data.OData;
    using Microsoft.Samples.WindowsAzure.ServiceManagement.ResourceModel;

    public class Plan : IODataResolvable
    {
        public Guid Id { get; set; }

        public string PlanName { get; set; }

        public string PlanType { get; set; }

        public long TransactionLimit { get; set; }

        public string TransactionUnit { get; set; }

        public decimal Price { get; set; }

        public string MarketName { get; set; }

        public string CountryCode { get; set; }

        public string CurrencyCode { get; set; }

        public string Description { get; set; }

        public string PlanIdentifier { get; set; }

        public bool IsPromoCodeRequired { get; set; }

        public void Resolve(ODataEntry entry)
        {
            Id = entry.GetPropetyValue<Guid>("Id");

            PlanName = entry.GetPropetyValue<string>("PlanName");

            PlanType = entry.GetPropetyValue<string>("PlanType");

            TransactionLimit = entry.GetPropetyValue<long>("TransactionLimit");

            TransactionUnit = entry.GetPropetyValue<string>("TransactionUnit");

            Price = entry.GetPropetyValue<decimal>("Price");

            MarketName = entry.GetPropetyValue<string>("MarketName");

            CountryCode = entry.GetPropetyValue<string>("CountryCode");

            CurrencyCode = entry.GetPropetyValue<string>("CurrencyCode");

            Description = entry.GetPropetyValue<string>("Description");

            PlanIdentifier = entry.GetPropetyValue<string>("PlanIdentifier");

            IsPromoCodeRequired = entry.GetPropetyValue<bool>("IsPromoCodeRequired");
        }
    }
}