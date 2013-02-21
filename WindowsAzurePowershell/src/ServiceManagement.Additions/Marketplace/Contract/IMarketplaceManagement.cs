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

namespace Microsoft.WindowsAzure.ServiceManagement.Marketplace.Contract
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using Microsoft.WindowsAzure.ServiceManagement.Marketplace.ResourceModel;
    using Microsoft.WindowsAzure.ServiceManagement.Utilities;

    [ServiceContract]
    public interface IMarketplaceManagement
    {
        /// <summary>
        /// Gets available Windows Azure plans.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [ODataBehavior(typeof(Offer))]
        [WebGet(UriTemplate = @"Offers?$filter=IsAvailableInAzureStores eq true")]
        IAsyncResult BeginListWindowsAzureOffers(AsyncCallback callback, object state);

        List<Offer> EndListWindowsAzureOffers(IAsyncResult asyncResult);

        /// <summary>
        /// Gets offer plans
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [ODataBehavior(typeof(Plan))]
        [WebGet(UriTemplate = @"Offers(guid'{Id}')/Plans?$filter={query}")]
        IAsyncResult BeginListOfferPlans(string Id, string query, AsyncCallback callback, object state);

        List<Plan> EndListOfferPlans(IAsyncResult asyncResult);
    }
}
