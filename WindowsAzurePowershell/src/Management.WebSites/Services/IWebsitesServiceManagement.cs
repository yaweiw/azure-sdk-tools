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
    using System.Runtime.Serialization;
    using System.ServiceModel.Web;
    using System.ServiceModel;
    using System.Xml.Serialization;
    using Utilities;

    [XmlRootAttribute(ElementName = "Error", Namespace = Constants.ServiceManagementNS)]
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
    /// A list of webspaces.
    /// </summary>
    [CollectionDataContract(Name = "WebSpaces", ItemName = "WebSpace", Namespace = Constants.ServiceManagementNS)]
    public class WebspaceList : List<Webspace>
    {
        public WebspaceList() { }

        public WebspaceList(IEnumerable<Webspace> webspaces) : base(webspaces) { }
    }

    /// <summary>
    /// A website site properties.
    /// </summary>
    [DataContract(Name = "NameValuePair", Namespace = Constants.ServiceManagementNS)]
    public class NameValuePair
    {
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Value { get; set; }
    }

    /// <summary>
    /// A website site properties.
    /// </summary>
    [DataContract(Name = "SiteProperties", Namespace = Constants.ServiceManagementNS)]
    public class WebsiteSiteProperties
    {
        [DataMember(EmitDefaultValue = false)]
        public object Metadata { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IList<NameValuePair> Properties { get; set; }
    }

    /// <summary>
    /// A webspace.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Webspace
    {
        [DataMember(EmitDefaultValue = false)]
        public string AvailabilityState { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ComputeMode { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string CurrentNumberOfWorkers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string CurrentWorkerSize { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string GeoLocation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string GeoRegion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string NumberOfWorkers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Plan { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Status { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Subscription { get; set; }
    }

    /// <summary>
    /// A list of websites.
    /// </summary>
    [CollectionDataContract(Name = "Sites", ItemName = "Site", Namespace = Constants.ServiceManagementNS)]
    public class WebsiteList : List<Website>
    {
        public WebsiteList() { }
        public WebsiteList(IEnumerable<Website> websites) : base(websites) { }
    }

    /// <summary>
    /// A website.
    /// </summary>
    [DataContract(Name = "Site", Namespace = Constants.ServiceManagementNS)]
    public class Website
    {
        [DataMember(EmitDefaultValue = false)]
        public bool AdminEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string AvailabilityState { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IList<string> EnabledHostNames { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IList<string> HostNames { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Owner { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string RepositorySiteName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SelfLink { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public WebsiteSiteProperties SiteProperties { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string State { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UsageState { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string WebSpace { get; set; }
    }

    /// <summary>
    /// A website.
    /// </summary>
    [DataContract(Name = "SiteConfig", Namespace = Constants.ServiceManagementNS)]
    public class WebsiteConfig
    {
        [DataMember(EmitDefaultValue = false)]
        public bool DetailedErrorLoggingEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool HttpLoggingEnabled { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IList<string> Metadata { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string NetFrameworkVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int NumberOfWorkers { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PhpVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PublishingPassword { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PublishingUsername { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool RequestTracingEnabled { get; set; }

        [IgnoreDataMember]
        public bool AdminEnabled { get; set; }

        [IgnoreDataMember]
        public string AvailabilityState { get; set; }

        [IgnoreDataMember]
        public IList<string> EnabledHostNames { get; set; }

        [IgnoreDataMember]
        public IList<string> HostNames { get; set; }

        [IgnoreDataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public string Owner { get; set; }

        [IgnoreDataMember]
        public string RepositorySiteName { get; set; }

        // Website properties
        [IgnoreDataMember]
        public string SelfLink { get; set; }

        [IgnoreDataMember]
        public WebsiteSiteProperties SiteProperties { get; set; }

        [IgnoreDataMember]
        public string State { get; set; }

        [IgnoreDataMember]
        public string UsageState { get; set; }

        [IgnoreDataMember]
        public string WebSpace { get; set; }

        public void Merge(Website website)
        {
            AdminEnabled = website.AdminEnabled;
            AvailabilityState = website.AvailabilityState;
            EnabledHostNames = website.EnabledHostNames;
            HostNames = website.HostNames;
            Name = website.Name;
            Owner = website.Owner;
            RepositorySiteName = website.RepositorySiteName;
            SelfLink = website.SelfLink;
            SiteProperties = website.SiteProperties;
            State = website.State;
            UsageState = website.UsageState;
            WebSpace = website.WebSpace;
        }
    }

    /// <summary>
    /// Provides the Windows Azure Service Management Api for Windows Azure Websites. 
    /// </summary>
    [ServiceContract(Namespace = Constants.ServiceManagementNS)]
    public interface IWebsitesServiceManagement
    {
        /// <summary>
        /// Gets the list of created webspaces.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/")]
        IAsyncResult BeginGetWebspaces(string subscriptionId, AsyncCallback callback, object state);
        WebspaceList EndGetWebspaces(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the list of created websites in a webspace.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/?propertiesToInclude={propertiesToInclude}")]
        IAsyncResult BeginGetWebsites(string subscriptionId, string webspace, string propertiesToInclude, AsyncCallback callback, object state);
        WebsiteList EndGetWebsites(IAsyncResult asyncResult);

        /// <summary>
        /// Gets a website.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{website}?propertiesToInclude={propertiesToInclude}")]
        IAsyncResult BeginGetWebsite(string subscriptionId, string webspace, string website, string propertiesToInclude, AsyncCallback callback, object state);
        Website EndGetWebsite(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the site configuration.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "GET", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{website}/config")]
        IAsyncResult BeginGetWebsiteConfiguration(string subscriptionId, string webspace, string website, AsyncCallback callback, object state);
        WebsiteConfig EndGetWebsiteConfiguration(IAsyncResult asyncResult);

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
        IAsyncResult BeginNewWebsite(string subscriptionId, string webspace, Website website, AsyncCallback callback, object state);
        void EndNewWebsite(IAsyncResult asyncResult);

        /// <summary>
        /// Update a website.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(Method = "PUT", UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{websiteName}")]
        IAsyncResult BeginUpdateWebsite(string subscriptionId, string webspace, string websiteName, Website website, AsyncCallback callback, object state);
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
