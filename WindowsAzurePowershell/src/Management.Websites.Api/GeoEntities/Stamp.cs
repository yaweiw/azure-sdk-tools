//------------------------------------------------------------------------------
// <copyright file="Stamp.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.GeoEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Web.Hosting.Administration;
    using WebEntities;

    [DataContract(Name = "StampState", Namespace = UriElements.ServiceNamespace)]
    public enum StampState
    {
        [EnumMember]
        Online = 0,
        [EnumMember]
        Full = 1,
        [EnumMember]
        Unhealthy = 2,
        [EnumMember]
        Preparing = 3,
    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Stamp
    {

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string GeoLocation { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string ServiceAddress { get; set; }

        [DataMember]
        public StampState State { get; set; }

        [DataMember]
        public StampCapacities Capacities { get; set; }
    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Stamps : List<Stamp>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Stamps() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="plans"></param>
        public Stamps(List<Stamp> stamps) : base(stamps) { }
    }
}

