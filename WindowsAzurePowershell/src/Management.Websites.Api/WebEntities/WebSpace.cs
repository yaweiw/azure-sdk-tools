//------------------------------------------------------------------------------
// <copyright file="WebSpace.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    [DataContract(Name = "Status", Namespace = UriElements.ServiceNamespace)]
    public enum StatusOptions
    {
        [EnumMember]
        Ready = 0,
        [EnumMember]
        Pending = 1,
    }


    [DataContract(Name = "ComputeMode", Namespace = UriElements.ServiceNamespace)]
    public enum ComputeModeOptions
    {
        [EnumMember]
        Shared = 0,
        [EnumMember]
        Dedicated = 1
    }

    [DataContract(Name = "WorkerSize", Namespace = UriElements.ServiceNamespace)]
    public enum WorkerSizeOptions
    {
        [EnumMember]
        Small = 0,
        [EnumMember]
        Medium = 1,
        [EnumMember]
        Large = 2
    }

    [DataContract(Name = "WebSpaceAvailabilityState", Namespace = UriElements.ServiceNamespace)]
    public enum WebSpaceAvailabilityState
    {
        [EnumMember]
        Normal = 0,
        [EnumMember]
        Limited = 1,
    }

    /// <summary>
    /// WebSpace
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class WebSpace
    {
        [DataMember(IsRequired = false)]
        public string Name { get; set; }

        [DataMember(IsRequired = false)]
        public string Plan { get; set; }

        [DataMember(IsRequired = false)]
        public string Subscription { get; set; }

        [DataMember(IsRequired = false)]
        public string GeoLocation { get; set; }

        [DataMember(IsRequired = false)]
        public string GeoRegion { get; set; }

        [DataMember(IsRequired = false)]
        public ComputeModeOptions? ComputeMode { get; set; }

        [DataMember(IsRequired = false)]
        public WorkerSizeOptions? WorkerSize { get; set; }

        [DataMember(IsRequired = false)]
        public int? NumberOfWorkers { get; set; }

        [DataMember(IsRequired = false)]
        public WorkerSizeOptions? CurrentWorkerSize { get; set; }

        [DataMember(IsRequired = false)]
        public int? CurrentNumberOfWorkers { get; set; }

        [DataMember(IsRequired = false)]
        public StatusOptions Status { get; set; }

        [DataMember(IsRequired = false)]
        public WebSpaceAvailabilityState AvailabilityState { get; set; }
    }

    /// <summary>
    /// Collection of webspaces
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class WebSpaces : List<WebSpace>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public WebSpaces() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="sites"></param>
        public WebSpaces(List<WebSpace> webspaces) : base(webspaces) { }
    }
}
