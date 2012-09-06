//------------------------------------------------------------------------------
// <copyright file="SitePolicy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Web.Hosting.Administration;

    /// <summary>
    /// Class that represents a Web Plan's policy.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Policy
    {
        /// <summary>
        /// Name for the plan
        /// </summary>
        [DataMember(Order = 0)]
        public string PlanName { get; set; }

        [DataMember(Order = 1)]
        public ComputeModeOptions ComputeMode { get; set; }

        [DataMember(Order = 2)]
        public string SiteMode { get; set; }

        [DataMember(Order = 3)]
        public SiteProcessSettings SiteProcessSettings { get; set; }

        [DataMember(Order = 4)]
        public Features Features { get; set; }
    }

    /// <summary>
    /// Collection of policies
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Policies : List<Policy>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Policies() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="plans"></param>
        public Policies(List<Policy> policies) : base(policies) { }
    }


    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Features
    {
        [DataMember]
        public bool CustomDomainsEnabled { get; set; }

        [DataMember]
        public bool SniBasedSslEnabled { get; set; }

        [DataMember]
        public bool IpBasedSslEnabled { get; set; }
    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class SiteProcessSettings
    {
        [DataMember]
        public double CpuLimitPercentage { get; set; }

        [DataMember]
        public int CpuLimitPeriodInMinutes { get; set; }

        [DataMember]
        public int CpuLimitAction { get; set; }

        [DataMember]
        public int MemoryLimitInMB { get; set; }

        [DataMember]
        public int MemoryLimitWorkingSetInMB { get; set; }

        [DataMember]
        public int FailProtectionLimit { get; set; }

        [DataMember]
        public int FailProtectionPeriodInSeconds { get; set; }

        [DataMember]
        public int FailProtectionPenaltyPeriodInSeconds { get; set; }

        [DataMember]
        public int IdleTimeoutInMinutes { get; set; }

        [DataMember]
        public int IdlePriority { get; set; }

        [DataMember]
        public int WorkerProcessLimit { get; set; }

        [DataMember]
        public int FastCgiProcessLimit { get; set; }

        [DataMember]
        public int HttpQueueLength { get; set; }
    }
}
