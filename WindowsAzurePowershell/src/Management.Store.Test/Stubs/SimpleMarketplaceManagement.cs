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

namespace Microsoft.WindowsAzure.Management.Store.Test.Stubs
{
    using System;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.ServiceManagement.Marketplace.Contract;
    using Microsoft.WindowsAzure.ServiceManagement.Marketplace.ResourceModel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.Management.Test.Tests.Utilities;

    class SimpleMarketplaceManagement : IMarketplaceManagement
    {
        /// <summary>
        /// Gets or sets a value indicating whether the thunk wrappers will
        /// throw an exception if the thunk is not implemented.  This is useful
        /// when debugging a test.
        /// </summary>
        public bool ThrowsIfNotImplemented { get; set; }

        /// <summary>
        /// Initializes a new instance of the SimpleMarketplaceManagement class.
        /// </summary>
        public SimpleMarketplaceManagement()
        {
            ThrowsIfNotImplemented = true;
        }

        public Func<SimpleServiceManagementAsyncResult, List<Offer>> ListWindowsAzureOffersThunk { get; set; }
        public IAsyncResult BeginListWindowsAzureOffers(AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["callback"] = callback;
            result.Values["state"] = state;

            return result;
        }

        public List<Offer> EndListWindowsAzureOffers(IAsyncResult asyncResult)
        {
            List<Offer> offers = new List<Offer>();

            if (ListWindowsAzureOffersThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                offers = ListWindowsAzureOffersThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("ListOffersThunk is not implemented!");
            }

            return offers;
        }

        public Func<SimpleServiceManagementAsyncResult, List<Plan>> ListOfferPlansThunk { get; set; }
        public IAsyncResult BeginListOfferPlans(string Id, string query, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["Id"] = Id;
            result.Values["query"] = query;
            result.Values["callback"] = callback;
            result.Values["state"] = state;

            return result;
        }

        public List<Plan> EndListOfferPlans(IAsyncResult asyncResult)
        {
            List<Plan> OfferPlans = new List<Plan>();

            if (ListOfferPlansThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                OfferPlans = ListOfferPlansThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("ListOfferPlansThunk is not implemented!");
            }

            return OfferPlans;
        }
    }
}
