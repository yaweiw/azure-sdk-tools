//------------------------------------------------------------------------------
// <copyright file="WebSystemSummary.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;

    /// <summary>
    /// Class that represents a Web System summary.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class WebSystemSummary
    {
        /// <summary>
        /// Number of web workers
        /// </summary>
        [DataMember]
        public int NumberOfWebWorkers { get; set; }

        /// <summary>
        /// Number of publishers
        /// </summary>
        [DataMember]
        public int NumberOfPublishers { get; set; }

        /// <summary>
        /// Number of load balancers
        /// </summary>
        [DataMember]
        public int NumberOfLoadBalancers { get; set; }

        /// <summary>
        /// Number of controllers
        /// </summary>
        [DataMember]
        public int NumberOfControllers { get; set; }

        /// <summary>
        /// Number of active websites
        /// </summary>
        [DataMember]
        public int NumberOfActiveWebsites { get; set; }

    }
}
