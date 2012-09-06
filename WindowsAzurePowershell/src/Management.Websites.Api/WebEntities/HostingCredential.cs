//------------------------------------------------------------------------------
// <copyright file="HostingCredential.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Utilities;

    /// <summary>
    /// Class that represents a system credential.
    /// </summary>
    [DataContract(Namespace = UriElements.ServiceNamespace)]
    public class HostingCredential
    {
        /// <summary>
        /// Username 
        /// </summary>
        [DataMember]
        public string CredentialName { get; set; }

        /// <summary>
        /// Username 
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// Password for the account
        /// </summary>
        [DataMember]
        [PIIValue]
        public string Password { get; set; }
    }

    /// <summary>
    /// Collection of servers
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class HostingCredentials : List<HostingCredential>
    {
        
    }
}
