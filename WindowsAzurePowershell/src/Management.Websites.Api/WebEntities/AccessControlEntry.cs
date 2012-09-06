//------------------------------------------------------------------------------
// <copyright file="AccessControlEntry.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Api;

    [DataContract(Name = "AccessControlEntry", Namespace = UriElements.ServiceNamespace)]
    public class AccessControlEntry
    {
        [DataMember(IsRequired = true)]
        public string EntityName { get; set; }

        [DataMember(IsRequired = true)]
        public string UserName { get; set; }

        [DataMember(IsRequired = true)]
        public string RoleName { get; set; }

        [DataMember(IsRequired = false)]
        public string Description { get; set; }
    }

    /// <summary>
    /// Collection of AccessControlEntries
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class AccessControlList : List<AccessControlEntry>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public AccessControlList() { }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="entries"></param>
        public AccessControlList(List<AccessControlEntry> entries) : base(entries) { }
    }
}
