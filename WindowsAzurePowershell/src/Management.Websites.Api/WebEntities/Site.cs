//------------------------------------------------------------------------------
// <copyright file="Site.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Name = "UsageState", Namespace = UriElements.ServiceNamespace)]
    public enum UsageState
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        Exceeded = 1,
    }

    [DataContract(Name = "SiteAvailabilityState", Namespace = UriElements.ServiceNamespace)]
    public enum SiteAvailabilityState
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        Limited = 1,
    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Site
    {
        [DataMember(IsRequired = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false)]
        public string State { get; set; }

        [DataMember(IsRequired = false)]
        public string[] HostNames { get; set; }

        [DataMember(IsRequired = false)]
        public string WebSpace { get; set; }

        [DataMember(IsRequired = false)]
        public Uri SelfLink { get; set; }

        [DataMember(IsRequired = false)]
        public string RepositorySiteName { get; set; }

        [DataMember(IsRequired = false)]
        public string Owner { get; set; }

        [DataMember(IsRequired = false)]
        public UsageState UsageState { get; set; }

        [DataMember(IsRequired = false)]
        public bool? Enabled { get; set; }

        [DataMember(IsRequired = false)]
        public bool? AdminEnabled { get; set; }

        [DataMember(IsRequired = false)]
        public string[] EnabledHostNames { get; set; }

        [DataMember(IsRequired = false)]
        public SiteProperties SiteProperties { get; set; }

        [DataMember(IsRequired = false)]
        public SiteAvailabilityState AvailabilityState { get; set; }

        [DataMember(IsRequired = false)]
        public Certificate[] SSLCertificates { get; set; }

        [DataMember(IsRequired = false)]
        public string SiteMode { get; set; }

        [DataMember(IsRequired = false)]
        public HostNameSslStates HostNameSslStates { get; set; }
    }

    [DataContract(Namespace = UriElements.ServiceNamespace, Name = "Site")]
    public class SiteWithWebSpace : Site
    {
        [DataMember(IsRequired = false)]
        public WebSpace WebSpaceToCreate { get; set; }
    }

    [DataContract(Namespace = UriElements.ServiceNamespace, Name = "Site")]
    public class SiteWithDetails : Site
    {
        [DataMember(IsRequired = false)]
        public string UNCPath { get; set; }

        [DataMember(IsRequired = false)]
        public int? NumberOfWorkers { get; set; }

        [DataMember(IsRequired = false)]
        public Servers RunningWorkers { get; set; }

        [DataMember(IsRequired = false)]
        public string Subscription { get; set; }
    }

    /// <summary>
    /// Collection of sites
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Sites : List<Site>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Sites() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="sites"></param>
        public Sites(List<Site> sites) : base(sites) { }
    }

    /// <summary>
    /// Paged collection of sites
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class PagedSites : PagedSet<SiteWithDetails>
    {
        public PagedSites() : base() { }
    }
}