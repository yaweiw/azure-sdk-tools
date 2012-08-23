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

namespace Microsoft.WindowsAzure.Management.Websites.Test.UnitTests.Utilities
{
    using System;
    using System.Collections.Generic;
    using Management.Test.Tests.Utilities;
    using VisualStudio.TestTools.UnitTesting;
    using Websites.Services;

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

        #region GetWebsiteConfiguration

        public Func<SimpleServiceManagementAsyncResult, WebsiteConfig> GetWebsiteConfigurationThunk { get; set; }

        public IAsyncResult BeginGetWebsiteConfiguration(string subscriptionId, string webspace, string website, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["webspace"] = webspace;
            result.Values["website"] = website;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public WebsiteConfig EndGetWebsiteConfiguration(IAsyncResult asyncResult)
        {
            if (GetWebsiteConfigurationThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                return GetWebsiteConfigurationThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("GetWebsiteConfigurationThunk is not implemented!");
            }

            return default(WebsiteConfig);
        }

        #endregion

        #region NewWebsite

        public Action<SimpleServiceManagementAsyncResult> NewWebsiteThunk { get; set; }

        public IAsyncResult BeginNewWebsite(string subscriptionId, string webspace, Website website, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["webspace"] = webspace;
            result.Values["website"] = website;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndNewWebsite(IAsyncResult asyncResult)
        {
            if (NewWebsiteThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                NewWebsiteThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("NewWebsiteThunk is not implemented!");
            }
        }

        #endregion

        #region UpdateWebsite

        public Action<SimpleServiceManagementAsyncResult> UpdateWebsiteThunk { get; set; }

        public IAsyncResult BeginUpdateWebsite(string subscriptionId, string webspace, string websiteName, Website website, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["webspace"] = webspace;
            result.Values["websiteName"] = websiteName;
            result.Values["website"] = website;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndUpdateWebsite(IAsyncResult asyncResult)
        {
            if (UpdateWebsiteThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                UpdateWebsiteThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("UpdateWebsiteThunk is not implemented!");
            }
        }

        #endregion

        #region GetPublishingUsers

        public Func<SimpleServiceManagementAsyncResult, IList<string>> GetPublishingUsersThunk { get; set; }

        public IAsyncResult BeginGetPublishingUsers(string subscriptionId, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public IList<string> EndGetPublishingUsers(IAsyncResult asyncResult)
        {
            if (GetPublishingUsersThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                return GetPublishingUsersThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("GetPublishingUsersThunk is not implemented!");
            }

            return default(IList<string>);
        }

        #endregion

        #region CreateWebsiteRepository

        public Action<SimpleServiceManagementAsyncResult> CreateWebsiteRepositoryThunk { get; set; }

        public IAsyncResult BeginCreateWebsiteRepository(string subscriptionId, string webspace, string websiteName, AsyncCallback callback, object state)
        {
            SimpleServiceManagementAsyncResult result = new SimpleServiceManagementAsyncResult();
            result.Values["subscriptionId"] = subscriptionId;
            result.Values["webspace"] = webspace;
            result.Values["websiteName"] = websiteName;
            result.Values["callback"] = callback;
            result.Values["state"] = state;
            return result;
        }

        public void EndCreateWebsiteRepository(IAsyncResult asyncResult)
        {
            if (CreateWebsiteRepositoryThunk != null)
            {
                SimpleServiceManagementAsyncResult result = asyncResult as SimpleServiceManagementAsyncResult;
                Assert.IsNotNull(result, "asyncResult was not SimpleServiceManagementAsyncResult!");

                CreateWebsiteRepositoryThunk(result);
            }
            else if (ThrowsIfNotImplemented)
            {
                throw new NotImplementedException("CreateWebsiteRepositoryThunk is not implemented!");
            }
        }

        #endregion

        #endregion
    }
}

