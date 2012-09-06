//------------------------------------------------------------------------------
// <copyright file="QuotaUsage.cs" company="Microsoft">
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

    /// <summary>
    /// Class that represents usage of the quota resource.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Usage
    {
        /// <summary>
        /// Name of the quota 
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Name of the quota resource
        /// </summary>
        [DataMember]
        public string ResourceName { get; set; }

        /// <summary>
        /// Units of measurement for the quota resource
        /// </summary>
        [DataMember]
        public string Unit { get; set; }

        /// <summary>
        /// The current value of the resource counter
        /// </summary>
        [DataMember]
        public long CurrentValue { get; set; }

        /// <summary>
        /// The resource limit
        /// </summary>
        [DataMember]
        public long Limit { get; set; }

        /// <summary>
        /// Next reset time for the resource counter
        /// </summary>
        [DataMember]
        public DateTime NextResetTime { get; set; }

        /// <summary>
        /// ComputeMode used for this usage
        /// </summary>
        [DataMember]
        public ComputeModeOptions? ComputeMode { get; set; }

        /// <summary>
        /// SiteMode used for this usage
        /// </summary>
        [DataMember]
        public string SiteMode { get; set; }

    }

    /// <summary>
    /// Collection of usage
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Usages : List<Usage>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Usages() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="plans"></param>
        public Usages(List<Usage> usages) : base(usages) { }
    }
}
