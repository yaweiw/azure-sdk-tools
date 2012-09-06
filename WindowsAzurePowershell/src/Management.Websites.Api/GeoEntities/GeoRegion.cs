//------------------------------------------------------------------------------
// <copyright file="GeoRegion.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.GeoEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Web.Hosting.Administration;

    [DataContract]
    public class GeoRegion
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int? SortOrder { get; set; }
    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class GeoRegions : List<GeoRegion>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public GeoRegions() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="plans"></param>
        public GeoRegions(List<GeoRegion> regions) : base(regions) { }
    }
}
