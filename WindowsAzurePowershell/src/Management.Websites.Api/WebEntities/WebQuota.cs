//------------------------------------------------------------------------------
// <copyright file="WebQuota.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public enum TimeUnits
    {
        [EnumMember]
        Days = 0,
        [EnumMember]
        Minutes = 1,
        [EnumMember]
        Hours = 2,
        [EnumMember]
        Months = 3
    }

    [DataContract(Namespace = UriElements.ServiceNamespace, Name = "ExceededAction")]
    public enum ExceededAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Stop = 1,
        [EnumMember]
        RunLimited = 2,
        [EnumMember]
        Redirect = 3
    }

    [DataContract(Namespace = UriElements.ServiceNamespace, Name = "EnforcementScope")]
    public enum EnforcementScope
    {
        [EnumMember]
        WebSpace = 0,
        [EnumMember]
        Site = 1
    }

    /// <summary>
    /// Class that represents a Web Quota.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class WebQuota
    {
        [DataMember(IsRequired = false)]
        public string WebPlan { get; set; }

        [DataMember(IsRequired = false)]
        public ComputeModeOptions ComputeMode { get; set; }

        [DataMember(IsRequired = false)]
        public string QuotaName { get; set; }

        [DataMember(IsRequired = false)]
        public string ResourceName { get; set; }

        [DataMember(IsRequired = false)]
        public long? Limit { get; set; }

        [DataMember(IsRequired = false)]
        public TimeUnits? Unit { get; set; }

        [DataMember(IsRequired = false)]
        public int? Period { get; set; }

        [DataMember(IsRequired = false)]
        public ExceededAction? ExceededAction { get; set; }

        [DataMember(IsRequired = false)]
        public string CustomActionName { get; set; }

        [DataMember(IsRequired = false)]
        public string SiteMode { get; set; }

        [DataMember(IsRequired = false)]
        public EnforcementScope EnforcementScope { get; set; }
    }


    /// <summary>
    /// Collection of webquotas
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class WebQuotas : List<WebQuota>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public WebQuotas() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="sites"></param>
        public WebQuotas(List<WebQuota> webquotas) : base(webquotas) { }
    }
}
