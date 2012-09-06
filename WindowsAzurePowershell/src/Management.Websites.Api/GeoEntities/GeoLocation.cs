//------------------------------------------------------------------------------
// <copyright file="GeoLocation.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.GeoEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Web.Hosting.Administration;

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class GeoLocation
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

    }

    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class GeoLocations : List<GeoLocation>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public GeoLocations() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="plans"></param>
        public GeoLocations(List<GeoLocation> locations) : base(locations) { }
    }
}
