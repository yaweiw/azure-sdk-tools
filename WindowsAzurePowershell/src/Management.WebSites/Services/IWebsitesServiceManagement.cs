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

namespace Microsoft.WindowsAzure.Management.WebSites.Services
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.ServiceModel.Web;
    using System.ServiceModel;
    using Utilities;

    /// <summary>
    /// A list of webspaces.
    /// </summary>
    [CollectionDataContract(Name = "WebSpaces", ItemName = "WebSpace", Namespace = Constants.ServiceManagementNS)]
    public class WebspaceList : List<WebSpace>
    {
        public WebspaceList() { }

        public WebspaceList(IEnumerable<WebSpace> webspaces) : base(webspaces) { }
    }

    /// <summary>
    /// A website site properties.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class WebsiteSiteProperties
    {
        [DataMember(Order = 1)]
        public object Metadata { get; set; }

        [DataMember(Order = 2)]
        public Dictionary<string, string> Properties { get; set; }
    }

    /// <summary>
    /// A webspace.
    /// </summary>
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class WebSpace : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public string AvailabilityState { get; set; }

        [DataMember(Order = 2)]
        public string ComputeMode { get; set; }

        [DataMember(Order = 3)]
        public string CurrentNumberOfWorkers { get; set; }

        [DataMember(Order = 4)]
        public string CurrentWorkerSize { get; set; }

        [DataMember(Order = 5)]
        public string GeoLocation { get; set; }

        [DataMember(Order = 6)]
        public string GeoRegion { get; set; }

        [DataMember(Order = 7)]
        public string Name { get; set; }

        [DataMember(Order = 8)]
        public string NumberOfWorkers { get; set; }

        [DataMember(Order = 9)]
        public string Plan { get; set; }

        [DataMember(Order = 10)]
        public string Status { get; set; }

        [DataMember(Order = 11)]
        public string Subscription { get; set; }
        
        public ExtensionDataObject ExtensionData { get; set; }
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
    [DataContract(Namespace = Constants.ServiceManagementNS)]
    public class Website : IExtensibleDataObject
    {
        [DataMember(Order = 1)]
        public bool AdminEnabled { get; set; }

        [DataMember(Order = 2)]
        public string AvailabilityState { get; set; }

        [DataMember(Order = 3)]
        public IList<string> EnabledHostNames { get; set; }

        [DataMember(Order = 4)]
        public IList<string> HostNames { get; set; }

        [DataMember(Order = 5)]
        public string Name { get; set; }

        [DataMember(Order = 6)]
        public string Owner { get; set; }

        [DataMember(Order = 7)]
        public string RepositorySiteName { get; set; }

        [DataMember(Order = 8)]
        public string SelfLink { get; set; }

        [DataMember(Order = 9)]
        public WebsiteSiteProperties SiteProperties { get; set; }

        [DataMember(Order = 10)]
        public string State { get; set; }

        [DataMember(Order = 11)]
        public string UsageState { get; set; }

        [DataMember(Order = 12)]
        public string WebSpace { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }

    /// <summary>
    /// Provides the Windows Azure Service Management Api for Windows Azure Websites. 
    /// </summary>
    [ServiceContract(Namespace = Constants.ServiceManagementNS)]
    public partial interface IWebsitesServiceManagement
    {
        /// <summary>
        /// Gets the list of created webspaces.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/webspaces/")]
        IAsyncResult BeginGetWebspaces(string subscriptionId, AsyncCallback callback, object state);
        WebspaceList EndGetWebspaces(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the list of created websites in a webspace.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/?propertiesToInclude={propertiesToInclude}")]
        IAsyncResult BeginGetWebsites(string subscriptionId, string webspace, string propertiesToInclude, AsyncCallback callback, object state);
        WebsiteList EndGetWebsites(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the site configuration.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/webspaces/{webspace}/sites/{website}/config")]
        IAsyncResult BeginGetWebsiteConfiguration(string subscriptionId, string webspace, string website, AsyncCallback callback, object state);
        Website EndGetWebsiteConfiguration(IAsyncResult asyncResult);

        /// <summary>
        /// Gets the list of publishing users.
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = @"{subscriptionId}/services/webspaces/?properties=publishingUsers")]
        IAsyncResult BeginGetPublishingUsers(string subscriptionId, AsyncCallback callback, object state);
        Website EndGetPublishingUsers(IAsyncResult asyncResult);
    }
}
