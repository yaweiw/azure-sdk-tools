//------------------------------------------------------------------------------
// <copyright file="HostNameSslState.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;

    /// <summary>
    /// Class that represents a SSL-enabled host name.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class HostNameSslState
    {

        /// <summary>
        /// Host name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// SSL type
        /// </summary>
        [DataMember]
        public SslState SslState { get; set; }

    }

    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public enum SslState
    {
        [EnumMember]
        Disabled = 0,
        [EnumMember]
        SniEnabled = 1,
        [EnumMember]
        IpBasedEnabled = 2
    }

    /// <summary>
    /// Collection of SSL-enabled host names
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class HostNameSslStates : List<HostNameSslState>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public HostNameSslStates() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="sites"></param>
        public HostNameSslStates(List<HostNameSslState> list) : base(list) { }
    }
}
