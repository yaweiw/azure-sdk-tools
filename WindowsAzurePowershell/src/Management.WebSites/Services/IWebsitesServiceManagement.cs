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

namespace Microsoft.WindowsAzure.Management.Websites.Services
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Web;
    using System.ServiceModel;
    using System.Xml.Serialization;
    using Utilities;
    using WebEntities;

    [XmlRootAttribute(ElementName = "Error", Namespace = UriElements.ServiceNamespace)]
    public class ServiceError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string ExtendedCode { get; set; }
        public string MessageTemplate { get; set; }

        [XmlArray("Parameters")]
        [XmlArrayItem(typeof(string), Namespace="http://schemas.microsoft.com/2003/10/Serialization/Arrays")]
        public List<string> Parameters { get; set; }
    }

    /// <summary>
    /// Provides the Windows Azure Service Management Api for Windows Azure Websites. 
    /// </summary>
    [ServiceContract(Namespace = UriElements.ServiceNamespace)]
    public interface IWebsitesServiceManagement
    {
        /// <summary>
        /// Gets the list of created webspaces.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/")]
        IAsyncResult BeginGetWebspaces(string subscriptionId, AsyncCallback callback, object state);
        WebSpaces EndGetWebspaces(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the list of created websites in a webspace.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/?propertiesToInclude={propertiesToInclude}")]
        IAsyncResult BeginGetWebsites(string subscriptionId, string webspace, string propertiesToInclude, AsyncCallback callback, object state);
        Sites EndGetWebsites(IAsyncResult asyncResult);

        /// <summary>
        /// Gets a website.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{website}?propertiesToInclude={propertiesToInclude}")]
        IAsyncResult BeginGetWebsite(string subscriptionId, string webspace, string website, string propertiesToInclude, AsyncCallback callback, object state);
        Site EndGetWebsite(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the site configuration.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{website}/config")]
        IAsyncResult BeginGetWebsiteConfiguration(string subscriptionId, string webspace, string website, AsyncCallback callback, object state);
        SiteConfig EndGetWebsiteConfiguration(IAsyncResult asyncResult);

        /// <summary>
        /// Deletes a site.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "DELETE", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{website}")]
        IAsyncResult BeginDeleteWebsite(string subscriptionId, string webspace, string website, AsyncCallback callback, object state);
        void EndDeleteWebsite(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the list of publishing users.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/?properties=publishingUsers")]
        IAsyncResult BeginGetPublishingUsers(string subscriptionId, AsyncCallback callback, object state);
        IList<string> EndGetPublishingUsers(IAsyncResult asyncResult);

        /// <summary>
        /// Create a new website.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites")]
        IAsyncResult BeginNewWebsite(string subscriptionId, string webspace, Site website, AsyncCallback callback, object state);
        void EndNewWebsite(IAsyncResult asyncResult);

        /// <summary>
        /// Update a website.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{websiteName}")]
        IAsyncResult BeginUpdateWebsite(string subscriptionId, string webspace, string websiteName, Site website, AsyncCallback callback, object state);
        void EndUpdateWebsite(IAsyncResult asyncResult);

        /// <summary>
        /// Update a website repository.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "POST", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{websiteName}/repository")]
        IAsyncResult BeginCreateWebsiteRepository(string subscriptionId, string webspace, string websiteName, AsyncCallback callback, object state);
        void EndCreateWebsiteRepository(IAsyncResult asyncResult);
    }
}
