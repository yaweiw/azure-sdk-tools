// ----------------------------------------------------------------------------------
//
// Copyright 2011 Microsoft Corporation
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

namespace Microsoft.WindowsAzure.Management.WebSites.Test.UnitTests.Utilities
{
    using System;
    using Management.Test.Tests.Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Services;

    /// <summary>
    /// Simple implementation of the <see cref="IWebsitesServiceManagement"/> interface that can be
    /// used for mocking basic interactions without involving Azure directly.
    /// </summary>
    public class SimpleWebsitesManagement : IWebsitesServiceManagement
    {
        /// <summary>
        /// Gets or sets a value indicating whether the thunk wrappers will
        /// throw an exception if the thunk is not implemented.  This is useful
        /// when debugging a test.
        /// </summary>
        public bool ThrowsIfNotImplemented { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleWebsitesManagement"/> class.
        /// </summary>
        public SimpleWebsitesManagement()
        {
            ThrowsIfNotImplemented = true;
        }

        #region Implementation Thunks

        #region GetWebspaces

        public Func<SimpleServiceManagementAsyncResult, WebspaceList> GetWebspacesThunk { get; set; }

        public IAsyncResult BeginGetWebspaces(string subscriptionId, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public WebspaceList EndGetWebspaces(IAsyncResult asyncResult)
        {
            if (GetWebspacesThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                return GetWebspacesThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("GetWebspacesThunk is not implemented!");
            }

            return default(WebspaceList);
        }

        #endregion

        #region GetWebsites

        public Func<SimpleServiceManagementAsyncResult, WebsiteList> GetWebsitesThunk { get; set; }

        public IAsyncResult BeginGetWebsites(string subscriptionId, string webspace, string propertiesToInclude, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["webspace"] = webspace;
            result.Values["propertiesToInclude"] = propertiesToInclude;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public WebsiteList EndGetWebsites(IAsyncResult asyncResult)
        {
            if (GetWebspacesThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                return GetWebsitesThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("GetWebsitesThunk is not implemented!");
            }

            return default(WebsiteList);
        }

        #endregion

        #region DeleteWebsite

        public Action<SimpleServiceManagementAsyncResult> DeleteWebsiteThunk { get; set; }

        public IAsyncResult BeginDeleteWebsite(string subscriptionId, string webspace, string website, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["webspace"] = webspace;
            result.Values["website"] = website;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndDeleteWebsite(IAsyncResult asyncResult)
        {
            if (DeleteWebsiteThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                DeleteWebsiteThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("DeleteWebsiteThunk is not implemented!");
            }
        }

        #endregion

        public IAsyncResult BeginGetWebsiteConfiguration(string subscriptionId, string webspace, string website, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public WebsiteConfig EndGetWebsiteConfiguration(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginGetPublishingUsers(string subscriptionId, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public Website EndGetPublishingUsers(IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }
     
        #endregion
    }
}

