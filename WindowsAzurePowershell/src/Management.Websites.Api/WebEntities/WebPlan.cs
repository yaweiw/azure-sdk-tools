//------------------------------------------------------------------------------
// <copyright file="WebPlan.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;

    /// <summary>
    /// Class that represents a Web Plan.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class WebPlan
    {
        /// <summary>
        /// Name for the web system
        /// </summary>
        [DataMember]
        public string WebSystem { get; set; }

        [DataMember]
        public bool? VirtualDedicatedEnabled { get; set; }
    }
}
