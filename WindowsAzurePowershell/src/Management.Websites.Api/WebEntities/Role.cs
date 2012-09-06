//------------------------------------------------------------------------------
// <copyright file="Role.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Management.Websites.Api.WebEntities
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Management.Websites.Api;
    using Web.Hosting.Administration;

    // Role is a user friendly way of grouping together a set of permissions and their scopes
    [DataContract(Name = "Role", Namespace = UriElements.ServiceNamespace)]
    public class Role
    {
        [DataMember(IsRequired = true)]
        public string Name { get; set; }

        [DataMember(IsRequired = false)]
        public string Description { get; set; }

        [DataMember(IsRequired = false)]
        public PermissionScopePairs Permissions { get; set; }
    }

    /// <summary>
    /// Collection of Roles
    /// </summary>
    [CollectionDataContract(Namespace = UriElements.ServiceNamespace)]
    public class Roles : List<Role>
    {

        /// <summary>
        /// Empty collection
        /// </summary>
        public Roles(){ }

        /// <summary>
        /// Initialize collection
        /// </summary>
        /// <param name="roles"></param>
        public Roles(List<Role> roles) : base(roles) { }
    }
}
