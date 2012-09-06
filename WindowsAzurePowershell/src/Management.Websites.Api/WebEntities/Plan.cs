//------------------------------------------------------------------------------
// <copyright file="Plan.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;

    /// <summary>
    /// Class that represents a hosting Plan.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class Plan
    {
        /// <summary>
        /// Name for the plan
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// The description for the plan
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Is the plan active
        /// </summary>
        [DataMember]
        public bool? Active { get; set; }
    }

    /// <summary>
    /// Collection of plans
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Plans : List<Plan>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Plans() { }

        /// <summary>
        /// Initialize from list
        /// </summary>
        /// <param name="plans"></param>
        public Plans(List<Plan> plans) : base(plans) { }
    }
}
